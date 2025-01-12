﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Controllers
{
    public class ChequeBooksController : BaseController
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly IRepository<ChequeBooks> chequebooks;
        private readonly IRepository<Accounts> accounts;

        public ChequeBooksController()
        {
            chequebooks = new Repository<ChequeBooks>();
            accounts = new Repository<Accounts>();
        }
        
        // GET
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetAccountData()
        {
            var user = (Accounts)Session["user"];
            var data = chequebooks.Get(x => x.AccountId == user.AccountId).Select(x => new ChequeBookViewModel
            {
                ChequeBookId = x.ChequeBookId,
                Code = x.Code,
                AccountName = "#" + x.Account.AccountId + " - " + x.Account.Name,
                ChequesUsed = x.Cheques?.Count ?? 0,
                StatusName = ((ChequeBookStatus)x.Status).ToString(),
                Status = x.Status
            });

            return Json(new
            {
                data = data.ToList(),
                message = "Success",
                statusCode = 200
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostData()
        {
            var user = (Accounts)Session["user"];
            var errors = new Dictionary<string, string>();
            var check = true;
            if (!accounts.CheckDuplicate(x => x.AccountId == user.AccountId))
            {
                check = false;
                errors.Add("AccountId", "Account does not exist!");
            }

            if (!check)
                return Json(new
                {
                    statusCode = 402,
                    message = "Error",
                    data = errors
                }, JsonRequestBehavior.AllowGet);
            {
                string random;
                do
                {
                    random = Utils.RandomString(16);
                } while (chequebooks.CheckDuplicate(x => x.Code == random));
                var chequeBook = new ChequeBooks();
                chequeBook.Code = random;
                chequeBook.AccountId = user.AccountId;
                chequebooks.Add(chequeBook);
                return Json(new
                {
                    statusCode = 200,
                    message = "Success"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PutData(int id)
        {
            var x = chequebooks.Get(id);
            var user = (Accounts)Session["user"];
            var account = accounts.Get(user.AccountId);

            if (!chequebooks.CheckDuplicate(y => y.ChequeBookId == id && y.AccountId == account.AccountId))
            {
                return Json(new
                {
                    statusCode = 404,
                    message = "Not found",
                }, JsonRequestBehavior.AllowGet);
            }

            if (x.Status == (int)ChequeBookStatus.Deleted)
            {
                return Json(new
                {
                    statusCode = 400,
                    data = "This cheque book was deleted",
                    message = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            var data = x.Status == 0 ? "Close book successfully" : "Open book succesfully";
            x.Status = x.Status == 0 ? 1 : 0;
            if (!chequebooks.Edit(x))
            {
                return Json(new
                {
                    statusCode = 400,
                    data = "Something wrong happen",
                    message = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                statusCode = 200,
                data = data,
                message = "Success"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteData(int id)
        {
            var x = chequebooks.Get(id);

            var user = (Accounts)Session["user"];
            var account = accounts.Get(user.AccountId);
            if (!chequebooks.CheckDuplicate(y => y.ChequeBookId == id && y.AccountId == account.AccountId))
            {
                return Json(new
                {
                    statusCode = 404,
                    message = "Not found",
                }, JsonRequestBehavior.AllowGet);
            }

            if (x.Status == (int)ChequeBookStatus.Deleted)
            {
                return Json(new
                {
                    statusCode = 400,
                    data = "This cheque book was deleted",
                    message = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            if (x.Cheques.Count > 0)
            {
                return Json(new
                {
                    statusCode = 400,
                    data = "This cheque book has cheque was used, cannot delete",
                    message = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            if (!chequebooks.Delete(x))
            {
                return Json(new
                {
                    statusCode = 400,
                    data = "Something wrong happen",
                    message = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                statusCode = 200,
                data = "Delete Successfully",
                message = "Success"
            }, JsonRequestBehavior.AllowGet);
        }
    }
}