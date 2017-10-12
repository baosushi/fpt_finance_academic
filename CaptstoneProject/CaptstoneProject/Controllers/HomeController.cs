using CaptstoneProject.Models;
using DataService.Model;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static CaptstoneProject.Controllers.AccountController;

namespace CaptstoneProject.Controllers
{
    public class HomeController : MyBaseController
    {
        private DB_Finance_AcademicEntities contextDB = new DB_Finance_AcademicEntities();

        public ActionResult Index()
        {
            return View("Index");
        }
        public ActionResult Test()
        {
            return View();
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

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            //else
            //{
            //    HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //}
            return View("Login");
        }

       


    }
}