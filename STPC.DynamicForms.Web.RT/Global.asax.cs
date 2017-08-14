using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Common;
using System.Web.Security;

namespace STPC.DynamicForms.Web.RT
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			routes.IgnoreRoute("favicon.ico");
			routes.IgnoreRoute("robots.txt");
			routes.MapPageRoute("Report", "Reports/Report", "~/Reports/Report.aspx", true, null, new RouteValueDictionary { { "outgoing", new MyCustomConstaint() } });
			routes.MapPageRoute("Error", "Shared/globalError/{handler}", "~/Views/Shared/Error.aspx", true, null, new RouteValueDictionary { { "outgoing", new MyCustomConstaint() } });
			routes.MapRoute(
				 "Default", // Route name
				 "{controller}/{action}/{id}", // URL with parameters
				 new { controller = "Account", action = "LogOn", id = UrlParameter.Optional } // Parameter defaults
			);

		}

		void Application_Error(object sender, EventArgs e)
		{
			Exception exc = Server.GetLastError();

			ILogging eventWriter = LoggingFactory.GetInstance();
			eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}, InnerException: {2}, Source: {3}", exc.Message, exc.StackTrace, exc.TargetSite.Name, exc.Source));
			Response.RedirectPermanent("~/Shared/globalError/Application_Error%20-%20Global", true);
		}

		protected void Application_Start()
		{
			Application["UsersOnline"] = 0;

			//AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
			MvcHandler.DisableMvcResponseHeader = true;
		}

		public void Session_OnEnd()
		{

			Application.Lock();
			Application["UsersOnline"] = (int)Application["UsersOnline"] - 1;
			Application.UnLock();
		}

		public void Session_OnStart()
		{
			Application.Lock();
			Application["UsersOnline"] = (int)Application["UsersOnline"] + 1;
			Application.UnLock();
		}

		//protected void Session_End(object sender, EventArgs e)
		//{

		//}
	}

	public class MyCustomConstaint : IRouteConstraint
	{
		public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			return routeDirection == RouteDirection.IncomingRequest;
		}
	}
}