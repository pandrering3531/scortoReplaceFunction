using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using STPC.DynamicForms.DecisionEngine;
using STPC.DynamicForms.Web.RT.Helpers;
using STPC.DynamicForms.Web.RT.Models;
using System.Configuration;
using STPC.DynamicForms.Web.RT.Services.Request;
using STPC.DynamicForms.Web.Common.Messages;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Common;
using STPC.DynamicForms.Web.RT.Services.ScriptGenerator;
using System.Net;

namespace STPC.DynamicForms.Web.RT.Controllers
{
	[Authorize]
    
    
	public class FormController : BaseController
	{
		CustomMembershipProvider UsersProvider = (CustomMembershipProvider)Membership.Provider;
		CustomRequestProvider requestProvider = new CustomRequestProvider();
		ScriptGeneratorServiceClient _ScriptGenerator;

		Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

		private IDecisionEngine _decisionEngine;

		public FormController(IDecisionEngine decisionEngine)
		{
			this._ScriptGenerator = new ScriptGeneratorServiceClient();
			_decisionEngine = decisionEngine;
			_stpcForms.IgnoreResourceNotFoundException = true;

		}

		public FormController()
		{
			this._ScriptGenerator = new ScriptGeneratorServiceClient();
			string user = System.Configuration.ConfigurationManager.AppSettings["UserWs"];
			string pws = System.Configuration.ConfigurationManager.AppSettings["PwsWs"];
			string pollInterval = System.Configuration.ConfigurationManager.AppSettings["pollInterval"];
			string timeOut = System.Configuration.ConfigurationManager.AppSettings["timeOut"];
			this._decisionEngine = new STPC.DynamicForms.DecisionEngine.DecisionEngine(user, pws, Convert.ToInt32(pollInterval), Convert.ToInt32(timeOut));
		}

		[HttpPost]
		[ValidateAntiForgeryTokenAttribute]
		public ActionResult CreateRequest(Guid id)
		{
			var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == id).FirstOrDefault();

			CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
			var theuser = provider.GetUser(this.User.Identity.Name);

			Request request = new Request()
			{
				AplicationNameId = theuser.AplicationNameId,
				FormId = id,
				CreatedBy = User.Identity.Name,
				Created = DateTime.Now,
				UpdatedBy = User.Identity.Name,
				Updated = DateTime.Now,
				PageFlowId = thisForm.Pages.OrderBy(o => o.DisplayOrder).First().Uid,
				AssignedTo = User.Identity.Name,
			};

			int requestId = 0;
			try
			{
				requestId = requestProvider.CreateRequest(request);


				if (requestId == 0)
				{
					throw new Exception("Error creando la solicitud:");
				}
			}
			catch (Exception Ex)
			{

				return PartialView("_ErrorDetail", new HandleErrorInfo(Ex, "Form Controller", "CreateRequest"));
			}

			return RedirectToAction("RespondNew", new { requestId = requestId, id = id });
		}

		public ActionResult RespondNew(int requestId, Guid id)
		{
			var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == id).FirstOrDefault();
			if (thisForm == null) return View("Error");
			ViewBag.FormName = thisForm.Name;
			ViewData["PageId"] = thisForm.Pages.OrderBy(o => o.DisplayOrder).First().Uid;
			ViewData["RequestId"] = requestId;

			return PartialView(thisForm);
		}

		public ActionResult Respond(int requestId, Guid id)
		{
			var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == id).FirstOrDefault();
			if (thisForm == null) return View("Error");
			ViewBag.FormName = thisForm.Name;
			ViewData["PageId"] = thisForm.Pages.OrderBy(o => o.DisplayOrder).First().Uid;
			ViewData["RequestId"] = requestId;

			return View(thisForm);
		}

		public ActionResult Respond_dt(Guid id)
		{
			try
			{
				//Provisional - Generate stript call
				//string val = GenerateScriptString(id);
				var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == id).FirstOrDefault();
				if (thisForm == null) return View("Error");
				ViewBag.FormName = thisForm.Name;
				return View(thisForm.Pages.OrderBy(o => o.DisplayOrder));
				//return View(thisForm.Pages.OrderBy(o => o.SortOrder));
			}
			catch (Exception ex)
			{

				Guid correlationID = Guid.NewGuid();
				ILogging eventWriter = LoggingFactory.GetInstance();
				string errorMessage = string.Format(CustomMessages.E0007, "FormController", "Respond_dt", correlationID, ex.Message);
				eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
				bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
				// validar si se despliega error detallado al usuario
				if (ShowErrorDetail)
					return PartialView("_ErrorDetail", new HandleErrorInfo(ex, "Form Controller", "Respond_dt"));
				return PartialView("_ErrorGeneral", CustomMessages.E0001 + "\n" + "Código del error: " + correlationID.ToString());
			}
		}

		public ActionResult FormPageRespond(Guid FormId, Guid PageId, int requestId)
		{
			var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == FormId).FirstOrDefault();
			if (thisForm == null) return View("Error");
			ViewBag.FormName = thisForm.Name;
			ViewData["PageId"] = PageId;
			ViewData["RequestId"] = requestId;
			return View("Respond", thisForm);
		}

		public ActionResult FormPageRespondDinamic(Guid FormId, Guid PageId, int requestId)
		{
			var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == FormId).FirstOrDefault();
			if (thisForm == null) return View("Error");
			ViewBag.FormName = thisForm.Name;
			ViewData["PageId"] = PageId;
			ViewData["RequestId"] = requestId;
			return PartialView("Respond", thisForm);
		}

		[HttpPost]
		public ActionResult FormPageRespondView(Guid FormId, int requestId, Guid PageFlowId)
		{
			try
			{
				var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == FormId).FirstOrDefault();
				Services.Entities.Request request = _stpcForms.Request.Where(e => e.RequestId == requestId).FirstOrDefault();

				if (thisForm == null || request == null) return View("Error");
				ViewBag.FormName = thisForm.Name;
				ViewData["PageId"] = PageFlowId;
				ViewData["RequestId"] = requestId;
				return PartialView("Respond", thisForm);
			}
			catch (Exception ex)
			{
				// crear registro de error en el visor de sucesos
				Guid correlationID = Guid.NewGuid();
				ILogging eventWriter = LoggingFactory.GetInstance();
				string errorMessage = string.Format(CustomMessages.E0007, "FormController", "FormPageRespondView", correlationID, ex.Message);
				eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
				bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
				// validar si se despliega error detallado al usuario
				if (ShowErrorDetail)
					return PartialView("_ErrorDetail", new HandleErrorInfo(ex, "Form Controller", "FormPageRespondView"));
				return PartialView("_ErrorGeneral", CustomMessages.E0001 + "\n" + "Código del error: " + correlationID.ToString());
			}

		}
		//[HttpPost]
		//public ActionResult FormPageRespondView(Guid FormId, int requestId)
		//{
		//	return View();
		//}


		public ActionResult FormPagesMenu(Guid id, int requestId)
		{
			var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == id).FirstOrDefault();
			ViewData["RequestId"] = requestId;
			return PartialView("FormPagesMenu", thisForm);
		}

		/// <summary>
		/// Generates the script.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <param name="strFormName">Name of the STR form.</param>
		/// <returns></returns>
		public ActionResult GenerateScript(Guid id, string strFormName)
		{
			string resultScript = this._ScriptGenerator.GenerateScriptString(id);

			ScriptsViewModel _scriptVM = new ScriptsViewModel();
			_scriptVM.Script = resultScript;
			_scriptVM.uid = id;

			ViewBag.FormName = strFormName;
			ViewBag.retv = resultScript;

			ViewBag.FormName = strFormName;
			ViewBag.FormId = id;

			return View(_scriptVM);
		}

		//RG Provisional - left here but needed to move to another class
		public string GenerateScriptString(Guid formId)
		{
			var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == formId).FirstOrDefault();

			foreach (Services.Entities.FormPage fp in thisForm.Pages)
			{
				var formPage = _stpcForms.FormPages.Expand("Panels").Where(fpage => fpage.Uid == fp.Uid).FirstOrDefault();
				foreach (Services.Entities.Panel p in formPage.Panels)
				{
					var fields = _stpcForms.PageFields.Where(xp => xp.Panel.Uid == p.Uid);
					var flist = fields.ToList();
				}
			}
			return "NULL";
		}


		[Authorize(Roles = "Administrador")]
		public ActionResult List()
		{
			var forms = from items in _stpcForms.Forms.Expand("Pages")
							orderby items.Name
							select items;
			return View(forms);
		}

		[Authorize(Roles = "Administrador")]
		[HttpPost]
		[ValidateAntiForgeryTokenAttribute]
		public ActionResult ListPost()
		{
			return GetDataList();
		}

		private ActionResult GetDataList()
		{
			var forms = from items in _stpcForms.Forms.Expand("Pages")
							orderby items.Name
							select items;
			return PartialView("List", forms);
		}

		public ActionResult Edit(Guid id)
		{
			var item = _stpcForms.Forms.Expand("Pages").Where((x => x.Uid == id)).FirstOrDefault();
			//TODO: Refactor código repetido en otro controller
			ViewBag.StrategiesSelect = new List<SelectListItem>();
            ViewBag.StrategiesSelect.Add(new SelectListItem { Text = "Seleccione una estrategia", Value = "" });
			foreach (var strategy in _decisionEngine.GetStrategyList())
				ViewBag.StrategiesSelect.Add(new SelectListItem { Text = strategy.Name, Value = strategy.Id.ToString() });
			ViewBag.FormName = item.Name;
			ViewBag.FormId = item.Uid;
			return View(item.Pages.OrderBy(o => o.DisplayOrder));
			//return View(item.Pages.OrderBy(o => o.SortOrder));
		}




		public ActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Create(string name)
		{
			var userGuid = UsersProvider.GetUser(this.User.Identity.Name).Id;
			var theNewForm = new Services.Entities.Form
			{
				Name = name,
				Timestamp = DateTime.Now,
				UserId = userGuid
			};
			_stpcForms.AddToForms(theNewForm);
			_stpcForms.SaveChanges();
			return GetDataList();
		}

		[HttpPost]
		public ActionResult DeletePost(Guid id)
		{
			if (id == null) return View("Error");

			var theForm = _stpcForms.Forms.Where(i => i.Uid == id).FirstOrDefault();
			if (theForm != null)
			{
				_stpcForms.DeleteObject(theForm);
				_stpcForms.SaveChanges();
				return GetDataList();
			}
			return Json(new { Success = false });
		}


		public ActionResult Delete(Guid id)
		{
			if (id == null) return View("Error");
			var theForm = _stpcForms.Forms.Where(i => i.Uid == id).FirstOrDefault();
			if (theForm != null)
			{
				_stpcForms.DeleteObject(theForm);
				_stpcForms.SaveChanges();
				return GetDataList();
			}
			return View("Error");
		}

		// CUSTOM, reference http://blog.stevensanderson.com/2010/01/28/validating-a-variable-length-list-aspnet-mvc-2-style/
		public class AjaxViewResult : ViewResult
		{
			public string UpdateValidationForFormId { get; set; }

			public AjaxViewResult(string viewName, object model)
			{
				ViewName = viewName;
				ViewData = new ViewDataDictionary { Model = model };
			}

			public override void ExecuteResult(ControllerContext context)
			{
				var result = base.FindView(context);
				var viewContext = new ViewContext(context, result.View, ViewData, TempData, context.HttpContext.Response.Output);

				BeginCapturingValidation(viewContext);
				base.ExecuteResult(context);
				EndCapturingValidation(viewContext);

				result.ViewEngine.ReleaseView(context, result.View);
			}

			private void BeginCapturingValidation(ViewContext viewContext)
			{
				if (string.IsNullOrEmpty(UpdateValidationForFormId))
					return;
				viewContext.ClientValidationEnabled = true;
				viewContext.FormContext = new FormContext { FormId = UpdateValidationForFormId };
			}

			private static void EndCapturingValidation(ViewContext viewContext)
			{
				if (!viewContext.ClientValidationEnabled)
					return;
				viewContext.OutputClientValidation();
				viewContext.Writer.WriteLine("<script type=\"text/javascript\">Sys.Mvc.FormContext._Application_Load()</script>");
			}
			//End CUSTOM
		}

	}
}