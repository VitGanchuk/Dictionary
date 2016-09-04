using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using VitEgoDictionary.Models.Parameters;
using VitEgoDictionary.Models.ViewModels;

namespace VitEgoDictionary.Controllers
{
    public class AccountController : DictionaryController
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View(new UrlReferrerViewModel(this.Request.UrlReferrer, this.Request.IsAuthenticated, this.HttpContext.User, _entities));
        }

        [HttpPost]
        public JsonResult Login(AccountLoginParameter parameters)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(parameters.UserName, parameters.Password))
                {
                    FormsAuthentication.SetAuthCookie(parameters.UserName, parameters.RememberMe);
                    return Json(new { result = "redirect", url = parameters.UrlReferrer });
                }
                else
                {
                    return Json(new { result = "error", message = "The user name or password provided is incorrect" });
                }
            }
            else { return Json(new { result = "error", message = "The model state is invalid" }); }
        }

        [HttpGet]
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect(this.Request.UrlReferrer.ToString());
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View(new MasterLayoutViewModel(this.Request.IsAuthenticated, this.HttpContext.User, null));
        }

        [HttpPost]
        public ActionResult Register(int i = 0/*RegisterModel model*/)
        {
            //if (ModelState.IsValid)
            //{
            //    // Attempt to register the user
            //    MembershipCreateStatus createStatus;
            //    Membership.CreateUser(model.UserName, model.Password, model.Email, "question", "answer", true, null, out createStatus);

            //    if (createStatus == MembershipCreateStatus.Success)
            //    {
            //        MigrateShoppingCart(model.UserName);
            //        FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
            //        return RedirectToAction("Index", "Home");
            //    }
            //    else
            //    {
            //        ModelState.AddModelError("", ErrorCodeToString(createStatus));
            //    }
            //}

            //// If we got this far, something failed, redisplay form
            //return View(model);
            return null;
        }

        [HttpGet]
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View(new AccountChangePasswordiewModel(this.Request.UrlReferrer, this.Request.IsAuthenticated, this.HttpContext.User,
                Membership.MinRequiredPasswordLength, Membership.MinRequiredNonAlphanumericCharacters, _entities));
        }

        [HttpPost]
        [Authorize]     
        public JsonResult ChangePassword(AccountChangePasswordParameter parameters)
        {
            if (ModelState.IsValid)
            {
                if (parameters.NewPassword == parameters.ConfirmPassword)
                {
                    try
                    {
                        MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                        if (currentUser.ChangePassword(parameters.OldPassword, parameters.NewPassword))
                        {
                            return Json(new { result = "redirect", url = parameters.UrlReferrer });
                        }
                        else
                        {
                            return Json(new { result = "error", message = "The current password is incorrect or the new password is invalid" });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new
                        {
                            result = "error",
                            message = ex.Message,
                            innerMessage = ex.InnerException == null ? null : ex.InnerException.Message
                        });
                    }
                }
                else { return Json(new { result = "error", message = "Passwords provided do not match" }); }
            }
            else { return Json(new { result = "error", message = "The model state is invalid" }); }
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
