using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using STPC.DynamicForms.Infraestructure.Logging;

namespace STPC.DynamicForms.Web.Views.Shared
{
    public partial class Error : System.Web.Mvc.ViewPage<System.Web.Mvc.HandleErrorInfo>
	{
		protected void Page_Load(object sender, EventArgs e)
		{

			// Create safe error messages.
			string generalErrorMsg = "Se ha presentado un problema. Intente nuevamente. " +
				 "Si persiste el error, contacte a soporte.";
			string httpErrorMsg = "Un error HTTP se ha presentado. Recurso no encontrado. Intente nuevamente.";
			string unhandledErrorMsg = "El error no ha sido manejado por la aplicación.";

			// Display safe error message.
			FriendlyErrorMsg.Text = generalErrorMsg;

			// Determine where error was handled.
			string errorHandler = Request.QueryString["handler"];
			if (errorHandler == null)
			{
				errorHandler = "Error Page";
			}

			// Get the last error from the server.
			Exception ex = Server.GetLastError();

			// Get the error number passed as a querystring value.
			string errorMsg = Request.QueryString["msg"];
			if (errorMsg == "404")
			{
				ex = new HttpException(404, httpErrorMsg, ex);
				FriendlyErrorMsg.Text = ex.Message;
			}

			// If the exception no longer exists, create a generic exception.
			if (ex == null)
			{
				ex = new Exception(unhandledErrorMsg);
			}

			// Show error details to only you (developer). LOCAL ACCESS ONLY.
			if (Request.IsLocal)
			{
				// Detailed Error Message.
				ErrorDetailedMsg.Text = ex.Message;

				// Show where the error was handled.
				ErrorHandler.Text = errorHandler;

				// Show local access details.
				DetailedErrorPanel.Visible = true;

				if (ex.InnerException != null)
				{
					InnerMessage.Text = ex.GetType().ToString() + "<br/>" +
						 ex.InnerException.Message;
					InnerTrace.Text = ex.InnerException.StackTrace;
				}
				else
				{
					InnerMessage.Text = ex.GetType().ToString();
					if (ex.StackTrace != null)
					{
						InnerTrace.Text = ex.StackTrace.ToString().TrimStart();
					}
				}
			}

			// Log the exception.
			ILogging eventWriter = LoggingFactory.GetInstance();
			string errorMessage = string.Format("Exception: {0}, Stack Trace: {1}, Error Handler: {2}, RawUrl: {3}", ex.Message, ex.StackTrace, errorHandler, Request.RawUrl);
			eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

			// Clear the error from the server.
			Server.ClearError();
		}
	}
}