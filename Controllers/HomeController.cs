using MyPetShop_v3.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyPetShop_v3.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<Pet> pets = DB.GetPetsShow();
            return View(pets);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}