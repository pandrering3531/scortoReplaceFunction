using System.Web.Mvc;
namespace STPC.DynamicForms.Web.Controllers
{
	public class BaseController : Controller
	{

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{

			// If session exists

			if (filterContext.HttpContext.Session != null)
			{

				//if new session

				if (filterContext.HttpContext.Session.IsNewSession)
				{

					string cookie =

						 filterContext.HttpContext.Request.Headers["Cookie"];

					//if cookie exists and sessionid index is greater than zero

					if ((cookie != null) &&

						 (cookie.IndexOf("ASP.NET_SessionId") >= 0))
					{

						//redirect to desired session 

						//expiration action and controller

						filterContext.Result =

							 RedirectToAction("LogOn", "Account");

						return;

					}

				}

			}

			//otherwise continue with action

			base.OnActionExecuting(filterContext);

		}

	}
}