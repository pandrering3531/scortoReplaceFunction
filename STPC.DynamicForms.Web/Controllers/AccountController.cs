using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Linq;
using STPC.DynamicForms.Infraestructure;
using STPC.DynamicForms.Web.Helpers;
using STPC.DynamicForms.Web.Models;
using STPC.DynamicForms.Web.Common;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Common.Messages;
using STPC.DynamicForms.Core;
using STPC.DynamicForms.Web.Services.Entities;
using System.Security.Principal;

namespace STPC.DynamicForms.Web.Controllers
{

    [HandleError]
    public class AccountController : Controller
    {

        CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
        Services.Entities.STPC_FormsFormEntities db = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

        private bool ValidateCookie(string userName)
        {
            string registerUser = string.Empty;

            if (Request.Cookies["RegisterMachine_" + userName] != null)
            {
                registerUser = Request.Cookies["RegisterMachine_" + userName].Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpPost]
        public ActionResult generateCaptcha()
        {
            System.Drawing.FontFamily family = new System.Drawing.FontFamily("Arial");
            CaptchaImage img = new CaptchaImage(150, 50, family);
            string text = img.CreateRandomText(7);
            string resultCode = text.Substring(0, 4) + " " + text.Substring(4, 3);
            //string text = img.CreateRandomText(7) ;
            img.SetText(resultCode);
            img.GenerateImage();
            img.Image.Save(Server.MapPath("~") + this.Session.SessionID.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
            Session["captchaText"] = resultCode;
            return Json(this.Session.SessionID.ToString() + ".png?t=" + DateTime.Now.Ticks, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RefreshToken()
        {
            return PartialView("_AntiForgeryToken");
        }

        public ActionResult LogOn()
        {
            try
            {


                if (this.User.Identity.IsAuthenticated)
                {

                    return RedirectToAction("Menu", "Home");
                }
                else
                    return View(new LoginViewModel());
            }
            catch (Exception ex)
            {
                return DefaultErrorhandling(ex, "LogOn");

            }
        }

        private ActionResult DefaultErrorhandling(Exception ex, string triggerAction )
        {
            bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
            Guid correlationID = Guid.NewGuid();

            ILogging eventWriter = LoggingFactory.GetInstance();
            string errorMessage = string.Format(CustomMessages.E0007, "AccountController", triggerAction, correlationID, ex.Message);
            System.Diagnostics.Debug.WriteLine("Excepción: " + errorMessage);

            if (ShowErrorDetail)
            {
                eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
                return RedirectToAction("ErrorDetail", "Shared", new { errorMessage = ex.Message, controler = "AccountController", action = triggerAction });
            }
            else
            {
                //return PartialView("_ErrorGeneral", string.Format(CustomMessages.E0001, correlationID.ToString()));
                return RedirectToAction("ErrorDetail", "Shared", new { errorMessage = string.Format(CustomMessages.E0001, correlationID.ToString()), controler = "AccountController", action = triggerAction });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryTokenAttribute]
        public ActionResult LogOn(string Username, string Idtype, string Password, string returnUrl, bool viewCpatcha)
        {
            bool showCaptcha = false;
            string c = Request.UserHostAddress;
            int iattemptCount = 0;

            try
            {
                bool isResetPassword = false;


                if (ModelState.IsValid)
                {

                    if (Session["attemptCount"] == null)
                    {
                        Session["attemptCount"] = 1;

                    }
                    iattemptCount = Convert.ToInt32(Session["attemptCount"]);

                    if (provider.ValidateUser(Idtype + Username, Password, viewCpatcha, ref isResetPassword, ref showCaptcha, ref iattemptCount))
                    {
                        if (!showCaptcha)
                        {
                            if (!isResetPassword)
                            {

                                FormsAuthentication.SetAuthCookie(Idtype + Username, true);
                                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                                {
                                    if (ValidateCookie(Idtype + Username))
                                    {
                                        Session["attemptCount"] = "0";
                                        Session["Logged"] = "true";
                                        return Redirect(returnUrl);
                                    }
                                    else
                                    {
                                        return Json(JsonResponseFactory.ErrorResponse(Constants.ERROR_MAX_ATTEMPT_ALLOWED, "E002", showCaptcha), JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    if (ValidateCookie(Idtype + Username))
                                    {
                                        Session["attemptCount"] = "0";
                                        Session["Logged"] = "true";
                                        return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        return Json(JsonResponseFactory.ErrorResponse(Constants.ERROR_MAX_ATTEMPT_ALLOWED, "E002", showCaptcha), JsonRequestBehavior.AllowGet);
                                    }

                                }
                                //}

                            }
                            else
                            {
                                //var list = provider.GetUserResetQuestion(Idtype + Username);
                                //ViewBag.UserName = Idtype + Username;
                                //Session["UserName"] = Idtype + Username;
                                //return PartialView("_Answer_Recovery_Password", list.ToList());

                                ChangePasswordModel _ChangePasswordModel = new ChangePasswordModel();
                                _ChangePasswordModel.TempUserName = Idtype + Username;

                                _ChangePasswordModel.NewPassword = "123456789";
                                return PartialView("ChangePassword", _ChangePasswordModel);
                            }
                        }
                        else
                            return Json(JsonResponseFactory.ErrorResponse(Constants.ERROR_MAX_ATTEMPT_ALLOWED, showCaptcha), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Session["attemptCount"] = iattemptCount;
                        return Json(JsonResponseFactory.ErrorResponse(Constants.ERROR_AUTENTICATION_FAILED, showCaptcha), JsonRequestBehavior.AllowGet);
                    }
                }
                LoginViewModel model = new LoginViewModel();

                ModelState.AddModelError("", Constants.ERROR_AUTENTICATION_FAILED);
                return Json(JsonResponseFactory.ErrorResponse(Constants.ERROR_AUTENTICATION_FAILED, showCaptcha), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(JsonResponseFactory.ErrorResponse(ex.Message, showCaptcha), JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult LogIn(LoginViewModel model, string e, string f)
        {
            if (!string.IsNullOrEmpty(e))
            {
                #region validar el username y el key

                try
                {
                    #region construir la llave de la fecha

                    StringBuilder plainText = new StringBuilder();
                    plainText.Append(e[0]);
                    plainText.Append(e[2]);
                    plainText.Append(e[e.Length - 1]);
                    plainText.Append("-");
                    plainText.Append(e);
                    plainText.Append("-");
                    plainText.Append(DateTime.Now.ToString("yyyyMMdd"));

                    #endregion construir la llave de la fecha

                    #region obtener el key de validacion

                    string LOGIN_VALIDATION_KEY = System.Configuration.ConfigurationManager.AppSettings["loginKey"];

                    byte[] key = System.Text.Encoding.UTF8.GetBytes(LOGIN_VALIDATION_KEY);
                    byte[] plainbytes = Encoding.UTF8.GetBytes(plainText.ToString());

                    string result;
                    using (HMACMD5 hmac = new HMACMD5(key))
                    {
                        byte[] hashValue = hmac.ComputeHash(plainbytes);
                        result = Convert.ToBase64String(hashValue);
                    }

                    #endregion obtener el key de validacion

                    #region validar las llaves
                    result = result.Replace('+', ' ');
                    if (f == result)
                    {
                        FormsAuthentication.SetAuthCookie(e, true);
                        return RedirectToAction("MenuAuth", "Home", model);
                    }
                    else
                    {
                        return RedirectToLogOffPage();
                    }

                    #endregion validar las llaves
                }
                catch (Exception ex)
                {
                    Guid correlationID = Guid.NewGuid();

                    ILogging eventWriter = LoggingFactory.GetInstance();
                    string errorMessage = string.Format(CustomMessages.E0007, "AccountController", "LogIn", correlationID, ex.Message, ex.StackTrace, e);
                    System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
                    eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
                    //return RedirectToLogOffPage();
                    return RedirectToAction("Index", "Home");
                }

                #endregion validar el username y el key
            }

            //return RedirectToLogOffPage();
            return RedirectToAction("Index", "Home");

        }

        public ActionResult LogOff()
        {
            try
            {


                System.Web.HttpContext.Current.Cache.Remove(User.Identity.Name);
                FormsAuthentication.SignOut();
                Session.Abandon();
                HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
                cookie1.Expires = DateTime.Now.AddYears(-1);
                Response.Cookies.Add(cookie1);
                HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
                cookie2.Expires = DateTime.Now.AddYears(-1);
                Response.Cookies.Add(cookie2);
                provider.CloseSesion(User.Identity.Name);

                HttpContext.User =
         new GenericPrincipal(new GenericIdentity(string.Empty), null);

                FormsAuthentication.RedirectToLoginPage();

                return RedirectToAction("LogOn");
            }

            catch (Exception ex)
            {
                return DefaultErrorhandling(ex, "LogOff");

            }
        }


        //Hace logOf pero sin cambiar IsOnline=false
        public ActionResult LogOffIsOnline()
        {
            System.Web.HttpContext.Current.Cache.Remove(User.Identity.Name);
            FormsAuthentication.SignOut();
            Session.Abandon();
            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie1.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie1);
            HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
            cookie2.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie2);

            HttpContext.User =
     new GenericPrincipal(new GenericIdentity(string.Empty), null);

            FormsAuthentication.RedirectToLoginPage();

            return RedirectToAction("LogOn");

        }
        public ActionResult RedirectToLogOffPage()
        {
            try
            {
                if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings["LogOffToUrl"].ToLower()))
                {
                    string LOGOFF_URL = System.Configuration.ConfigurationManager.AppSettings["LogOffUrl"];
                    ViewBag.logoffUrl = LOGOFF_URL;
                    return Redirect(LOGOFF_URL);
                }
                else
                {
                    return RedirectToAction("LogOn", "Account");
                }
            }
            catch (Exception ex)
            {
                bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
                Guid correlationID = Guid.NewGuid();

                ILogging eventWriter = LoggingFactory.GetInstance();
                string errorMessage = string.Format(CustomMessages.E0007, "AccountController", "RedirectToLogOffPage", correlationID, ex.Message);
                System.Diagnostics.Debug.WriteLine("Excepción: " + errorMessage);
                if (ShowErrorDetail)
                {
                    eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
                    //return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(errorMessage), "AccountController", "RedirectToLogOffPage"));

                    return RedirectToAction("ErrorDetail", "Shared", new { errorMessage = ex.Message, controler = "AccountController", action = "RedirectToLogOffPage" });
                }
                else
                {
                    //return PartialView("_ErrorGeneral", string.Format(CustomMessages.E0001, correlationID.ToString()));
                    return RedirectToAction("ErrorDetail", "Shared", new { errorMessage = string.Format(CustomMessages.E0001, correlationID.ToString()), controler = "AccountController", action = "RedirectToLogOffPage" });
                }
            }

        }

        public ActionResult Register()
        {
            if (Roles.IsUserInRole("Administrador") || Roles.IsUserInRole("Co-Administrador"))
            {

                var model = new CreateUserViewModel();
                PopdlateCreateUserDropDowns(model);
                return View(model);
            }

            return View("NotAuthorized");
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Co-Administrador")]
        public ActionResult Register(CreateUserViewModel model, FormCollection par)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    var user = new STPC.DynamicForms.Web.Common.Services.Users.User
                    {
                        Username = model.IDType + model.IDNumber,
                        Email = model.User.Email,
                        GivenName = model.User.GivenName,
                        LastName = model.User.LastName,
                        Password = model.User.Password,
                        PasswordQuestion = model.User.PasswordQuestion,
                        PasswordAnswer = model.User.PasswordAnswer,
                        Phone_LandLine = model.User.Phone_LandLine,
                        Phone_Mobile = model.User.Phone_Mobile,
                        Address = model.User.Address,
                        IsResetPassword = true,
                        IsApproved = true,
                        Hierarchy = new STPC.DynamicForms.Web.Common.Services.Users.Hierarchy { Id = int.Parse(model.HierarchyId) }
                    };
                    MembershipCreateStatus status;
                    provider.CreateUser(user, out status);
                    if (status == MembershipCreateStatus.Success)
                    {


                        foreach (var key in Request.Form.AllKeys)
                        {
                            if (par["singleRoleMode"] == "False")
                            {
                                if (key.IndexOf("Rol_") == 0)
                                    Roles.AddUserToRole(user.Username, key.Substring(4));
                            }
                            else
                            {
                                if (key.IndexOf("RoleName") == 0)
                                    Roles.AddUserToRole(user.Username, par["RoleName"].Substring(4));
                            }

                            ViewBag.singleRoleMode = par["singleRoleMode"];
                        }
                        SendEmail(user, model.User.Password);
                        return RedirectToAction("List");
                    }
                    else
                        ModelState.AddModelError("IDNumber", status.ToString());


                }
                PopulateCreateUserDropDowns(model);



                return View(model);
            }
            catch (Exception ex)
            {
                PopulateCreateUserDropDowns(model);
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [Authorize]
        public ActionResult List()
        {
            try
            {
                var model = provider.GetAllUsers();
                return View(model);
            }
            catch(Exception ex)
            {
                return DefaultErrorhandling(ex, "List");

            }
        }

        [Authorize]
        public ActionResult Details(string id)
        {

            ViewBag.LoggedUserIsAdmin = Roles.IsUserInRole("Administrador");
            var theuser = provider.GetUser(id);

            if (Roles.IsUserInRole("Administrador") ||
                (Roles.IsUserInRole("Co-Administrador") && !Roles.GetRolesForUser(theuser.Username).Contains("Administrador")))
            {
                return View(theuser);
            }


            return View("NotAuthorized");
        }

        [Authorize]
        public ActionResult Edit(string id)
        {

            var user = provider.GetUser(id);

            if (Roles.IsUserInRole("Administrador") ||
                 (Roles.IsUserInRole("Co-Administrador") && !Roles.GetRolesForUser(user.Username).Contains("Administrador")))
            {

                var model = new CreateUserViewModel();
                PopdlateCreateUserDropDowns(model);


                model.User = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    GivenName = user.GivenName,
                    Hierarchy = user.Hierarchy,
                    LastName = user.LastName,
                    Phone_LandLine = user.Phone_LandLine,
                    Phone_Mobile = user.Phone_Mobile
                };
                model.AppRoles = GetRoleAdministrationList();
                model.UserRoles = Roles.GetRolesForUser(user.Username);
                ExtractIdTypeAndNumberFromUsername(model);
                return View(model);
            }

            return View("NotAuthorized");

        }

        private void ExtractIdTypeAndNumberFromUsername(CreateUserViewModel model)
        {
            if (string.IsNullOrEmpty(model.User.Username)) throw new ArgumentException("Username cannot be blank", "username");
            if (model.User.Username.Length < 3) throw new ArgumentException("Username shodld at least be 3 characters long", "username");
            string theidtypeasstring = model.User.Username.Substring(0, 2);
            IDTypesEnumeration theidtypeasenum;
            if (!Enum.TryParse(theidtypeasstring, out theidtypeasenum)) throw new ArgumentException("The ID Type part of the username doesn't match any valid id type");
            model.IDType = theidtypeasstring;
            string theidnumberasstring = model.User.Username.Substring(2);
            long theidnumber;
            if (!long.TryParse(theidnumberasstring, out theidnumber)) throw new ArgumentException("The ID Number part of the username doesn't seems to be a number");
            model.IDNumber = theidnumber;
        }

        [Authorize]
        [HttpPost]
        public ActionResult Edit(CreateUserViewModel model, FormCollection par)
        {
            //ModelState["User.Password"].Errors.Clear();
            //ModelState["User.PasswordQuestion"].Errors.Clear();
            //ModelState["User.PasswordAnswer"].Errors.Clear();
            //if (ModelState.IsValid)
            //{
            if (model.User.Username == this.User.Identity.Name || Roles.IsUserInRole("Administrador"))
            {
                bool mustLogoutAfterChange = false;
                if (model.IDType + model.IDNumber != model.User.Username)
                {
                    if (model.User.Username == this.User.Identity.Name) mustLogoutAfterChange = true;
                    model.User.Username = model.IDType + model.IDNumber;
                }
                model.User.Hierarchy = new STPC.DynamicForms.Web.Common.Services.Users.Hierarchy { Id = int.Parse(model.HierarchyId) };
                Common.Services.Users.User serviceUser = new Common.Services.Users.User();
                Object obj = (Object)serviceUser;
                model.User.CopyTo(ref obj);
                provider.UpdateUser(serviceUser);

                List<string> newRolesForUser = new List<string>();
                foreach (var key in Request.Form.AllKeys)
                {
                    if (key.IndexOf("Rol_") == 0)
                        newRolesForUser.Add(key.Substring(4));
                }

                string[] rolesByUser = Roles.GetRolesForUser(serviceUser.Username);

                if (par["singleRoleMode"] != "False" && rolesByUser.Length > 0)
                {

                    Roles.RemoveUserFromRoles(serviceUser.Username, rolesByUser);

                }
                foreach (var key in Request.Form.AllKeys)
                {
                    if (par["singleRoleMode"] == "False")
                    {
                        if (key.IndexOf("Rol_") == 0)
                            Roles.AddUserToRole(serviceUser.Username, key.Substring(4));
                    }
                    else
                    {
                        if (key.IndexOf("RoleName") == 0)
                            Roles.AddUserToRole(serviceUser.Username, par["RoleName"].Substring(4));
                    }

                    ViewBag.singleRoleMode = par["singleRoleMode"];
                }


                if (mustLogoutAfterChange) return RedirectToAction("LogOff");
                return RedirectToAction("List");
            }
            else { return View("NotAuthorized"); }
            //}

            model.UserRoles = Roles.GetRolesForUser(model.User.Username);


            PopdlateCreateUserDropDowns(model);
            return View(model);
        }

        private void UpdateUserRoles(string username, List<string> newroles)
        {
            var currentRoles = Roles.GetRolesForUser(username);
            foreach (var rol in currentRoles)
            {
                if (!newroles.Contains(rol))
                    Roles.RemoveUserFromRole(username, rol);
            }
            foreach (var rol in newroles)
            {
                if (!currentRoles.Contains(rol))
                    Roles.AddUserToRole(username, rol);
            }
        }

        [HttpPost]
        public JsonResult UnlockUser(string username)
        {
            provider.UnlockUser(username);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult LockUser(string username)
        {
            provider.LockUser(username);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult ApproveUser(string username)
        {
            provider.ApproveUser(username);
            return Json(new { success = true });
        }

        //[Authorize]
        public ActionResult ChangePassword()
        {
            ViewData["PasswordLength"] = provider.MinRequiredPasswordLength;
            ChangePasswordModel _ChangePasswordModel = new ChangePasswordModel();
            //_ChangePasswordModel.TempUserName = this.User.Identity.Name;
            _ChangePasswordModel.NewPassword = "123456789";
            return View(_ChangePasswordModel);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAttribute]
        public ActionResult ChangePasswordFromRecovery(string userName, string Idtype)
        {
            ViewData["PasswordLength"] = provider.MinRequiredPasswordLength;
            ChangePasswordModel _ChangePasswordModel = new ChangePasswordModel();
            _ChangePasswordModel.TempUserName = Idtype + userName;

            _ChangePasswordModel.NewPassword = "123456789";
            return PartialView("ChangePassword", _ChangePasswordModel);
        }


        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            try
            {


                if (model.TempUserName == null)
                    if (provider.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                    {
                        return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(JsonResponseFactory.ErrorResponse("La contraseña actual es incorrecta o la nueva contraseña no es válida."), JsonRequestBehavior.AllowGet);
                    }
                else
                    if (provider.ChangePasswordByChangePassword(model.TempUserName, model.NewPassword))
                    {

                        return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(JsonResponseFactory.ErrorResponse("La contraseña actual es incorrecta o la nueva contraseña no es válida."), JsonRequestBehavior.AllowGet);
                        //ModelState.AddModelError("", "La contraseña actual es incorrecta o la nueva contraseña no es válida.");
                    }

            }
            catch (Exception ex)
            {
                //if (model.TempUserName == null)
                //{
                //	ModelState.AddModelError("", ex.Message);
                //	ViewData["PasswordLength"] = provider.MinRequiredPasswordLength;
                //	return View(model);
                //}
                //else
                //{

                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
                //}
            }
        }


        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        [Authorize]
        public ActionResult ConfirmUserRegister(string userName, string token)
        {
            var user = provider.GetUser(userName);
            if (user != null)
            {
                if (!string.IsNullOrEmpty(user.Token))
                {
                    if (user.Token.Equals(token))
                    {
                        Common.Services.Users.User serviceUser = new Common.Services.Users.User()
                        {
                            Id = user.Id,
                            Username = user.Username,
                            Email = user.Email,
                            GivenName = user.GivenName,
                            Hierarchy = user.Hierarchy,
                            LastName = user.LastName,
                            Phone_LandLine = user.Phone_LandLine,
                            Phone_Mobile = user.Phone_Mobile,
                            IsApproved = true
                        };

                        provider.UpdateUser(serviceUser);

                        return View();
                    }
                    return View("NotAuthorized");
                }
                else
                    return View("NotAuthorized");

            }
            else
                return View("NotAuthorized");

        }

        private void PopdlateCreateUserDropDowns(CreateUserViewModel model)
        {
            //TODO: Sacar las URLs del servicio al web.config
            model.HierarchyLevels = new List<SelectListItem>();
            WebClient wc = new WebClient();
            XDocument xdoc = XDocument.Parse(wc.DownloadString(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString() + "GetHierarchiesLevels")));
            List<string> list = xdoc.Root.Descendants(((XNamespace)@"http://schemas.microsoft.com/ado/2007/08/dataservices") + "element").Select(xe => xe.Value).ToList();
            foreach (var level in list)
            {
                model.HierarchyLevels.Add(new SelectListItem { Text = level, Value = level });
            }

            IOrderedQueryable<STPC.DynamicForms.Web.Services.Entities.Hierarchy> modelList;
            if (string.IsNullOrEmpty(model.HierarchyId))
            {
                modelList = db.Hierarchies.Where(r => r.Level == model.HierarchyLevels.First().Text).OrderBy(z => z.Name);
            }
            else
            {
                string selectedlevel = db.Hierarchies.Where(i => i.Id == int.Parse(model.HierarchyId)).First().Level;
                modelList = db.Hierarchies.Where(r => r.Level == selectedlevel).OrderBy(z => z.Name);
            }

            List<SelectListItem> modelData = modelList.Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            }).ToList();
            model.Hierarchies = modelData;

            model.AppRoles = GetRoleAdministrationList();

        }

        private string[] GetRoleAdministrationList()
        {
            string[] roleList = Roles.GetAllRoles();



            if (!User.IsInRole("Administrador"))
            {
                if (User.IsInRole("Co-Administrador"))
                {
                    roleList = roleList.Where(r => r != "Administrador").ToArray();
                }

                else
                    roleList = roleList.Where(r => r != "Administrador" && r != "Co-Administrador").ToArray();
            }

            return roleList;

        }
        private string GetToken(string text)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
            byte[] hashbyte = md5.ComputeHash(data);


            System.Text.StringBuilder token = new System.Text.StringBuilder();

            for (int i = 0; i < hashbyte.Length; i++)
            {
                token.Append(hashbyte[i].ToString("x2").ToLower());
            }


            return token.ToString();
        }

        private void SendEmail(STPC.DynamicForms.Web.Common.Services.Users.User user, string newPassWord)
        {
            string server = ConfigurationManager.AppSettings["Server"].ToString();
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            string userName = ConfigurationManager.AppSettings["User"].ToString();
            string password = ConfigurationManager.AppSettings["Password"].ToString();
            string domain = ConfigurationManager.AppSettings["Domain"].ToString();
            string from = ConfigurationManager.AppSettings["From"].ToString();
            string url = ConfigurationManager.AppSettings["ValidateEmailURL"].ToString();
            string BodyMail = ConfigurationManager.AppSettings["BodyMail"].ToString();
            string Subjet = ConfigurationManager.AppSettings["Subjet"].ToString();
            string Body = BodyMail + " " + newPassWord;//GetURL(user);

            Email email = new Email(server, port, userName, password, domain, "");

            email.SendMail(from, user.Email, string.Empty, Subjet, Body);
        }

        private string GetURL(STPC.DynamicForms.Web.Common.Services.Users.User user)
        {
            string url = ConfigurationManager.AppSettings["ValidateEmailURL"].ToString();
            string newURL = string.Empty;

            newURL = string.Format(url, user.Username, "&", user.Token);

            return newURL;
        }

        public ActionResult Recover_Password()
        {


            //var list = provider.GetUserResetQuestion(this.User.Identity.Name);


            return View(new LoginViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryTokenAttribute]
        [Authorize]
        public ActionResult LoadQuestionUser(string Username, string Idtype)
        {
            try
            {
                Session["UserName"] = Idtype + Username;
                ModelState.Clear();
                var list = provider.GetUserResetQuestion(Idtype + Username);

                return PartialView("_Answer_Recovery_Password", list.ToList());
            }
            catch (Exception ex)
            {
                ModelState.Clear();
                ModelState.AddModelError("", ex.Message);

                LoginViewModel Newmodel = new LoginViewModel();
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult LoadQuestionUserPost(FormCollection par)
        {
            try
            {
                string[] Respuestas = par[2].Split(new Char[] { ',' });
                string[] EncodedAnwerBBDD = par[1].Split(new Char[] { ',' });

                string[] EncodedAnswer = provider.UnEncodeAnswer(Respuestas);

                for (int i = 0; i < EncodedAnwerBBDD.Length; i++)
                {
                    if (EncodedAnwerBBDD[i] != EncodedAnswer[i])
                        return Json(JsonResponseFactory.ErrorResponse(Constants.ERROR_ANSWER_FAILED), JsonRequestBehavior.AllowGet);
                }
                provider.UpdateFiedldIsResetPassword(Session["UserName"].ToString(), false);

                //if (ValidateCookie())
                //{
                return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
                //}
                //else
                //	return Json(JsonResponseFactory.ErrorResponse(Constants.ERROR_MAX_ATTEMPT_ALLOWED, "E002", false), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", ex.Message);

                LoginViewModel Newmodel = new LoginViewModel();
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Answer_Question(string userName)
        {
            try
            {
                ViewBag.UserName = userName;
                var list = provider.GetQuestions();

                return PartialView("_Answer_Recovery_Questions", list.ToList());

            }
            catch (Exception ex)
            {
                ModelState.Clear();
                ModelState.AddModelError("", ex.Message);

                LoginViewModel Newmodel = new LoginViewModel();
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Answer_QuestionLogIn(string userName)
        {
            try
            {
                ViewBag.UserName = userName;
                var list = provider.GetQuestions();

                return PartialView("_Answer_Recovery_Questions_LogIn", list.ToList());

            }
            catch (Exception ex)
            {
                ModelState.Clear();
                ModelState.AddModelError("", ex.Message);

                LoginViewModel Newmodel = new LoginViewModel();
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Answer_Question(FormCollection par)
        {
            try
            {


                string[] PreguntasId = par[1].Split(new Char[] { ',' });
                string[] PreguntasText = par[2].Split(new Char[] { ',' });
                string[] RespuestasText = par[3].Split(new Char[] { ',' });


                if (this.User.Identity.IsAuthenticated)
                {
                    provider.DeleteAllAnswerByUser(this.User.Identity.Name);
                    for (int i = 0; i < PreguntasId.Length; i++)
                    {
                        provider.InsertQuestionUser(RespuestasText[i], Convert.ToInt32(PreguntasId[i]), this.User.Identity.Name);
                    }

                    provider.UpdateUserIsResetPassword(this.User.Identity.Name);
                }
                else
                {
                    provider.DeleteAllAnswerByUser(par["HiddenuserName"]);
                    for (int i = 0; i < PreguntasId.Length; i++)
                    {
                        provider.InsertQuestionUser(RespuestasText[i], Convert.ToInt32(PreguntasId[i]), par["HiddenuserName"]);

                    }

                    provider.UpdateUserIsResetPassword(par["HiddenuserName"]);

                    FormsAuthentication.SetAuthCookie(par["HiddenuserName"], true);
                }

                return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", ex.Message);

                LoginViewModel Newmodel = new LoginViewModel();
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult m_btValidate_Click(string m_tbCaptcha)
        {
            try
            {
                if (m_tbCaptcha == Session["captchaText"].ToString())
                {
                    return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(JsonResponseFactory.ErrorResponse(), JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(JsonResponseFactory.ErrorResponse(), JsonRequestBehavior.AllowGet);
            }
        }
        private void PopulateCreateUserDropDowns(CreateUserViewModel model)
        {
            //TODO: Sacar las URLs del servicio al web.config
            model.HierarchyLevels = new List<SelectListItem>();
            WebClient wc = new WebClient();
            XDocument xdoc = XDocument.Parse(wc.DownloadString(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString() + "GetHierarchiesLevels")));
            List<string> list = xdoc.Root.Descendants(((XNamespace)@"http://schemas.microsoft.com/ado/2007/08/dataservices") + "element").Select(xe => xe.Value).ToList();
            foreach (var level in list)
            {
                model.HierarchyLevels.Add(new SelectListItem { Text = level, Value = level });
            }

            IOrderedQueryable<STPC.DynamicForms.Web.Services.Entities.Hierarchy> modelList;
            if (string.IsNullOrEmpty(model.HierarchyId))
            {
                modelList = db.Hierarchies.Where(r => r.Level == model.HierarchyLevels.First().Text).OrderBy(z => z.Name);
            }
            else
            {
                string selectedlevel = db.Hierarchies.Where(i => i.Id == int.Parse(model.HierarchyId)).First().Level;
                modelList = db.Hierarchies.Where(r => r.Level == selectedlevel).OrderBy(z => z.Name);
            }

            List<SelectListItem> modelData = modelList.Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            }).ToList();
            model.Hierarchies = modelData;

            model.AppRoles = Roles.GetAllRoles();
        }

        [HttpPost]
        public ActionResult Register_Cookie(string userName, string Idtype)
        {
            try
            {


                HttpCookie myCookie = new HttpCookie("RegisterMachine_" + Idtype + userName);
                DateTime now = DateTime.Now;

                // Set the cookie value.
                myCookie.Value = Idtype + userName;
                // Set the cookie expiration date.
                myCookie.Expires = now.AddMonths(6);

                // Add the cookie.
                Response.Cookies.Add(myCookie);
                Session["Logged"] = "true";

                return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Register_User(string userName, string Idtype)
        {
            try
            {

                Session["Logged"] = "true";

                return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Reset_Pwd(string userName)
        {
            try
            {
                string newPassWord = string.Empty;

                newPassWord = provider.ResetPassword(userName);
                provider.UpdateFiedldIsResetPassword(userName, true);

                STPC.DynamicForms.Infraestructure.Email _email = new Infraestructure.Email();

                SendEmail(provider.GetUser(userName), newPassWord);

                return RedirectToAction("List");

            }
            catch (Exception ex)
            {
                ModelState.Clear();
                ModelState.AddModelError("", ex.Message);

                LoginViewModel Newmodel = new LoginViewModel();
                return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }
    }
}