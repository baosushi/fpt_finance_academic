using CaptstoneProject.Models;
using DataService.Model;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static CaptstoneProject.Controllers.AccountController;

namespace CaptstoneProject.Controllers
{
    public class HomeController : Controller
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

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult GoogleSignIn()
        {

            var id = Request.Form["id"]; 
            var fullName = Request.Form["fullName"];
            var givenName = Request.Form["givenName"];
            var imgUrl = Request.Form["imgUrl"];
            var email = Request.Form["email"];
            if(id == null)
            {
                return View("Login");
            }

            User userExist = contextDB.Users.Where(q => q.OAuthId.Equals(id)).FirstOrDefault();
            User user = null;

            //check if existed
            if (userExist != null)
            {
                user = userExist;
            }
            else
            {
                user = contextDB.Users.Create();
                user.OAuthId = id; //Open Authenticate Id
                user.Email = email;
                user.FullName = fullName;
                user.Name = givenName;
                user.ImgUrl = imgUrl;

                contextDB.Users.Add(user);
                contextDB.SaveChanges();
            }

            Session["uImgUrl"] = user.ImgUrl;
            Session["uId"] = user.Id;
            Session["uName"] = user.Name;

            //use Id to save to identify user, haven't implement


            return View("Index");
        }


    }
}