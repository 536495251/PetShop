using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyPetShop_v3.Models;

namespace MyPetShop_v3.Controllers
{
    public class BuyController : Controller
    {
        // GET: Buy
        public ActionResult Buy(int petid, decimal petprice)
        {
            int ok = 0;
            string error = null;
            User user = (User)Session["LoggedInUser"];
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            DB.Purchase(petid, user.Phone, out ok, out error);
            if (ok == 1)
            {
                user.Balance -= petprice;
                Session["LoggedInUser"] = user;
            }
            PurchaseResult purchaseResult = new PurchaseResult(ok, error);
            ViewBag.PetID = petid;
            return View(purchaseResult);
        }

    }
}