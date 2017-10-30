using CaptstoneProject.Models;
using DataService.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            { 
                var UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var uId = User.Identity.GetUserId();
                var roleList = UserManager.GetRoles(uId);
                var role = roleList.FirstOrDefault();
                if (returnUrl == null)
                {
                    switch (role)
                    {
                        case "Admin": return RedirectToAction("Index", "Admin", new { area = "Admin" });
                        case "Admin Training Management": return RedirectToAction("Index", "AdminTraining", new { area = "AdminTrainingDepartment" });
                        case "Teacher": return RedirectToAction("Index", "Course", new { area = "Teacher" });
                        case "Training Management": return RedirectToAction("Index", "Management", new { area = "TrainingManagement" });
                    }
                }
            }
            //else
            //{
            //    HttpContext.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //}
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

       


    }
}