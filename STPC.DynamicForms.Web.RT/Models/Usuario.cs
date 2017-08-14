using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

namespace STPC.DynamicForms.Web.RT.Models
{
    public class Usuario
    {
        public string tipoid { get; set; }
        public string identificacion { get; set; }
        public string correo { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string movil { get; set; }
        public string nivel { get; set; }
        public string jerarquia { get; set; }
        public string perfil { get; set; }
        public string contraseña { get; set; }
        public string validacion { get; set; }

        CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
        Services.Entities.STPC_FormsFormEntities db = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));


        public bool existeUsuario()
        {
            bool isExist = true;
            using (UserServiceClient srv = new UserServiceClient())
            {
                var user = srv.GetUser(tipoid + identificacion, false);
                if (user == null)
                    isExist = false;
            }
            return isExist;
        }
    }


}