using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using System.Security.Principal;
using System.Net;
using System.Configuration;
using System.Web.WebPages.Html;
using STPC.DynamicForms.Web.Common.Messages;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Common;
using System.Web.Security;

namespace STPC.DynamicForms.Web.RT.Reports
{
	public partial class Report : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (this.User.Identity.IsAuthenticated)
			{
				CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
				var user = provider.GetUser(this.User.Identity.Name);

				//if (Roles.IsUserInRole("Administrador") ||
				//	  (Roles.IsUserInRole("Co-Administrador") && !Roles.GetRolesForUser(user.Username).Contains("Administrador")))

				if (!Page.IsPostBack) LoadReport();
			}
		}

		  private void LoadReport()
		  {
				try
				{

					//if (Request.QueryString["user"] != null && Request.QueryString["param"] != null)

					 if (Request.QueryString["param"] != null)
					 {

						  //string user = Request.QueryString["user"];
						  string param = Request.QueryString["param"]; 
						 

						  this.ReportViewerCtrl.ServerReport.ReportServerCredentials = new MyReportServerCredentials();
						  this.ReportViewerCtrl.ServerReport.ReportServerUrl = new Uri(System.Configuration.ConfigurationManager.AppSettings["SsrsServerPath"]);
						  this.ReportViewerCtrl.ServerReport.ReportPath =  param;

						  ReportParameterInfoCollection reportCollection =  this.ReportViewerCtrl.ServerReport.GetParameters();

						  foreach (ReportParameterInfo item in reportCollection)
						  {
								if (item.Name == "UserName")
								{
									ReportParameter userParam = new ReportParameter("UserName", this.User.Identity.Name);
									 this.ReportViewerCtrl.ServerReport.SetParameters(userParam);
								}
								if (item.Name == "Idform")
								{
									ReportParameter userParam = new ReportParameter("Idform", Request.QueryString["requestId"]);
									 this.ReportViewerCtrl.ServerReport.SetParameters(userParam);
								}
						  }
						  
					 }
				}

				catch (Exception ex)
				{
				bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
				Guid correlationID = Guid.NewGuid();

				ILogging eventWriter = LoggingFactory.GetInstance();
				string errorMessage = string.Format(CustomMessages.E0007, "Report", "LoadReport", correlationID, ex.Message);
				System.Diagnostics.Debug.WriteLine("Excepción: " + errorMessage);
				if (ShowErrorDetail)
					eventWriter.WriteLog(string.Format("Excepción: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
				}
		  }
	}

	[Serializable]
	public sealed class MyReportServerCredentials :
		 IReportServerCredentials
	{
		public WindowsIdentity ImpersonationUser
		{
			get
			{
				// Use the default Windows user.  Credentials will be
				// provided by the NetworkCredentials property.
				return null;
			}
		}

		public ICredentials NetworkCredentials
		{
			get
			{
				// Read the user information from the Web.config file.  
				// By reading the information on demand instead of 
				// storing it, the credentials will not be stored in 
				// session, reducing the vulnerable surface area to the
				// Web.config file, which can be secured with an ACL.

				string userName =
					 ConfigurationManager.AppSettings
						  ["SsrsUser"];

				if (string.IsNullOrEmpty(userName))
					throw new Exception(
						 "Missing user name from web.config file");

				string password =
					 ConfigurationManager.AppSettings
						  ["SsrsPwd"];

				if (string.IsNullOrEmpty(password))
					throw new Exception(
						 "Missing password from web.config file");

				string domain =
					 ConfigurationManager.AppSettings
						  ["SsrsUserDomain"];

				if (string.IsNullOrEmpty(domain))
					throw new Exception(
						 "Missing domain from web.config file");

				return new System.Net.NetworkCredential(userName, password, domain);

			}
		}

		public bool GetFormsCredentials(out Cookie authCookie,
						out string userName, out string password,
						out string authority)
		{
			authCookie = null;
			userName = null;
			password = null;
			authority = null;

			// Not using form credentials
			return false;
		}
	}
}