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
using ACEDrivingSchool2.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Net.Mail;
using ACEDrivingSchool2.Extensions;

namespace ACEDrivingSchool2.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
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
        //returns the login page
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        //checks that the users email and password entered match a user stored in the database, if so logs them in and returns them to the homepage, if not displays an error message
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Index", "Home");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }



        //
        // GET: /Account/Register
        //returns the register page to the user
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        //creates a new user object and stores it in the databse from the data passed in from the model
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (UserManager.Users.Where(u => u.DrivingLicence == model.DrivingLicence).FirstOrDefault() != null)
                {
                    var uniqueLicenceNum = UserManager.Users.Where(u => u.DrivingLicence == model.DrivingLicence).First();
                    this.AddNotification("The Driving Licence Number You Have Entered Has Already Been Used To Create An Account. Are You Sure You Do Not Already Have An Account?", NotificationType.ERROR);
                    return View(model);
                }

                if (model.DateOfBirth.AddYears(16) > DateTime.Now)
                {
                    this.AddNotification("Sorry you must be atleast 16 years old to register", NotificationType.ERROR);
                    return View(model);
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.HomePhone, MobilePhone = model.MobilePhone, Name = model.Name, DrivingLicence = model.DrivingLicence, DateOfBirth = model.DateOfBirth, Address = model.Address };
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    //geereates a confirmation code for the user account created
                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action(
                       "ConfirmEmail", "Account",
                       new { userId = user.Id, code = code },
                       protocol: Request.Url.Scheme);

                    await ConfirmEmail(user.Id, code);
                    ////sends an email to the user with a link to activate their account
                    //await UserManager.SendEmailAsync(user.Id,
                      // "Confirm your account",
                      // "Please confirm your account by clicking this link: <a href=\""
                      //                                 + callbackUrl + "\">link</a>");

                    ////assigns the user a role (Student by default but an admin may pass in a role to create a new user of any role)
                    //await UserManager.AddToRoleAsync(user.Id, model.Role);

                    //returns a message to the user telling them a confirmation email has been sent to their email
                    //this.AddNotification("A confirmation email has been sent to your email address, you must use the confirmation link before you can make a booking", NotificationType.SUCCESS);
                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            //something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        //returns the page displayed when a user uses the confirmation link sent to them and verifies that the code and user id are valid, if not returns an error
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
        // POST: /Account/LogOff
        //logs the user out
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
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