using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace STPC.DynamicForms.Web.RT.Controllers
{
    public class SharedController : Controller
    {
        //
        // GET: /Shared/

		 public ActionResult ErrorDetail(string errorMessage,string controler,string action)
        {
			  return View("_ErrorDetail", new HandleErrorInfo(new Exception(errorMessage), controler, action));
			  
        }

         public ActionResult _ErrorGeneral(string id)
        {
            Exception exc = Server.GetLastError();
            return View("_ErrorGeneral", (object)id);
			  
        }

    }
}
