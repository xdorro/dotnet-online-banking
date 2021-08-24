﻿
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace OnlineBanking.DAL.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<OnlineBanking.DAL.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "OnlineBanking.DAL.ApplicationDbContext";
        }

        protected override void Seed(OnlineBanking.DAL.ApplicationDbContext context)
        {
            context.Roles.AddOrUpdate(x => x.RoleId,
                new Roles()
                {
                    RoleId = 1,
                    Name = "Admin",
                    Status = ((int)DefaultStatus.Actived)
                },
                new Roles()
                {
                    RoleId = 2,
                    Name = "Support",
                    Status = ((int)DefaultStatus.Actived)
                },
                new Roles()
                {
                    RoleId = 3,
                    Name = "User",
                    Status = ((int)DefaultStatus.Actived)
                }
            );

            context.Accounts.AddOrUpdate(x => x.AccountId,
                new Accounts()
                {
                    AccountId = 1,
                    Name = "Admin",
                    Email = "admin@gmail.com",
                    Password = "123456",
                    RoleId = 1,
                    Phone = "0973086596",
                    NumberId = "125616121",
                    Status = ((int)AccountStatus.Actived),
                    AttemptLogin = 0,
                },
                new Accounts()
                {
                    AccountId = 2,
                    Name = "Support 1",
                    Email = "support@gmail.com",
                    Password = "123456",
                    Phone = "0973086594",
                    NumberId = "125616125",
                    RoleId = 2,
                    Status = ((int)AccountStatus.Actived),
                    AttemptLogin = 0,
                },
                new Accounts()
                {
                    AccountId = 3,
                    Name = "Support 2",
                    Email = "support2@gmail.com",
                    Password = "123456",
                    Phone = "0973086593",
                    NumberId = "125616124",
                    RoleId = 2,
                    Status = ((int)AccountStatus.Actived),
                    AttemptLogin = 0,
                },
                new Accounts()
                {
                    AccountId = 4,
                    Name = "User 1",
                    Email = "user@gmail.com",
                    Password = "123456",
                    Phone = "0973086592",
                    NumberId = "125616123",
                    RoleId = 3,
                    Status = ((int)AccountStatus.Actived),
                    AttemptLogin = 0,
                },
                new Accounts()
                {
                    AccountId = 5,
                    Name = "User 2",
                    Email = "user2@gmail.com",
                    Password = "123456",
                    Phone = "0973086591",
                    NumberId = "125616122",
                    RoleId = 3,
                    Status = ((int)AccountStatus.Actived),
                    AttemptLogin = 0,
                }
            );

            context.Currencies.AddOrUpdate(x => x.CurrencyId,
                new Currencies()
                {
                    CurrencyId = 1,
                    Name = "VND",
                    Status = ((int)DefaultStatus.Actived)
                },
                new Currencies()
                {
                    CurrencyId = 2,
                    Name = "USD",
                    Status = ((int)DefaultStatus.Actived)
                },
                new Currencies()
                {
                    CurrencyId = 3,
                    Name = "EUR",
                    Status = ((int)DefaultStatus.Actived)
                }
            );
        }
    }
}