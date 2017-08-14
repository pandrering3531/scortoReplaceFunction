using STPC.DynamicForms.Web.Common;
using STPC.DynamicForms.Web.Common.Services.Users;
using STPC.DynamicForms.Web.RT.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;

namespace STPC.DynamicForms.Web.RT.Controllers
{
    
    
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    public class ValidationController : Controller
    {
        CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
        Services.Entities.STPC_FormsFormEntities db = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

        [HttpGet]
        public JsonResult IsAplicationNameExist(string AplicationName)
        {
            var lista = AplicationNameManager.LoadAplicationName(db);
            bool isExist = lista.Where(u => u.Text.ToLowerInvariant().Equals(AplicationName.ToLower())).FirstOrDefault() == null;
            return Json(!isExist, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult IsUserNameExist(string IDNumber, string IDType)
        {
            bool isExist = true;
            using (UserServiceClient srv = new UserServiceClient())
            {
                var user = srv.GetUser(IDType + IDNumber, false);
                if (user == null)
                    isExist = false;
            }
            return Json(!isExist, JsonRequestBehavior.AllowGet);
        }

    }
}
