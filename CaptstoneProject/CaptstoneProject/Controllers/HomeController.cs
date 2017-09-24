using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CaptstoneProject.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
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

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult SignIn()
        {
            var id = Request.Form["id"]; 
            var fullName = Request.Form["fullName"];
            var givenName = Request.Form["givenName"];
            var imgUrl = Request.Form["imgUrl"];
            var email = Request.Form["givenName"];

            //use Id to save to identify user, haven't implement


            return View();
        }
    }
}