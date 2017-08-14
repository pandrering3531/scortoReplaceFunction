using System;
using System.Web.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using STPC.DynamicForms.Web.RT.Services.Entities;
using System.Configuration;
using STPC.DynamicForms.Web.Common;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Common.Messages;
using STPC.DynamicForms.Web.RT.Helpers;

namespace STPC.DynamicForms.Web.RT.Controllers
{
    
    
	public class PerformanceIndicatorController : Controller
	{

		CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
		Services.Entities.STPC_FormsFormEntities _stpcForms = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

		//
		// GET: /PerformanceIndicator/

		public PerformanceIndicatorController()
		{
			_stpcForms.IgnoreResourceNotFoundException = true;
		}

		[Authorize]
		public ActionResult List()
		{
			try
			{


				LoadData();
				List<PerformanceIndicator> performIndicatorList = _stpcForms.PerformanceIndicators.Expand("Role").ToList();

				ValidateSingleTenant();

				int? aplicationNameIdUser;

				int IsSingleTenant = 0;

				if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
				{
					IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

				}
				if (!Roles.GetRolesForUser(this.User.Identity.Name).Contains("Administrador") && IsSingleTenant == 1)
				{

					var theuser = provider.GetUser(this.User.Identity.Name);
					aplicationNameIdUser = theuser.AplicationNameId;
					return View("Index", performIndicatorList.Where(e => e.AplicationNameId == aplicationNameIdUser).ToList());
				}
				else
					return View("Index", performIndicatorList.ToList());


			}
			catch (Exception ex)
			{
				return DefaultActionErrorHandling(ex, "List");
			}
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

			ViewBag.listAplication = AplicationNameManager.LoadAplicationName(_stpcForms);
		}


		[HttpPost]
		public ActionResult getListByAplicationName(int aplicationNameId)
		{
			List<PerformanceIndicator> thisPerformanceIndicator = null;

			if (aplicationNameId > 0)
				thisPerformanceIndicator = _stpcForms.PerformanceIndicators.Expand("Hierarchy").Where(e => e.AplicationNameId == aplicationNameId).ToList();
			else
				thisPerformanceIndicator = _stpcForms.PerformanceIndicators.Expand("Hierarchy").ToList();

			ValidateSingleTenant();
			return PartialView("_Index", thisPerformanceIndicator);
		}

		[HttpPost]
		public ActionResult GetIndex(PerformanceIndicator model, FormCollection par)
		{
			List<PerformanceIndicator> performIndicator = _stpcForms.PerformanceIndicators.Expand("Role").ToList();

			ValidateSingleTenant();
			int? aplicationNameIdUser;

			int IsSingleTenant = 0;

			if (par.AllKeys.Contains("ddlAplicationName") == true)
			{
				if (par["ddlAplicationName"].ToString() == string.Empty)
					return PartialView("_Index", performIndicator.ToList());
				else
					return PartialView("_Index", performIndicator.Where(e => e.AplicationNameId == Convert.ToInt32(par["ddlAplicationName"].ToString())).ToList());


			}
			else
			{
			

				if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
				{
					IsSingleTenant = Convert.ToInt32(ConfigurationManager.AppSettings["IsSingleTenant"].ToString());

				}

				if (IsSingleTenant == 1)
				{
					var theuser = provider.GetUser(this.User.Identity.Name);
					aplicationNameIdUser = theuser.AplicationNameId;
					return PartialView("_Index", performIndicator.Where(e => e.AplicationNameId == aplicationNameIdUser).ToList());
				}
				else
					return PartialView("_Index", performIndicator.ToList());

			}
		}


		//
		// GET: /PerformanceIndicator/Create

		public ActionResult Create()
		{
			try
			{


				LoadData();
				ValidateSingleTenant();
				ViewBag.roles = _stpcForms.Roles.ToList();
				PerformanceIndicator perIndicator = new PerformanceIndicator();
				perIndicator.LastModifiedBy = Guid.Parse(Membership.GetUser().ProviderUserKey.ToString());
				ViewBag.listAplication = AplicationNameManager.LoadAplicationName(_stpcForms);
				return PartialView(perIndicator);
			}

			catch (Exception ex)
			{
				return DefaultActionErrorHandling(ex, "Create");
			}
		}

		//
		// POST: /PerformanceIndicator/Create

		[HttpPost]
		[Authorize]
		public ActionResult Create(PerformanceIndicator model, FormCollection collection)
		{
			try
			{

				int? aplicationNameIdUser;

				if (collection.AllKeys.Contains("AplicationNameId") == true)
				{
					if (collection["AplicationNameId"].ToString() == string.Empty)
						aplicationNameIdUser = null;
					else
						aplicationNameIdUser = Convert.ToInt32(collection["AplicationNameId"].ToString());
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

				//_stpcForms.AddToPerformanceIndicators(model);
				model.AplicationNameId = aplicationNameIdUser;

				Role rol = _stpcForms.Roles.Where(e => e.Rolename == model.Role.Rolename).FirstOrDefault();
				_stpcForms.AddRelatedObject(rol, "PerformanceIndicator", model);
				model.Modified = DateTime.Now;
				if (collection["ApplyBy"] != null) selectAssignment(ref model, collection["ApplyBy"]);

				_stpcForms.SaveChanges();
				return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return DefaultActionErrorHandling(ex, "Create");
			}
		}

		private void selectAssignment(ref PerformanceIndicator model, string selectedAssignment)
		{
			if (selectedAssignment == "R")
			{
				model.Hierarchy = null;
				//model.user = null;
			};
			if (selectedAssignment == "U")
			{
				model.Hierarchy = null;
				model.Role = null;

			}
			if (selectedAssignment == "H")
			{
				model.Role = null;
				//model.user = null;      
			}
		}

		//
		// GET: /PerformanceIndicator/Edit/5

		public ActionResult Edit(int id)
		{
			PerformanceIndicator _performanceIndicator = _stpcForms.PerformanceIndicators.Expand("Role").Expand("Hierarchy").Where(e => e.Id == id).FirstOrDefault();
			LoadData();
			ValidateSingleTenant();
			////TODO: Optimize this
			//foreach (Role item in ViewBag.roles)
			//{
			//	PerformanceIndicator TempPerformanceIndicator = _stpcForms.PerformanceIndicators.Expand("Role").Where(e => e.Id == id && e.Role.Rolename == item.Rolename).FirstOrDefault();

			//	if (TempPerformanceIndicator != null)
			//	{
			//		_performanceIndicator.Role = item;
			//		break;
			//	}
			//}

			//foreach (Hierarchy item in ViewBag.Hierarchy)
			//{
			//	PerformanceIndicator TempPerformanceIndicator = _stpcForms.PerformanceIndicators.Where(e => e.Id == id && e.Hierarchy.Id == item.Id).FirstOrDefault();

			//	if (TempPerformanceIndicator != null)
			//	{
			//		_performanceIndicator.Hierarchy = item;
			//		break;
			//	}
			//}


			//TODO:allocation by user
			ViewBag.listAplication = AplicationNameManager.LoadAplicationName(_stpcForms);
			return PartialView(_performanceIndicator);
		}

		//
		// POST: /PerformanceIndicator/Edit/5

		[HttpPost]
		public ActionResult Edit(PerformanceIndicator model, FormCollection collection)
		{
			PerformanceIndicator _performanceIndicator = _stpcForms.PerformanceIndicators.Expand("AplicationName").Where(e => e.Id == model.Id).FirstOrDefault();

			int? aplicationNameIdUser;

			if (collection.AllKeys.Contains("AplicationNameId") == true)
			{
				if (collection["AplicationNameId"].ToString() == string.Empty)
					aplicationNameIdUser = null;
				else
					aplicationNameIdUser = Convert.ToInt32(collection["AplicationNameId"].ToString());
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

			if (collection["ApplyBy"] != null) selectAssignment(ref model, collection["ApplyBy"]);

			_performanceIndicator.AplicationNameId = aplicationNameIdUser;
			_performanceIndicator.IndicatorType = model.IndicatorType;
			_performanceIndicator.Modified = DateTime.Now;
			_performanceIndicator.Source = model.Source;
			_performanceIndicator.ViolationMaxvalue = model.ViolationMaxvalue;
			_performanceIndicator.ViolationMinvalue = model.ViolationMinvalue;
			_performanceIndicator.WarningMaxValue = model.WarningMaxValue;			
			_performanceIndicator.WarningMinValue = model.WarningMinValue;
			_performanceIndicator.Hierarchy = model.Hierarchy;
			_performanceIndicator.Role = model.Role;
			_performanceIndicator.Enabled = model.Enabled;
			_performanceIndicator.LastModifiedBy = Guid.Parse(Membership.GetUser().ProviderUserKey.ToString());
			_stpcForms.UpdateObject(_performanceIndicator);

			_stpcForms.SaveChanges(System.Data.Services.Client.SaveChangesOptions.Batch);

			return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
		}


		[HttpPost]
		public ActionResult Delete(int id)
		{
			try
			{
				PerformanceIndicator _performanceIndicator = _stpcForms.PerformanceIndicators.Where(e => e.Id == id).FirstOrDefault();

				if (_performanceIndicator != null)
				{
					_stpcForms.DeleteObject(_performanceIndicator);
					_stpcForms.SaveChanges();
				}
				return Json(new { Success = true });
			}
			catch
			{
				return Json(new { Success = true });
			}
		}


		private void LoadData()
		{
			ViewBag.roles = _stpcForms.Roles.ToList();
			ViewBag.Users = provider.GetAllUsersIndicatora();
			ViewBag.Hierarchy = _stpcForms.Hierarchies.ToList();
		}

		private void DefaultErrorHandling(Exception ex, string triggerAction)
		{
			bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
			Guid correlationID = Guid.NewGuid();

			ILogging eventWriter = LoggingFactory.GetInstance();
			string errorMessage = string.Format(CustomMessages.E0007, "PerformanceIndicatorController", triggerAction, correlationID, ex.Message);
			System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
			eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

		}

		private ActionResult DefaultActionErrorHandling(Exception ex, string triggerAction)
		{
			bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
			Guid correlationID = Guid.NewGuid();

			ILogging eventWriter = LoggingFactory.GetInstance();
			string errorMessage = string.Format(CustomMessages.E0007, "PerformanceIndicatorController", triggerAction, correlationID, ex.Message);
			System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
			eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
			if (ShowErrorDetail)
			{
				return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(errorMessage), "PerformanceIndicatorController", triggerAction));
			}
			else
			{
				return PartialView("_ErrorGeneral", string.Format(CustomMessages.E0001, correlationID.ToString()));
			}
		}
	}
}
