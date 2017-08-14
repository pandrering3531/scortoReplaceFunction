using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using STPC.DynamicForms.Infraestructure.Logging;

namespace STPC.DynamicForms.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("robots.txt");

            routes.MapPageRoute("Error", "Shared/globalError/{handler}", "~/Views/Shared/Error.aspx", true, null, new RouteValueDictionary { { "outgoing", new MyCustomConstaint() } });
            routes.MapRoute(
                "FormResponse", //Route Name
                "{shortpath}", //URL with parameters
                new { controller = "Response", action = "Respond" } //Paremeter defaults       
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Account", action = "LogOn", id = UrlParameter.Optional } // Parameter defaults
            );

           
     
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

				MvcHandler.DisableMvcResponseHeader = true;
            //ModelMetadataProviders.Current = new DataAnnotations4ModelMetadataProvider();
            //DataAnnotations4ModelValidatorProvider.RegisterProvider(); 
        }

		  void Application_Error(object sender, EventArgs e)
		  {

              Exception exc = Server.GetLastError();

              ILogging eventWriter = LoggingFactory.GetInstance();
              eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}, InnerException: {2}, Source: {3}", exc.Message, exc.StackTrace, exc.TargetSite.Name, exc.Source));
              Response.RedirectPermanent("~/Shared/globalError/Application_Error%20-%20Global", true);
              
		  }
    }

    public class MyCustomConstaint : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            return routeDirection == RouteDirection.IncomingRequest;
        }
    }
}