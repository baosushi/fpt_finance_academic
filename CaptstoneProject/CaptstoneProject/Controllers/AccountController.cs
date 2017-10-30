using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CaptstoneProject.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Security;

namespace CaptstoneProject.Controllers
{

    public class AccountController : MyBaseController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //edited
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(MyLoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Login", "Home", model);
            }
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, false, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    //get Roles when log in, User.Identity is store in respone context to add cookie in browser when log in.
                    var uId = SignInManager.AuthenticationManager.AuthenticationResponseGrant.Identity.GetUserId();
                    var roleList = UserManager.GetRoles(uId);
                    Session["uName"] = model.Username;
                    Session["uImgUrl"] = "/Images/prideKappa.jpg";
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
                    return RedirectToLocal(returnUrl);

                    //return RedirectToAction("Index", "Home");
                    return RedirectToLocal(returnUrl);
                //case SignInStatus.LockedOut:
                //    return View("Lockout");
                //case SignInStatus.RequiresVerification:
                //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    model.Username = null;
                    model.Password = null;
                    return View("../Home/Login", model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //edited
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterGG(string id, string name, string email, string imageUrl, string fullName)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    Name = name,
                    FullName = fullName,
                    Email = email,
                    IdGoogle = id,
                };
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    //Persistent cookie will saved as files in browser and exist even if browser is closed
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        return RedirectToAction("Index", "Home");
                    }
                }
                AddErrors(result);
            }

            // If we got this far, return to login form with error
            return RedirectToAction("");
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult("Google", Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //regis
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> MyRegister()
        {
            using (var context = new ApplicationDbContext())
            {
                //try
                //{
                //    var roleStore = new RoleStore<IdentityRole>(context);
                //    var roleManager = new RoleManager<IdentityRole>(roleStore);
                //    //await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
                //    //await roleManager.CreateAsync(new IdentityRole { Name = "Teacher" });
                //    await roleManager.CreateAsync(new IdentityRole { Name = "Student" });
                //    await roleManager.CreateAsync(new IdentityRole { Name = "Head of Department" });
                //    await roleManager.CreateAsync(new IdentityRole { Name = "Training Management" });


                //}
                //catch (Exception e)
                //{

                //    throw e;
                //}



                //var user = new ApplicationUser { UserName = "sensei", Email = "sensei@mail.com" };
                //var result = await UserManager.CreateAsync(user, "@Qawsed321");

                //var user2 = new ApplicationUser { UserName = "gakusei", Email = "gakusei@mail.com" };
                //var result2 = await UserManager.CreateAsync(user2, "@Qawsed123");

                //var user3 = new ApplicationUser { UserName = "bosshere", Email = "bosshere@mail.com" };
                //var result3 = await UserManager.CreateAsync(user3, "@Qawsed123");

                //var user =  UserManager.FindByEmail("sensei@mail.com");
                //var user2 =  UserManager.FindByEmail("gakusei@mail.com");
                //var user3 =  UserManager.FindByEmail("bosshere@mail.com");


                //UserManager.AddToRole(user.Id, "Teacher");
                //UserManager.AddToRole(user2.Id, "Student");
                //UserManager.AddToRole(user3.Id, "Training Management");


                //var user = new ApplicationUser { UserName = "admin", Email = "admin@mail.com" };
                //var result = await UserManager.CreateAsync(user, "@Qawsed123");

                //var user2 = new ApplicationUser { UserName = "admindaotao", Email = "admindaotao@mail.com" };
                //var result2 = await UserManager.CreateAsync(user2, "@Qawsed123");

                //var user3 = new ApplicationUser { UserName = "daotao", Email = "daotao@mail.com" };
                //var result3 = await UserManager.CreateAsync(user3, "@Qawsed123");

                //UserManager.AddToRole(user.Id, "Admin");
                //UserManager.AddToRole(user2.Id, "Admin Training Management");
                //UserManager.AddToRole(user3.Id, "Training Management"); // Employee

                //bao
                //ExternalLoginInfo info = new ExternalLoginInfo { DefaultUserName = "baotdse62099@fpt.edu.vn", Email = "baotdse62099@fpt.edu.vn" };
                //UserLoginInfo uinfo = new UserLoginInfo("Google", "109983346659077543724");
                //info.Login = uinfo;


                //info.Email = "baotdse62099@fpt.edu.vn";
                //info.DefaultUserName = "baotdse62099@fpt.edu.vn";

                //Danh
                //ExternalLoginInfo info2 = new ExternalLoginInfo { DefaultUserName = "danhdcse61904@fpt.edu.vn", Email = "danhdcse61904@fpt.edu.vn" };
                //UserLoginInfo uinfo2 = new UserLoginInfo("Google", "107166571606205395096");
                //info2.Login = uinfo2;

                //Phuong
                //try
                //{

                //    ExternalLoginInfo info4 = new ExternalLoginInfo { DefaultUserName = "phuonglhk@fpt.edu.vn", Email = "phuonglhk@fpt.edu.vn" };
                //    UserLoginInfo uinfo4 = new UserLoginInfo("Google", "phuonglhk@fpt.edu.vn");
                //    info4.Login = uinfo4;

                //    var phuong = new ApplicationUser { UserName = info4.Email, Email = info4.Email, Name = "Lâm Hữu Khánh Phương", FullName = "Lâm Hữu Khánh Phương" };
                //    var result = await UserManager.CreateAsync(phuong);

                //    var t = await UserManager.AddLoginAsync(phuong.Id, info4.Login);
                //    UserManager.AddToRole(phuong.Id, "Teacher");

                //}
                //catch (Exception e)
                //{

                //    throw e;
                //}




                //var bao = new ApplicationUser { UserName = info.Email, Email = info.Email, Name = "Trần Đức Bảo", FullName = "Bao Tran Duc" };
                //var danh = new ApplicationUser { UserName = info2.Email, Email = info2.Email, Name = "Đổng Công Danh", FullName = "(K10_HCM) Đổng Công Danh" };


                //var result = await UserManager.CreateAsync(bao);
                //var result2 = await UserManager.CreateAsync(danh);


                //await UserManager.AddLoginAsync(bao.Id, info.Login);
                //await UserManager.AddLoginAsync(danh.Id, info2.Login);


                //var son= UserManager.FindByEmail("sonphse61822@fpt.edu.vn");

                //UserManager.AddToRole(bao.Id, "Admin");
                //UserManager.AddToRole(danh.Id, "Admin");
                //UserManager.AddToRole(son.Id, "Admin");


            }

            return RedirectToAction("Login", "Home");
        }

        //edited
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            var claims = loginInfo.ExternalIdentity.Claims;

            var name = claims.Where(q => q.Type == ClaimTypes.Name).Select(q => q.Value).SingleOrDefault();
            //var fullName = claims.Where(q => q.Type == ClaimTypes.Surname).Select(q => q.Value).SingleOrDefault();
            var email = claims.Where(q => q.Type == ClaimTypes.Email).Select(q => q.Value).SingleOrDefault();
            var imageUrl = claims.Where(q => q.Type == ClaimTypes.Uri).Select(q => q.Value).SingleOrDefault();

            loginInfo.Login.ProviderKey = email;


            Session["uImgUrl"] = imageUrl;
            Session["uName"] = name;
            if (loginInfo == null)
            {
                return RedirectToAction("Login", "Home");
            }
            //var userExist = UserManager.FindByEmail(email);


            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var uId = SignInManager.AuthenticationManager.AuthenticationResponseGrant.Identity.GetUserId();
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
                    return RedirectToLocal(returnUrl);

                //case SignInStatus.LockedOut:
                //    return View("Lockout");
                //case SignInStatus.RequiresVerification:
                //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                //case SignInStatus.Failure:break;
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    //ViewBag.ReturnUrl = returnUrl;
                    //ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    //return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
                    MyLoginViewModel model = new MyLoginViewModel();
                    ModelState.AddModelError("", "Invalid account");
                    return View("../Home/Login", model);
            }
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff to Custom Login at HomeController
        [HttpPost]
        //[ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.Abandon();
            return RedirectToAction("Login", "Home", new { returnUrl = ""});
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}