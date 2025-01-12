﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Backend.Areas.Admin.Data;
using OnlineBanking.BLL.Repositories;
using OnlineBanking.DAL;

namespace Backend.Areas.Admin.Controllers
{
    public class CurrenciesController : BaseController
    {
        private readonly IRepository<Currencies> currencies;

        public CurrenciesController()
        {
            currencies = new Repository<Currencies>();
        }

        // GET: Admin/Currencies
        public ActionResult Index()
        {
            ViewBag.Currency = "active";
            return View();
        }

        public ActionResult GetData()
        {
            ViewBag.Accounts = "active";

            var data = currencies.Get().Select(x => new CurencyViewModel(x));

            return Json(new
            {
                data = data.ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FindId(int id)
        {
            var currency = currencies.Get(id);
            var data = new CurencyViewModel(currency);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PostData(Currencies c)
        {
            var errors = new Dictionary<string, string>();

            if (ModelState.IsValid)
            {
                var check = currencies.Get(x => x.Name == c.Name).FirstOrDefault();

                if (!Utils.IsNullOrEmpty(check))
                {
                    if (check != null && check.Status == (int) DefaultStatus.Deleted)
                    {
                        c.CurrencyId = check.CurrencyId;
                        c.Status = (int) DefaultStatus.Actived;
                        currencies.Update(c);

                        return Json(new
                        {
                            statusCode = 200,
                            message = "Success",
                            data = c
                        }, JsonRequestBehavior.AllowGet);
                    }

                    errors.Add("Name", "Name is duplicated!");

                    return Json(new
                    {
                        statusCode = 400,
                        message = "Error",
                        data = errors
                    }, JsonRequestBehavior.AllowGet);
                }

                c.Status = (int) DefaultStatus.Actived;
                currencies.Add(c);

                return Json(new
                {
                    statusCode = 200,
                    message = "Success",
                    data = c
                }, JsonRequestBehavior.AllowGet);
            }

            foreach (var k in ModelState.Keys)
            foreach (var err in ModelState[k].Errors)
            {
                var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
                if (!errors.ContainsKey(key))
                    errors.Add(key, err.ErrorMessage);
            }

            return Json(new
            {
                statusCode = 400,
                message = "Error",
                data = errors
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PutData(Currencies c)
        {
            var errors = new Dictionary<string, string>();

            if (ModelState.IsValid)
            {
                var check = currencies.Get(x => x.CurrencyId != c.CurrencyId && x.Name == c.Name);

                if (!check.IsNullOrEmpty())
                {
                    errors.Add("Name", "Name is duplicated!");

                    return Json(new
                    {
                        statusCode = 400,
                        message = "Error",
                        data = errors
                    }, JsonRequestBehavior.AllowGet);
                }

                c.Status = (int) DefaultStatus.Actived;
                currencies.Edit(c);

                return Json(new
                {
                    statusCode = 200,
                    message = "Success",
                    data = c
                }, JsonRequestBehavior.AllowGet);
            }

            foreach (var k in ModelState.Keys)
            foreach (var err in ModelState[k].Errors)
            {
                var key = Regex.Replace(k, @"(\w+)\.(\w+)", @"$2");
                if (!errors.ContainsKey(key))
                    errors.Add(key, err.ErrorMessage);
            }

            return Json(new
            {
                statusCode = 400,
                message = "Error",
                data = errors
            }, JsonRequestBehavior.AllowGet);
        }


        // GET: Admin/Currencies/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                var currency = currencies.Get(id);
                if (currency == null)
                {
                    return Json(new
                    {
                        statusCode = 400,
                        data = "This currency does not exist",
                        message = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }

                if (currency.BankAccounts.Count > 0)
                {
                    return Json(new
                    {
                        statusCode = 400,
                        data = "This currency have bank account link to it, cannot delete!",
                        message = "Error"
                    }, JsonRequestBehavior.AllowGet);
                }

                if (currencies.Delete(currency))
                {
                    return Json(new
                    {
                        statusCode = 200,
                        message = "Success"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (System.Exception)
            {
                return Json(new
                {
                    statusCode = 400,
                    data = "Something error happen",
                    message = "Error"
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                statusCode = 400,
                data = "Something error happen",
                message = "Error"
            }, JsonRequestBehavior.AllowGet);
        }

        private bool ExistCurrencyId(int id)
        {
            return Utils.IsNullOrEmpty(currencies.Get(id));
        }
    }
}