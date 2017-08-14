using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using STPC.DynamicForms.Core;
using STPC.DynamicForms.Infraestructure;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Common;
using STPC.DynamicForms.Web.Common.Messages;
using STPC.DynamicForms.Web.Common.Services.Users;
using STPC.DynamicForms.Web.RT.Helpers;
using STPC.DynamicForms.Web.RT.Models;
using STPC.DynamicForms.Web.RT.Services.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Linq;

namespace STPC.DynamicForms.Web.RT.Controllers
{

    [HandleError]
    public class AccountController : Controller
    {
        #region provider

        CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
        Services.Entities.STPC_FormsFormEntities db = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

        #endregion

        #region ActionResult

        #region ChangePassword

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

        #endregion

        #region LogOn

        public ActionResult LogOn()
        {

            if (this.User.Identity.IsAuthenticated)
            {

                return RedirectToAction("LogOff", "Account");
            }
            else
            {
                HttpCookie sessionCookie = Request.Cookies["ASP.NET_SessionId"];


                if (!this.ControllerContext.HttpContext.Request.IsAjaxRequest())
                    return View(new LoginViewModel());
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Content("EndSesion", MediaTypeNames.Text.Plain);
                    //return Json(new { success = false, Message = "EndSesion" });
                }

                //return View(new LoginViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAttribute]
        public ActionResult LogOn(string Username, string Idtype, string Password, string returnUrl, bool viewCpatcha)
        {
            bool showCaptcha = false;

            try
            {
                string c = Request.UserHostAddress;
                int iattemptCount = 0;
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
                                        Session["OpenSesion"] = true;
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
                                        Session["UserName"] = User.Identity.Name;
                                        return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                      var theuser = provider.GetUser(Idtype + Username);
                                      provider.CloseSesion(Idtype + Username);
                                        return Json(JsonResponseFactory.ErrorResponse(Constants.ERROR_MAX_ATTEMPT_ALLOWED, "E002", showCaptcha), JsonRequestBehavior.AllowGet);
                                    }

                                }
                                //}

                            }
                            else
                            {

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

                bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
                Guid correlationID = Guid.NewGuid();

                ILogging eventWriter = LoggingFactory.GetInstance();
                string errorMessage = string.Format(CustomMessages.E0007, "AccountController", "LogOn", correlationID, ex.Message);
                System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
                eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

                return Json(JsonResponseFactory.ErrorResponse(ex.Message, showCaptcha), JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Register

        [Authorize(Roles = "Administrador, Co-Administrador")]
        public ActionResult Register()
        {
            try
            {
                var model = new CreateUserViewModel();

                PopdlateCreateUserDropDowns(model);

                int? aplicationNameIdUser = validateSingleTenantAndRoleUser();

                ViewBag.aplicationNameIdUser = aplicationNameIdUser;

                ValidateSingleTenant();

                return View(model);
            }
            catch (Exception Ex)
            {

                return ErrorLogManagment(Ex, "Register");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Co-Administrador")]
        public ActionResult Register(CreateUserViewModel model, FormCollection par)
        {
            try
            {
                //Valida si llega el AplicationName del usuario
                int? aplicationNameIdUser;

                if (par.AllKeys.Contains("ddlAplicationName") == true)
                {

                    if (par["ddlAplicationName"].ToString() == string.Empty)
                        aplicationNameIdUser = null;
                    else
                        if (par["ddlAplicationName"].ToString() != "0")
                            aplicationNameIdUser = Convert.ToInt32(par["ddlAplicationName"].ToString());
                        else
                            aplicationNameIdUser = null;

                }
                else
                {
                    int IsSingleTenant = 0;

                    if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
                    {
                        IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

                    }

                    bool isAdmon = false;
                    if (Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador"))
                    {
                        isAdmon = true;
                    }

                    if (isAdmon && IsSingleTenant == 1)
                    {
                        aplicationNameIdUser = null;
                    }
                    if (!isAdmon && IsSingleTenant == 1)
                    {
                        var theuser = provider.GetUser(this.User.Identity.Name);
                        aplicationNameIdUser = theuser.AplicationNameId;
                    }
                    else
                        aplicationNameIdUser = null;
                }

                var user = new STPC.DynamicForms.Web.Common.Services.Users.User
                {
                    AplicationNameId = aplicationNameIdUser,
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
                    SendEmailNewUser(user, model.User.Password);
                    return RedirectToAction("List");
                }
                else
                    ModelState.AddModelError("IDNumber", status.ToString());

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

        #endregion

        #region HttpPost

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

        [HttpPost]
        public ActionResult RefreshToken()
        {
            return PartialView("_AntiForgeryToken");
        }

        [HttpPost]
        [Authorize]
        public ActionResult ListPaged(int pageIndex, int PageSize)
        {
            try
            {
                int totalRecords = 0;
                var model = provider.GetAllUsersPaged(pageIndex, PageSize, out totalRecords).ToList();
                ViewBag.totalPages = ((totalRecords / PageSize) + 1).ToString();

                int IsSingleTenant = 0;
                int? aplicationNameIdUser;

                if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
                {
                    IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

                }

                if (!Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador") && IsSingleTenant == 1)
                {

                    var theuser = provider.GetUser(this.User.Identity.Name);
                    aplicationNameIdUser = theuser.AplicationNameId;
                    return PartialView(model.Where(e => e.AplicationNameId == aplicationNameIdUser).ToList());
                }
                else
                    return PartialView(model);


            }
            catch (Exception ex)
            {
                bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
                Guid correlationID = Guid.NewGuid();

                ILogging eventWriter = LoggingFactory.GetInstance();
                string errorMessage = string.Format(CustomMessages.E0007, "AccountController", "ListPaged", correlationID, ex.Message);
                System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
                eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
                if (ShowErrorDetail)
                {
                    return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(errorMessage), "FormPageController", "GetPublicResource"));
                }
                else
                {
                    return PartialView("_ErrorGeneral", string.Format(CustomMessages.E0001, correlationID.ToString()));
                }
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult ListPagedAndFilter(int pageIndex, int PageSize, string searchText)
        {
            try
            {
                int totalRecords = 0;
                int totalPages = 0;
                var model = provider.GetAllUsersPaged(pageIndex, PageSize, out totalRecords, out totalPages, searchText).ToList();
                ViewBag.totalPages = ((totalRecords / PageSize) + 1).ToString();

                int IsSingleTenant = 0;
                int? aplicationNameIdUser;

                if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
                {
                    IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

                }

                if (!Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador") && IsSingleTenant == 1)
                {

                    var theuser = provider.GetUser(this.User.Identity.Name);
                    aplicationNameIdUser = theuser.AplicationNameId;
                    return PartialView("ListPaged", model.Where(e => e.AplicationNameId == aplicationNameIdUser).ToList());
                }
                else
                    return PartialView("ListPaged", model);



            }
            catch (Exception ex)
            {

                return ErrorLogManagment(ex, "ListPagedAndFilter");
            }
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
        [ValidateAntiForgeryTokenAttribute]
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

        [Authorize]
        [HttpPost]
        public ActionResult Edit(CreateUserViewModel model, FormCollection par)
        {
            //ModelState["User.Password"].Errors.Clear();
            //ModelState["User.PasswordQuestion"].Errors.Clear();
            //ModelState["User.PasswordAnswer"].Errors.Clear();
            //if (ModelState.IsValid)
            //{
            //if (model.User.Username == this.User.Identity.Name || Roles.IsUserInRole("Administrador"))
            //{




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
            serviceUser.IsApproved = true;
            int IsSingleTenant = 0;
            int? aplicationNameIdUser;
            //Valida si llega el AplicationName del usuario
            if (par.AllKeys.Contains("ddlAplicationName") == true)
            {

                if (par["ddlAplicationName"].ToString() == string.Empty)
                    aplicationNameIdUser = null;
                else
                    if (par["ddlAplicationName"].ToString() != "0")
                        aplicationNameIdUser = Convert.ToInt32(par["ddlAplicationName"].ToString());
                    else
                        aplicationNameIdUser = null;

                serviceUser.AplicationNameId = aplicationNameIdUser;

            }
            else
            {


                if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
                {
                    IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

                }
                bool isAdmon = false;
                if (Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador"))
                {
                    isAdmon = true;
                }

                if (isAdmon && IsSingleTenant == 1)
                {
                    serviceUser.AplicationNameId = null;
                }
                if (!isAdmon && IsSingleTenant == 1)
                {
                    var theuser = provider.GetUser(this.User.Identity.Name);
                    serviceUser.AplicationNameId = theuser.AplicationNameId;
                }
                else
                    serviceUser.AplicationNameId = null;

            }

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
            //}
            //else { return View("NotAuthorized"); }
            //}

            model.UserRoles = Roles.GetRolesForUser(model.User.Username);


            PopdlateCreateUserDropDowns(model);
            return View(model);
        }

        #endregion

        #region ActionResult

        [Authorize(Roles = "Administrador, Co-Administrador")]
        public ActionResult Reset_Pwd(string userName)
        {
            try
            {
                string newPassWord = string.Empty;

                newPassWord = provider.ResetPassword(userName);
                provider.UpdateFiedldIsResetPassword(userName, true);

                STPC.DynamicForms.Infraestructure.Email _email = new Infraestructure.Email();

                SendEmailResetUser(provider.GetUser(userName), newPassWord);

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

        [Authorize(Roles = "Administrador, Co-Administrador")]
        public ActionResult Details(string id)
        {
            ViewBag.LoggedUserIsAdmin = true;

            var theuser = provider.GetUser(id);

            if (!Roles.IsUserInRole("Administrador"))
            {
                foreach (var item in theuser.Roles)
                {
                    if (item.Rolename == "Administrador")
                    {
                        ViewBag.LoggedUserIsAdmin = false;
                        break;
                    }
                }
            }
            return View(theuser);

        }

        [Authorize]
        [Authorize(Roles = "Administrador, Co-Administrador")]
        public ActionResult Edit(string id)
        {
            //if (id == this.User.Identity.Name)
            //{
            var model = new CreateUserViewModel();
            var user = provider.GetUser(id);
            ViewBag.LoggedUserIsAdmin = Roles.IsUserInRole("Administrador");
            PopdlateCreateUserDropDowns(model, user.Hierarchy.Level, user.Hierarchy.Id);

            model.User = new STPC.DynamicForms.Web.RT.Models.User
            {
                AplicationNameId = user.AplicationNameId,
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                GivenName = user.GivenName,
                Hierarchy = user.Hierarchy,
                LastName = user.LastName,
                Phone_LandLine = user.Phone_LandLine,
                Phone_Mobile = user.Phone_Mobile
            };
            ViewBag.userHierarchy = user.Hierarchy.Id;
            List<string> listRoles = model.AppRoles.ToList();

            model.UserRoles = Roles.GetRolesForUser(user.Username);

            ViewBag.Level = user.Hierarchy.Level;

            if (model.UserRoles.Contains("Administrador"))
            {
                model.AppRoles = Roles.GetAllRoles();

            }
            else
            {
                listRoles.Remove("Administrador");
                model.AppRoles = listRoles.ToArray();

            }

            int? aplicationNameIdUser = validateSingleTenantAndRoleUser();

            ViewBag.aplicationNameIdUser = aplicationNameIdUser;


            ExtractIdTypeAndNumberFromUsername(model);

            ValidateSingleTenant();


            return View(model);
            //}
            //return View("NotAuthorized");
        }

        [Authorize]
        public ActionResult List()
        {
            try
            {
                int totalRecords = 0;
                var model = provider.GetAllUsersPaged(1, 20, out totalRecords).ToList();

                int IsSingleTenant = 0;
                int? aplicationNameIdUser;

                if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
                {
                    IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

                }

                if (!Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador") && IsSingleTenant == 1)
                {

                    var theuser = provider.GetUser(this.User.Identity.Name);
                    aplicationNameIdUser = theuser.AplicationNameId;
                    return View(model.Where(e => e.AplicationNameId == aplicationNameIdUser).ToList());
                }
                else
                    return View(model);
            }
            catch (Exception ex)
            {
                bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
                Guid correlationID = Guid.NewGuid();

                ILogging eventWriter = LoggingFactory.GetInstance();
                string errorMessage = string.Format(CustomMessages.E0007, "AccountController", "ListPaged", correlationID, ex.Message);
                System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
                eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
                if (ShowErrorDetail)
                {
                    return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(errorMessage), "FormPageController", "GetPublicResource"));
                }
                else
                {
                    return PartialView("_ErrorGeneral", string.Format(CustomMessages.E0001, correlationID.ToString()));
                }
            }
        }

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

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

            return RedirectToLogOffPage();

        }

        public ActionResult LogOff()
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

            return RedirectToLogOffPage();

        }

        public ActionResult asyncLogOff()
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

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

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
                        Session["attemptCount"] = "0";
                        Session["Logged"] = "true";
                        Session["OpenSesion"] = true;

                        if (provider.ValidateExternalUser(e))
                            return RedirectToAction("MenuAuth", "Home", model);
                        else
                            return RedirectToLogOffPage();
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

        public ActionResult Recover_Password()
        {


            //var list = provider.GetUserResetQuestion(this.User.Identity.Name);


            return View(new LoginViewModel());
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

        public ActionResult m_btValidate_Click(string m_tbCaptcha)
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

        public ActionResult CloseSesionUser(string userName)
        {
            try
            {

                var theuser = provider.GetUser(userName);
                provider.CloseSesion(userName);

                if (userName == this.User.Identity.Name)
                {
                    LogOff();
                }


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

        private ActionResult ErrorLogManagment(Exception Ex, string method)
        {
            #region Validacion de errores

            bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
            Guid correlationID = Guid.NewGuid();

            ViewBag.correlationID = correlationID;

            // enmascarar la excepcion
            #region Exception, No existe store procedure

            if (Ex.Message.Contains("Could not find stored procedure"))
            {
                // crear registro de error en el visor de sucesos
                EventLogRegister.WriteEventLog(string.Format("Ha ocurrido un error, el o los objetos {0} no existen o no han sido creados en la base de datos. Por favor comunicarse con el administrador", Ex.Message.Split(' ')[Ex.Message.Split(' ').Length - 1]), "PanelController", "Respond", correlationID);

                if (ShowErrorDetail)
                    return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(string.Format("Ha ocurrido un error, el o los objetos {0} no existen o no han sido creados en la base de datos. Por favor comunicarse con el administrador", Ex.Message.Split(' ')[Ex.Message.Split(' ').Length - 1])), "Panel Controller", "Respond"));

                return PartialView("_ErrorGeneral", "Ha ocurrido un error procesando su solicitud. Por favor comunicarse con el administrador" + "\n" + "Código del error: " + correlationID.ToString());
            }

            #endregion Exception, No existe store procedure

            #region Exception, No existe tabla

            else if (Ex.Message.Contains("Invalid object name"))
            {
                // crear registro de error en el visor de sucesos
                EventLogRegister.WriteEventLog(string.Format("Ha ocurrido un error, el o los objetos {0} no existen o no han sido creados en la base de datos. Por favor comunicarse con el administrador", Ex.Message.Split(' ')[Ex.Message.Split(' ').Length - 1]), "PanelController", "Respond", correlationID);

                if (ShowErrorDetail)
                    return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(string.Format("Ha ocurrido un error, el o los objetos {0} no existen o no han sido creados en la base de datos. Por favor comunicarse con el administrador", Ex.Message.Split(' ')[Ex.Message.Split(' ').Length - 1])), "Panel Controller", "Respond"));

                return PartialView("_ErrorGeneral", "Ha ocurrido un error procesando su solicitud. Por favor comunicarse con el administrador" + "\n" + "Código del error: " + correlationID.ToString());
            }

            #endregion Exception, No existe tabla

            #region Excepcion no clasificada

            else
            {
                // crear registro de error en el visor de sucesos

                if (Ex.InnerException != null)
                    EventLogRegister.WriteEventLog(Ex.Message + " detail " + Ex.InnerException.Message, "AccountController", method, Guid.NewGuid());
                else
                    EventLogRegister.WriteEventLog(Ex.Message, "AccountController", method, Guid.NewGuid());

                if (ShowErrorDetail)
                    return PartialView("_ErrorDetail", new HandleErrorInfo(Ex, "Account Controller", method));

                return PartialView("_ErrorGeneral", "Ha ocurrido un error procesando su solicitud. Por favor comunicarse con el administrador" + "\n" + "Código del error: " + correlationID.ToString());
            }

            #endregion Excepcion no clasificada

            #endregion Validacion de errores
        }

        #endregion

        #endregion

        #region JsonResult

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

        [HttpPost]
        public JsonResult GetConfigInfo()
        {

            try
            {
                string SessionWarning = System.Configuration.ConfigurationManager.AppSettings["SessionWarning"];
                double timeOut = provider.GetTimeOut();

                string[] result = new string[2];

                result[0] = SessionWarning;
                result[1] = timeOut.ToString();

                return this.Json(result);
            }
            catch
            {
                return this.Json(string.Empty);
            }
        }

        [HttpPost]
        public JsonResult GetExpirationDate()
        {

            try
            {

                FormsIdentity identity = this.User.Identity as FormsIdentity;

                if (identity != null)
                {
                    FormsAuthenticationTicket ticket = identity.Ticket;


                    DateTime oldDate = DateTime.Now;
                    DateTime newDate = ticket.Expiration;

                    // Difference in days, hours, and minutes.
                    TimeSpan ts = newDate - oldDate;

                    // Difference in days.
                    double differenceInDays = ts.TotalMilliseconds;

                    return this.Json(differenceInDays.ToString());
                }
                else
                    return this.Json(string.Empty);


            }
            catch
            {
                return this.Json(string.Empty);
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetHierarchyLevelByAplicationName(int AplicationNameid, string levelUser)
        {
            List<HierarchyNodeType> modelList;

            List<string> list = getLevelByAplicationId(AplicationNameid);

            List<SelectListItem> HierarchyLevels = new List<SelectListItem>();

            foreach (var level in list)
            {
                if (levelUser == level)
                    HierarchyLevels.Add(new SelectListItem { Text = level, Value = level, Selected = true });
                else
                    HierarchyLevels.Add(new SelectListItem { Text = level, Value = level, Selected = false });
            }

            return Json(HierarchyLevels, JsonRequestBehavior.AllowGet);


        }

        #endregion

        #region Methods

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

            IOrderedQueryable<STPC.DynamicForms.Web.RT.Services.Entities.Hierarchy> modelList;
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


        private void SendEmailNewUser(STPC.DynamicForms.Web.Common.Services.Users.User user, string newPassWord)
        {
            string server = ConfigurationManager.AppSettings["Server"].ToString();
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            string userName = ConfigurationManager.AppSettings["User"].ToString();
            string password = ConfigurationManager.AppSettings["Password"].ToString();
            string domain = ConfigurationManager.AppSettings["Domain"].ToString();
            string from = ConfigurationManager.AppSettings["From"].ToString();
            string url = ConfigurationManager.AppSettings["ValidateEmailURL"].ToString();
            string Subjet = ConfigurationManager.AppSettings["Subjet"].ToString();
            string alias = ConfigurationManager.AppSettings["Alias"].ToString();

            string Body = "<h3>Se&ntilde;or(a): " + user.GivenName + " " + user.LastName + "</h3><p>Bienvenido a la plataforma. Al ser su registro inicial, nuestro sistema le ha generado la siguiente contrase&ntilde;a:</p><p><strong>" + newPassWord + "</strong></p><p>Por su seguridad debe cambiar la contrase&ntilde;a y generar las preguntas de seguridad para poder continuar con el proceso de cr&eacute;dito.</p>";

            Email email = new Email(server, port, userName, password, domain, alias);

            email.SendMail(from, user.Email, string.Empty, Subjet, Body);
        }

        private void SendEmailResetUser(STPC.DynamicForms.Web.Common.Services.Users.User user, string newPassWord)
        {
            string server = ConfigurationManager.AppSettings["Server"].ToString();
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            string userName = ConfigurationManager.AppSettings["User"].ToString();
            string password = ConfigurationManager.AppSettings["Password"].ToString();
            string domain = ConfigurationManager.AppSettings["Domain"].ToString();
            string from = ConfigurationManager.AppSettings["From"].ToString();
            string url = ConfigurationManager.AppSettings["ValidateEmailURL"].ToString();
            string Subjet = ConfigurationManager.AppSettings["Subjet"].ToString();
            string alias = ConfigurationManager.AppSettings["Alias"].ToString();

            string Body = "<h3>Se&ntilde;or(a): " + user.GivenName + " " + user.LastName + "</h3><p>Se ha restablecido su contrase&ntilde;a</p><p><strong>" + newPassWord + "</strong></p><p>Por su seguridad, no olvide&nbsp;cambiar la contrase&ntilde;a para poder continuar con el proceso de cr&eacute;dito.</p>";

            Email email = new Email(server, port, userName, password, domain, alias);

            email.SendMail(from, user.Email, string.Empty, Subjet, Body);
        }

        private int? validateSingleTenantAndRoleUser()
        {
            int IsSingleTenant = 0;
            int? aplicationNameIdUser;
            bool isAdmon = false;
            if (Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador"))
            {
                isAdmon = true;
            }

            if (isAdmon && IsSingleTenant == 1)
            {
                aplicationNameIdUser = 0;
            }
            if (!isAdmon && IsSingleTenant == 1)
            {
                var theuser = provider.GetUser(this.User.Identity.Name);
                aplicationNameIdUser = theuser.AplicationNameId;
            }
            else
                aplicationNameIdUser = 0;
            return aplicationNameIdUser;
        }

        private void ValidateSingleTenant()
        {
            //Consulta si se maneja multiEmpresa
            int IsSingleTenant = 0;
            bool isAdmon = false;

            if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
            {
                IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

            }

            //Valida si el usuario Logeado es administrador
            if (Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador"))
            {
                isAdmon = true;
            }

            if (isAdmon && IsSingleTenant == 1)
            {
                ViewBag.IsSingleTenant = 1;
            }
            else
            {
                ViewBag.IsSingleTenant = 0;
            }

            ViewBag.listAplication = AplicationNameManager.LoadAplicationName(db);
        }

        private void ValidateSingleTenantAutoUser()
        {
            int IsSingleTenant = 0;

            if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
            {
                IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

            }

            if (IsSingleTenant == 1)
            {
                ViewBag.IsSingleTenant = 1;
            }
            else
            {
                ViewBag.IsSingleTenant = 0;
            }
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

        private void PopdlateCreateUserDropDownsSolicitante(CreateUserViewModel model)
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

            IOrderedQueryable<STPC.DynamicForms.Web.RT.Services.Entities.Hierarchy> modelList;
            if (string.IsNullOrEmpty(model.HierarchyId))
            {
                modelList = db.Hierarchies.Where(r => r.Level == model.HierarchyLevels.First().Text && r.IsActive == true).OrderBy(z => z.Name);
            }
            else
            {
                string selectedlevel = db.Hierarchies.Where(i => i.Id == int.Parse(model.HierarchyId)).First().Level;
                modelList = db.Hierarchies.Where(r => r.Level == selectedlevel && r.IsActive == true).OrderBy(z => z.Name);
            }

            List<SelectListItem> modelData = modelList.Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            }).ToList();
            model.Hierarchies = modelData;

            //model.AppRoles = Roles.GetAllRoles();

            //List<string> listRoles = Roles.GetAllRoles().ToList();

            //model.UserRoles = Roles.GetRolesForUser(this.User.Identity.Name);

            //    listRoles.Remove("Administrador");
            //    listRoles.Remove("Analista de Estructuración");
            //    listRoles.Remove("Analista de estudios económicos");
            //    listRoles.Remove("Asistente Comercial");
            //    listRoles.Remove("Asistente operativo");
            //    listRoles.Remove("Co-Administrador");
            //    listRoles.Remove("Cómite de crédito 1");
            //    listRoles.Remove("Cómite de Crédito 2");
            //    listRoles.Remove("Directora Gestión de Operaciones");
            //    listRoles.Remove("Gerente de Financiación Estructurada");
            //    listRoles.Remove("Gerente de negocio");
            //    listRoles.Remove("Gerente General");
            //    listRoles.Remove("Administrador");






            //    model.AppRoles = listRoles.ToArray();


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

            IOrderedQueryable<STPC.DynamicForms.Web.RT.Services.Entities.Hierarchy> modelList;
            if (string.IsNullOrEmpty(model.HierarchyId))
            {
                modelList = db.Hierarchies.Where(r => r.Level == model.HierarchyLevels.First().Text && r.IsActive == true).OrderBy(z => z.Name);
            }
            else
            {
                string selectedlevel = db.Hierarchies.Where(i => i.Id == int.Parse(model.HierarchyId)).First().Level;
                modelList = db.Hierarchies.Where(r => r.Level == selectedlevel && r.IsActive == true).OrderBy(z => z.Name);
            }

            List<SelectListItem> modelData = modelList.Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            }).ToList();
            model.Hierarchies = modelData;

            //model.AppRoles = Roles.GetAllRoles();

            List<string> listRoles = Roles.GetAllRoles().ToList();

            model.UserRoles = Roles.GetRolesForUser(this.User.Identity.Name);
            if (model.UserRoles.Contains("Administrador"))
                model.AppRoles = Roles.GetAllRoles();
            else
            {
                listRoles.Remove("Administrador");
                model.AppRoles = listRoles.ToArray();

            }
        }

        private void PopdlateCreateUserDropDowns(CreateUserViewModel model, string levelUser, int hierarchyIdUser)
        {
            //TODO: Sacar las URLs del servicio al web.config
            model.HierarchyLevels = new List<SelectListItem>();
            //List<string> list = getLevelByAplicationId();

            //foreach (var level in list)
            //{
            //	if (levelUser == level)
            //		model.HierarchyLevels.Add(new SelectListItem { Text = level, Value = level, Selected = true });
            //	else
            //		model.HierarchyLevels.Add(new SelectListItem { Text = level, Value = level, Selected = false });
            //}

            IOrderedQueryable<STPC.DynamicForms.Web.RT.Services.Entities.Hierarchy> modelList;

            if (string.IsNullOrEmpty(model.HierarchyId))
            {
                modelList = db.Hierarchies.Where(r => r.Level == levelUser).OrderBy(z => z.Name);
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

            foreach (var item in modelData)
            {
                if (hierarchyIdUser.ToString().Equals(item.Value))
                {
                    item.Selected = true;
                }
                else
                    item.Selected = false;
            }
            model.Hierarchies = modelData;

            model.AppRoles = Roles.GetAllRoles();
        }

        private List<string> getLevelByAplicationId(int aplicationNameId = 0)
        {

            WebClient wc = new WebClient();
            XDocument xdoc = null;

            if (aplicationNameId == 0)
            {
                xdoc = XDocument.Parse(wc.DownloadString(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString() + "GetHierarchiesLevels")));
            }
            else
            {
                xdoc = XDocument.Parse(wc.DownloadString(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString() + "GetHierarchiesLevelsByAplicatioName" + "?aplicationNameId=" + aplicationNameId)));
            }

            List<string> list = xdoc.Root.Descendants(((XNamespace)@"http://schemas.microsoft.com/ado/2007/08/dataservices") + "element").Select(xe => xe.Value).ToList();


            return list;
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

        private void SendEmail(STPC.DynamicForms.Web.Common.Services.Users.User user)
        {
            string server = ConfigurationManager.AppSettings["Server"].ToString();
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            string userName = ConfigurationManager.AppSettings["User"].ToString();
            string password = ConfigurationManager.AppSettings["Password"].ToString();
            string domain = ConfigurationManager.AppSettings["Domain"].ToString();
            string from = ConfigurationManager.AppSettings["From"].ToString();
            string url = ConfigurationManager.AppSettings["ValidateEmailURL"].ToString();
            string alias = ConfigurationManager.AppSettings["Alias"].ToString();


            string Body = GetURL(user);

            Email email = new Email(server, port, userName, password, domain, alias);

            email.SendMail(from, user.Email, string.Empty, string.Empty, Body);
        }

        private string GetURL(STPC.DynamicForms.Web.Common.Services.Users.User user)
        {
            string url = ConfigurationManager.AppSettings["ValidateEmailURL"].ToString();
            string newURL = string.Empty;

            newURL = string.Format(url, user.Username, "&", user.Token);

            return newURL;
        }

        #endregion

        #region Hecho por LiSim

        #region Creacion Autousuario
        //Creacion de usuarios con perfil de solicitante para poder hacer solicitudes de forma autonoma

        #region Autousuario

        public ActionResult AutoUser()
        {
            try
            {
                var model = new AutoUserViewModel();
                ValidateSingleTenantAutoUser();
                return View(model);
            }
            catch (Exception Ex)
            {

                return ErrorLogManagment(Ex, "Register");
            }
        }

        [HttpPost]
        public ActionResult AutoUser(AutoUserViewModel model, FormCollection par)
        {
            try
            {
                CustomMembershipProvider pc = (CustomMembershipProvider)Membership.Providers["CustomMembershipProvider"];
                int numeric = pc.MinRequiredNumericCharacters;
                int special = pc.MinRequiredNonAlphanumericCharacters;
                int upper = pc.MinRequiredUpperCharacters;
                int lower = pc.MinRequiredPasswordLength - pc.MinRequiredNumericCharacters - pc.MinRequiredNonAlphanumericCharacters - pc.MinRequiredUpperCharacters + 3;
                var lista = AplicationNameManager.LoadAplicationName(db);
                string pwdrnd = creapwd(numeric, upper, lower, special);
                //Valida si llega el AplicationName del usuario


                int? aplicationNameIdUser;
                int idjerarquia = 0;


                if (par.AllKeys.Contains("AplicationName") == true)
                {
                    string appid = par["AplicationName"].ToString() != null ? par["AplicationName"].ToString() : null;

                    if (appid == string.Empty)
                        aplicationNameIdUser = null;
                    else
                        if (appid != "0")
                        {
                            var result = lista.Where(u => u.Text.ToLowerInvariant().Equals(appid.ToLower())).FirstOrDefault();
                            aplicationNameIdUser = Convert.ToInt32(result.Value.ToString());

                            bool count = db.Hierarchies.Where(x => x.AplicationNameId == aplicationNameIdUser).FirstOrDefault() == null;
                            if (!count)
                            {
                                var jerarquia = db.Hierarchies.Where(r => r.Name == "NO PRESENCIAL" && r.AplicationNameId == aplicationNameIdUser && r.IsActive == true).FirstOrDefault();
                                if (jerarquia != null)
                                    idjerarquia = int.Parse(jerarquia.Id.ToString());
                                else
                                {
                                    var j = db.Hierarchies.Where(x => x.IsActive == true && x.AplicationNameId == aplicationNameIdUser).OrderBy(r => r.Id).FirstOrDefault();
                                    idjerarquia = int.Parse(j.Id.ToString());
                                }
                            }
                            else
                            {
                                var jerarquia = db.Hierarchies.Where(x => x.IsActive == true && x.AplicationNameId == aplicationNameIdUser).OrderBy(r => r.Id).FirstOrDefault();
                                idjerarquia = int.Parse(jerarquia.Id.ToString());
                            }

                        }
                        else
                        {
                            aplicationNameIdUser = null;
                            var jerarquia = db.Hierarchies.Where(x => x.IsActive == true).OrderBy(r => r.Id).FirstOrDefault();
                            idjerarquia = int.Parse(jerarquia.Id.ToString());
                        }
                }
                else
                {
                    aplicationNameIdUser = null;
                    var jerarquia = db.Hierarchies.Where(x => x.IsActive == true).OrderBy(r => r.Id).FirstOrDefault();
                    idjerarquia = int.Parse(jerarquia.Id.ToString());
                }





                var user = new STPC.DynamicForms.Web.Common.Services.Users.User
                {
                    AplicationNameId = aplicationNameIdUser,
                    Username = model.IDType + model.IDNumber,
                    Email = model.Email,
                    GivenName = model.GivenName,
                    LastName = model.LastName,
                    Password = pwdrnd,
                    Phone_LandLine = model.Phone_LandLine,
                    Phone_Mobile = model.Phone_Mobile,
                    Address = model.Address,
                    IsResetPassword = true,
                    IsApproved = true,
                    Hierarchy = new STPC.DynamicForms.Web.Common.Services.Users.Hierarchy { Id = idjerarquia }
                };

                MembershipCreateStatus status;
                provider.CreateUser(user, out status);
                if (status == MembershipCreateStatus.Success)
                {
                    Roles.AddUserToRole(user.Username, "Solicitante");
                    SendEmailNewUser(user, pwdrnd);
                    return RedirectToAction("LogOn");
                }
                else
                    ModelState.AddModelError("IDNumber", status.ToString());

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        #endregion

        #region crear password randomico

        private string generado(string posibles, int cantidad)
        {
            var ltM = posibles;
            var randomltM = new Random();
            var resultltM = new string(
                Enumerable.Repeat(ltM, cantidad)
                          .Select(s => s[randomltM.Next(s.Length)])
                          .ToArray());
            return resultltM;
        }

        private string randomic(string cadena)
        {
            string[] arra = cadena.Select(c => c.ToString()).ToArray();
            Random rnd = new Random();
            string[] MyRandomArray = arra.OrderBy(x => rnd.Next()).ToArray();
            var Pwd = new StringBuilder();
            for (int i = 0; i < MyRandomArray.Length; i++)
                Pwd.Append(MyRandomArray[i]);
            return Pwd.ToString();
        }

        public string creapwd(int numeros, int mayus, int minus, int especial)
        {

            string StrMayus, StrMinus, StrNumer, StrEspec;
            StrMayus = generado("ABCDEFGHIJKLMNOPQRSTUVWXYZ", mayus);
            StrMinus = generado("abcdefghijklmnopqrstuvwxyz", minus);
            StrNumer = generado("0123456789", numeros);
            StrEspec = generado("][?/<~#`!@$%^&*()+=}|:>{}", especial);
            string a = string.Format("{0}{1}{2}{3}", StrMayus, StrMinus, StrNumer, StrEspec);
            string PwdT = randomic(a);
            return PwdT;

        }

        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }

        #endregion

        #endregion

        #region Cargue Masivo

        public ActionResult massiveLoad()
        {
            var model = new MyViewModels();
            return View(model);
        }

        [HttpPost]
        [MultipleButton]
        public ActionResult Cargar(MyViewModels model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            DataTable dt = GetDataTableFromSpreadsheet(model.MyExcelFile.InputStream, false);
            var valida = new List<Usuario>();
            valida = GetListUsers(dt, "valido");
            var invalida = new List<Usuario>();
            invalida = GetListUsers(dt, "invalida");
            TempData["ListaOk"] = valida;
            string strContent = ConvertDataTableToHTMLTable(dt);
            model.MSExcelTable = strContent;
            return View(model);
        }


        [HttpPost]
        [MultipleButton]
        public ActionResult Aceptar()
        {
            var listaIN = (List<Usuario>)TempData["ListaOk"];
            if (listaIN != null)
            {
                if (listaIN.Count != 0)
                {
                    #region creacion de usuario batch
                    /*CREACION DE USUARIOS*/

                    try
                    {
                        foreach (var item in listaIN)
                        {
                            int idjerarquia = 0;
                            var jerarquia = db.Hierarchies.Where(r => r.Name == item.jerarquia && r.IsActive == true).FirstOrDefault();
                            if (jerarquia != null)
                                idjerarquia = int.Parse(jerarquia.Id.ToString());
                            else
                            {
                                var j = db.Hierarchies.Where(x => x.IsActive == true).OrderBy(r => r.Id).FirstOrDefault();
                                idjerarquia = int.Parse(j.Id.ToString());
                            }

                            var rol = db.Roles.Where(r => r.Rolename == item.perfil).FirstOrDefault();

                            var user = new STPC.DynamicForms.Web.Common.Services.Users.User
                            {
                                AplicationNameId = null,
                                Username = item.tipoid + item.identificacion,
                                Email = item.correo,
                                GivenName = item.nombres,
                                LastName = item.apellidos,
                                Password = item.contraseña,
                                Phone_LandLine = item.telefono,
                                Phone_Mobile = item.movil,
                                Address = item.direccion,
                                IsResetPassword = true,
                                IsApproved = true,
                                Hierarchy = new STPC.DynamicForms.Web.Common.Services.Users.Hierarchy { Id = idjerarquia }
                            };

                            MembershipCreateStatus status;
                            provider.CreateUser(user, out status);
                            if (status == MembershipCreateStatus.Success)
                            {
                                Roles.AddUserToRole(user.Username, item.perfil);
                                SendEmailNewUser(user, item.contraseña);
                            }
                            else
                                ModelState.AddModelError("IDNumber", status.ToString());

                        }
                        ViewData["Message"] = "Se han insertado " + listaIN.Count.ToString();
                        return View();

                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                        return View();
                    }

                    /*FIN DE CREACION DE USUARIOS*/
                    #endregion
                }
                else
                {

                    ViewData["Message"] = "No se inserto ningun registro";
                    return View("massiveLoad");
                }
            }
            else
                return View("massiveLoad");
        }

        public DataTable GetDataTableFromSpreadsheet(Stream MyExcelStream, bool ReadOnly)
        {

            DataTable dt = new DataTable();
            using (SpreadsheetDocument sDoc = SpreadsheetDocument.Open(MyExcelStream, ReadOnly))
            {
                WorkbookPart workbookPart = sDoc.WorkbookPart;
                IEnumerable<Sheet> sheets = sDoc.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                string relationshipId = sheets.First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)sDoc.WorkbookPart.GetPartById(relationshipId);
                Worksheet workSheet = worksheetPart.Worksheet;
                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                IEnumerable<Row> rows = sheetData.Descendants<Row>();

                CustomMembershipProvider pc = (CustomMembershipProvider)Membership.Providers["CustomMembershipProvider"];
                int numeric = pc.MinRequiredNumericCharacters;
                int special = pc.MinRequiredNonAlphanumericCharacters;
                int upper = pc.MinRequiredUpperCharacters;
                int lower = pc.MinRequiredPasswordLength - pc.MinRequiredNumericCharacters - pc.MinRequiredNonAlphanumericCharacters - pc.MinRequiredUpperCharacters + 3;


                foreach (Cell cell in rows.ElementAt(0))
                {
                    dt.Columns.Add(GetCellValue(sDoc, cell));
                }

                dt.Columns.Add("Contraseña");
                dt.Columns.Add("Aprobacion");

                foreach (Row row in rows) //this will also include your header row...
                {
                    DataRow tempRow = dt.NewRow();

                    for (int i = 0; i <= row.Descendants<Cell>().Count() + 1; i++)
                    {
                        if (i == row.Descendants<Cell>().Count() + 1)
                        {
                            bool isExist = true;
                            using (UserServiceClient srv = new UserServiceClient())
                            {
                                string nombreUsuario = tempRow[0].ToString() + tempRow[1].ToString();
                                var user = srv.GetUser(nombreUsuario, false);
                                if (user == null)
                                    isExist = false;
                            }
                            bool exists = dt.AsEnumerable().Where(c => c.Field<string>("Tipo de Identificacion").Equals(tempRow[0].ToString()) && c.Field<string>("Numero de Identificacion").Equals(tempRow[1].ToString())).Count() > 0;
                            if (!isExist && !exists)
                            {

                                Regex correo = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                                Regex telefono = new Regex(@"^[\+]{0,1}[1-9]{1}[0-9]{6}$");
                                Regex celular = new Regex(@"^3[0-9]{9}$");
                                // var role = db.Roles.Where(r => r.Rolename == tempRow[10].ToString()).FirstOrDefault();
                                var jerarquia = db.Hierarchies.Where(r => r.Name == tempRow[9].ToString() && r.IsActive == true).FirstOrDefault();
                                if (!correo.IsMatch(tempRow[2].ToString()))
                                    tempRow[i] = "CORREO INCORRECTO";
                                else
                                {
                                    if (tempRow[6].ToString() != string.Empty && tempRow[6].ToString() != "0" && !telefono.IsMatch(tempRow[6].ToString()) && !celular.IsMatch(tempRow[6].ToString()))
                                        tempRow[i] = "TELEFONO INCORRECTO";
                                    else
                                    {
                                        if (!celular.IsMatch(tempRow[7].ToString()))
                                            tempRow[i] = "CELULAR INCORRECTO";
                                        //else if (role == null)
                                        //    tempRow[i] = "NO EXISTE EL PERFIL";
                                        else if (jerarquia == null)
                                            tempRow[i] = "NO EXISTE LA JERARQUIA";
                                        else
                                            tempRow[i] = "REGISTRO CORRECTO";
                                    }
                                }

                            }
                            else if (exists)
                            {
                                tempRow[i] = "USUARIO REPETIDO EN EL ARCHIVO";
                            }
                            else
                                tempRow[i] = "USUARIO EXISTENTE EN EL APLICATIVO";

                        }
                        else if (i == row.Descendants<Cell>().Count())
                        {
                            GenerarPassword pwd = new GenerarPassword();
                            pwd.especial = special;
                            pwd.mayus = upper;
                            pwd.minus = lower;
                            pwd.numeros = numeric;
                            tempRow[i] = pwd.creapwd();
                        }
                        else
                        {
                            tempRow[i] = GetCellValue(sDoc, row.Descendants<Cell>().ElementAt(i));
                        }

                    }

                    dt.Rows.Add(tempRow);
                }

            }
            dt.Rows.RemoveAt(0);
            return dt;
        }

        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue != null ? cell.CellValue.InnerXml : "0";

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }

        public static string ConvertDataTableToHTMLTable(DataTable dt)
        {
            string ret = "";

            List<Usuario> o = new List<Usuario>();
            ret = "<div id=" + (char)34 + "dvForm" + (char)34 + "><main id=" + (char)34 + "center" + (char)34 + "><h1>Lista de Usuarios.</h1>";

            ret += "<table id=" + (char)34 + "TheTable" + (char)34 + " class=" + (char)34 + "resultados" + (char)34 + ">";

            ret += "<thead><tr>";
            foreach (DataColumn col in dt.Columns)
            {
                if (col.ColumnName != "Contraseña")
                    ret += "<th>" + col.ColumnName + "</th>";
            }
            ret += "</tr></thead><tbody>";

            foreach (DataRow row in dt.Rows)
            {
                Usuario usuario = new Usuario();
                ret += "<tr>";
                for (int i = 0; i < dt.Columns.Count; i++)
                {

                    if (i == 0)//tipo documento
                    {
                        if (row[i].ToString() == "CC" || row[i].ToString() == "CE" || row[i].ToString() == "NIP" || row[i].ToString() == "NIT")
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                            usuario.tipoid = row[i].ToString();
                        }
                        else
                        {
                            ret += "<td class=" + (char)34 + "otra" + (char)34 + ">" + row[i].ToString() + "</td>";
                        }

                    }
                    if (i == 1)//numero documento
                    {
                        if (row[i].ToString().Length < 255)
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                            usuario.identificacion = row[i].ToString();
                        }
                        else
                        {
                            ret += "<td class=" + (char)34 + "otra" + (char)34 + ">" + row[i].ToString() + "</td>";
                        }

                    }
                    if (i == 2)//correo electronico
                    {
                        Regex r = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                        if (r.IsMatch(row[i].ToString()))
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                            usuario.correo = row[i].ToString();
                        }
                        else
                        {
                            ret += "<td class=" + (char)34 + "otra" + (char)34 + ">" + row[i].ToString() + "</td>";
                        }

                    }
                    if (i == 3)//nombres
                    {
                        if (row[i].ToString().Length < 255)
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                            usuario.nombres = row[i].ToString();
                        }
                        else
                        {
                            ret += "<td class=" + (char)34 + "otra" + (char)34 + ">" + row[i].ToString() + "</td>";
                        }

                    }
                    if (i == 4)//apellidos
                    {
                        if (row[i].ToString().Length < 100)
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                            usuario.apellidos = row[i].ToString();
                        }
                        else
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                        }

                    }
                    if (i == 5)//direccion
                    {
                        if (row[i].ToString().Length < 100)
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                            usuario.direccion = row[i].ToString();
                        }
                        else
                        {
                            ret += "<td class=" + (char)34 + "otra" + (char)34 + ">" + row[i].ToString() + "</td>";
                        }

                    }
                    if (i == 6)//telefono
                    {
                        Regex r = new Regex(@"^3[0-9]{9}$");
                        Regex q = new Regex(@"^[\+]{0,1}[1-9]{1}[0-9]{6}$");
                        if (q.IsMatch(row[i].ToString()) || r.IsMatch(row[i].ToString()) || row[i].ToString() == "0" || row[i].ToString() == string.Empty)
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                            usuario.telefono = row[i].ToString();
                        }
                        else
                        {
                            ret += "<td class=" + (char)34 + "otra" + (char)34 + ">" + row[i].ToString() + "</td>";
                        }

                    }
                    if (i == 7)//celular
                    {
                        Regex r = new Regex(@"^3[0-9]{9}$");
                        if (r.IsMatch(row[i].ToString()))
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                            usuario.movil = row[i].ToString();
                        }
                        else
                        {
                            ret += "<td class=" + (char)34 + "otra" + (char)34 + ">" + row[i].ToString() + "</td>";
                        }

                    }
                    if (i == 8)//nivel
                    {
                        if (row[i].ToString().Length < 100)
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                            usuario.nivel = row[i].ToString();
                        }
                        else
                        {
                            ret += "<td class=" + (char)34 + "otra" + (char)34 + ">" + row[i].ToString() + "</td>";
                        }

                    }
                    if (i == 9)//jerarquia
                    {
                        if (row[i].ToString().Length < 100)
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                            usuario.jerarquia = row[i].ToString();
                        }
                        else
                        {
                            ret += "<td class=" + (char)34 + "otra" + (char)34 + ">" + row[i].ToString() + "</td>";
                        }

                    }
                    if (i == 10)//perfil
                    {
                        if (row[i].ToString().Length < 100)
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                            usuario.perfil = row[i].ToString();
                        }
                        else
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                        }

                    }

                    //if (i == 11)//contraseña
                    //{
                    //    ret += "<td>" + row[i].ToString() + "</td>";
                    //    usuario.contraseña = row[i].ToString();
                    //}

                    if (i == 12)//validacion
                    {
                        if (row[i].ToString() == "REGISTRO CORRECTO")
                        {
                            ret += "<td>" + row[i].ToString() + "</td>";
                            usuario.validacion = row[i].ToString();
                        }
                        else
                        {
                            ret += "<td class=" + (char)34 + "otra" + (char)34 + ">" + row[i].ToString() + "</td>";
                        }
                    }
                }

                if (usuario.tipoid != null && usuario.identificacion != null && usuario.correo != null && usuario.nombres != null && usuario.apellidos != null && usuario.direccion != null && usuario.telefono != null && usuario.movil != null && usuario.nivel != null && usuario.jerarquia != null && usuario.perfil != null && usuario.contraseña != null)
                {
                    if (!usuario.existeUsuario())
                    {
                        o.Add(usuario);
                    }

                }
                ret += "</tr>";
            }
            var count = o.Count;
            ret += "</tbody></table></main></div>";
            ret += "<br/>";
            return ret;
        }

        public static List<Usuario> GetListUsers(DataTable dt, string lista_seleccionada)
        {
            var dictionary = new Dictionary<string, List<Usuario>>();
            List<Usuario> ok = new List<Usuario>();
            List<Usuario> no = new List<Usuario>();

            ok = dt.AsEnumerable().Where(a => a.Field<string>("Aprobacion") == "REGISTRO CORRECTO").Select(dataRow => new Usuario
            {
                tipoid = dataRow.Field<string>("Tipo de Identificacion"),
                identificacion = dataRow.Field<string>("Numero de Identificacion"),
                correo = dataRow.Field<string>("Correo Electronico"),
                nombres = dataRow.Field<string>("Nombres"),
                apellidos = dataRow.Field<string>("Apellidos"),
                direccion = dataRow.Field<string>("Direccion"),
                telefono = dataRow.Field<string>("Telefono"),
                movil = dataRow.Field<string>("Movil"),
                nivel = dataRow.Field<string>("Nivel"),
                jerarquia = dataRow.Field<string>("Jerarquia"),
                perfil = dataRow.Field<string>("Perfil"),
                contraseña = dataRow.Field<string>("Contraseña"),
                validacion = dataRow.Field<string>("Aprobacion")
            }).ToList();

            no = dt.AsEnumerable().Where(a => a.Field<string>("Aprobacion") != "REGISTRO CORRECTO").Select(dataRow => new Usuario
            {
                tipoid = dataRow.Field<string>("Tipo de Identificacion"),
                identificacion = dataRow.Field<string>("Numero de Identificacion"),
                correo = dataRow.Field<string>("Correo Electronico"),
                nombres = dataRow.Field<string>("Nombres"),
                apellidos = dataRow.Field<string>("Apellidos"),
                direccion = dataRow.Field<string>("Direccion"),
                telefono = dataRow.Field<string>("Telefono"),
                movil = dataRow.Field<string>("Movil"),
                nivel = dataRow.Field<string>("Nivel"),
                jerarquia = dataRow.Field<string>("Jerarquia"),
                perfil = dataRow.Field<string>("Perfil"),
                contraseña = dataRow.Field<string>("Contraseña"),
                validacion = dataRow.Field<string>("Aprobacion")
            }).ToList();

            dictionary.Add("valido", ok);
            dictionary.Add("invalida", no);
            return dictionary[lista_seleccionada];

        }

        #endregion

        #endregion
    }
}