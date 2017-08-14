using System.Web.Mvc;
using System.Web.Security;
using STPC.DynamicForms.Web.Common;
using STPC.DynamicForms.Web.Models;

namespace STPC.DynamicForms.Web.Controllers
{
  [HandleError]
  public class HomeController : Controller
  {
    CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
    public ActionResult Index()
    {
      if (this.User.Identity.IsAuthenticated) return View("Menu");
      return View(new LoginViewModel());
    }


    [Authorize]
    public ActionResult Menu()
    {
		 var theuser = provider.GetUser(this.User.Identity.Name);
		 if (!theuser.IsOnLine || Session["Logged"] == null)
			 return RedirectToAction("LogOffIsOnline", "Account");

      return View();
    }
	 public JsonResult GetDaysToExpirePassword()
    {
      try
      {

        return this.Json(provider.EsxpirarDaysPassword((Common.Services.Users.User) (this.User.Identity)));
      }
      catch
      {
        return this.Json(string.Empty);
      }
    }
  }
}