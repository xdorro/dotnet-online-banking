﻿using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Backend.Hubs
{
    [HubName("chatHub")]
    public class ChatHub : Hub
    {
        public static ConcurrentDictionary<string, string> adminSessions = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, string> userSessions = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, string> channelSessions = new ConcurrentDictionary<string, string>();
        private static List<Accounts> _accounts = new List<Accounts>();
        private static List<Channels> _channels = new List<Channels>();
        private IRepository<Accounts> accountRepo;
        private IRepository<Channels> channelRepo;
        private IRepository<Messages> messageRepo;

        public ChatHub()
        {
            this.accountRepo = new Repository<Accounts>();
            this.channelRepo = new Repository<Channels>();
            this.messageRepo = new Repository<Messages>();
        }

        public void Send(string message)
        {
            if (!Utils.IsNullOrEmpty(message))
            {
                var account = FindAccountByAccountId(GetIntegerAccountId());

                if (!Utils.IsNullOrEmpty(account))
                {
                    UserSendMessage(account, message);
                }
            }
        }

        public void Reply(int channelId, string message)
        {
            if (!Utils.IsNullOrEmpty(message))
            {
                var account = FindAccountByAccountId(GetIntegerAccountId());
                var channel = FindChannelByChannelId(channelId);

                if (!Utils.IsNullOrEmpty(channel))
                {
                    AdminReplyMessage(channel, account, message);
                }
            }
        }

        public void ConnectChannel(string channelId)
        {
            if (!Utils.IsNullOrEmpty(channelId))
            {
                AddChannelSession(channelId, Context.ConnectionId);
            }
        }

        public IEnumerable<Accounts> GetOnlineUsers()
        {
            return _accounts;
        }

        private void sendListOnline()
        {


            Clients.All.onlineList();
        }

        // Handler message from user
        private void UserSendMessage(Accounts account, string message)
        {
            var channel = FindChannelByAccountId(account.AccountId);
            if (Utils.IsNullOrEmpty(channel))
            {
                channel = new Channels();
                channel.AccountId = account.AccountId;
                channelRepo.Add(channel);
            }

            AddChannelSession(account.AccountId.ToString(), Context.ConnectionId);

            var messageObj = new Messages
            {
                AccountId = account.AccountId,
                ChannelId = channel.ChannelId,
                Content = message,
                Timestamp = DateTime.Now
            };

            if (messageRepo.Add(messageObj))
            {
                SendMessageToAdmin(channel.ChannelId, account, messageObj);
            }
        }

        private void AdminReplyMessage(Channels channel, Accounts account, string message)
        {
            if (Utils.IsNullOrEmpty(channel))
            {
                channel = new Channels
                {
                    AccountId = account.AccountId
                };
                channelRepo.Add(channel);
            }

            AddChannelSession(account.AccountId.ToString(), Context.ConnectionId);

            var messageObj = new Messages
            {
                AccountId = account.AccountId,
                ChannelId = channel.ChannelId,
                Content = message,
                Timestamp = DateTime.Now
            };

            if (messageRepo.Add(messageObj))
            {
                SendMessageToAdmin(channel.ChannelId, account, messageObj);
            }
        }

        private Channels FindChannelByChannelId(int channelId)
        {
            return channelRepo.Get(x => x.ChannelId == channelId).FirstOrDefault();
        }

        private Channels FindChannelByAccountId(int accountId)
        {
            return channelRepo.Get(x => x.AccountId == accountId).FirstOrDefault();
        }

        private Accounts FindAccountByAccountId(int accountId)
        {
            return accountRepo.Get(x => x.AccountId == accountId).FirstOrDefault();
        }

        private void SendMessageToAdmin(int channelId, Accounts account, Messages message)
        {
            List<Accounts> _accounts = accountRepo.Get(x => (x.RoleId == 1 || x.RoleId == 2) && x.AccountId != account.AccountId).ToList();
            //Clients.Caller.showErrorMessage("The user is no longer connected.");

            if (!Utils.IsNullOrEmpty(_accounts))
            {
                List<string> adminConnections = new List<string>();
                //var output = new Object { Name = account.Name, Content = message.Content };

                _accounts.ForEach(x =>
                {
                    adminSessions.TryGetValue(x.AccountId.ToString(), out string connectionId);

                    if (!Utils.IsNullOrEmpty(connectionId) && !adminConnections.Contains(connectionId))
                    {
                        adminConnections.Add(connectionId);
                    }
                });

                if (!Utils.IsNullOrEmpty(adminConnections))
                {
                    adminConnections.ForEach(connectionId =>
                    {
                        Clients.Client(connectionId).addNewMessageToPage(account.Name, message.Content);
                        Clients.Client(connectionId).reloadChatData();
                    });
                }
            }

            Clients.Client(Context.ConnectionId).addNewMessageToPage(account.Name, message.Content);
        }

        private void AddConnection(ConcurrentDictionary<string, string> sessions, string connection)
        {
            var accountId = GetAccountId();

            sessions.TryGetValue(accountId, out string existingConnections);

            sessions.TryAdd(accountId, connection);

            //AddChannelSession(accountId, connection);
        }

        private void AddChannelSession(string accountId, string connection)
        {
            RemoveConnection(channelSessions, connection);

            var account = FindAccountByAccountId(int.Parse(accountId));

            if (!Utils.IsNullOrEmpty(account))
            {
                var channel = FindChannelByAccountId(account.AccountId);

                if (!Utils.IsNullOrEmpty(channel))
                {
                    channelSessions.TryGetValue(channel.ChannelId.ToString(), out string existingConnections);

                    channelSessions.TryAdd(channel.ChannelId.ToString(), connection);
                }
            }
        }

        public override Task OnConnected()
        {
            var connection = Context.ConnectionId;

            checkAddSessions(connection);

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            var connection = Context.ConnectionId;

            checkAddSessions(connection);

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var connection = Context.ConnectionId;

            RemoveConnection(adminSessions, connection);
            RemoveConnection(userSessions, connection);
            RemoveConnection(channelSessions, connection);

            return base.OnDisconnected(stopCalled);
        }

        private void checkAddSessions(string connection)
        {
            var account = FindAccountByAccountId(GetIntegerAccountId());

            if (!Utils.IsNullOrEmpty(account))
            {
                if (account.RoleId == 1 || account.RoleId == 2)
                {
                    AddConnection(adminSessions, connection);
                }
                else
                {
                    AddConnection(userSessions, connection);
                }
            }
        }

        private int GetIntegerAccountId()
        {
            return int.Parse(GetAccountId());
        }

        private string GetAccountId()
        {
            return Context.QueryString["userId"].ToString();
        }
        private string GetConnectionByAccountId(ConcurrentDictionary<string, string> sessions, string accountId)
        {
            string connectionAccounts;
            sessions.TryGetValue(accountId, out connectionAccounts);

            return connectionAccounts;
        }

        private List<string> FilterDuplicateConnections(string connections)
        {
            var keys = new List<string>();
            var values = new List<string>();

            //connections.ToList().ForEach(x =>
            //{
            //    var session = GetConnectionKey(allSessions, x);

            //    if (!keys.Contains(session.Key))
            //    {
            //        values.Add(x);
            //    }
            //});

            //connections.Where(x => GetConnectionKey(channelSessions, x).Key == x.Key);

            return values;
        }

        private KeyValuePair<string, string> GetConnectionKey(ConcurrentDictionary<string, string> sessions, string connection)
        {
            return sessions.FirstOrDefault(x => x.Value.Contains(connection));
        }

        private void RemoveConnection(ConcurrentDictionary<string, string> sessions, string connection)
        {
            var account = GetConnectionKey(sessions, connection);

            if (!Utils.IsNullOrEmpty(account))
            {
                var key = account.Key;

                if (!Utils.IsNullOrEmpty(key))
                {
                    sessions.TryRemove(key, out string value);
                }
            }
        }
    }
}