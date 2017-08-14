using STPC.DynamicForms.DecisionEngine;
using STPC.DynamicForms.Web.Helpers;
using STPC.DynamicForms.Web.Models;
using STPC.DynamicForms.Web.Services.Entities;
using STPC.DynamicForms.Web.Services.ScriptGenerator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using STPC.DynamicForms.Web.Common;

namespace STPC.DynamicForms.Web.Controllers
{
	public class ControlEventsController : Controller
	{
		Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
		Services.Entities.STPC_FormsFormEntities _stpcFormsPage = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

		private IDecisionEngine _decisionEngine;
		CustomMembershipProvider UsersProvider;
		ScriptGeneratorServiceClient _ScriptGenerator;

		public ControlEventsController()
		{
			this._ScriptGenerator = new ScriptGeneratorServiceClient();
			this.UsersProvider = (CustomMembershipProvider)Membership.Provider;
			this._stpcForms = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

			string user = System.Configuration.ConfigurationManager.AppSettings["UserWs"];
			string pws = System.Configuration.ConfigurationManager.AppSettings["PwsWs"];
			string pollInterval = System.Configuration.ConfigurationManager.AppSettings["pollInterval"];
			string timeOut = System.Configuration.ConfigurationManager.AppSettings["timeOut"];
			STPC.DynamicForms.DecisionEngine.DecisionEngine iEngine = new STPC.DynamicForms.DecisionEngine.DecisionEngine(user, pws, Convert.ToInt32(pollInterval), Convert.ToInt32(timeOut));
			this._decisionEngine = iEngine;
		}

		public ControlEventsController(IDecisionEngine iEngine)
		{
			this._ScriptGenerator = new ScriptGeneratorServiceClient();
			this.UsersProvider = (CustomMembershipProvider)Membership.Provider;
			this._stpcForms = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
			this._decisionEngine = iEngine;
		}


		private List<PageEvent> LoadIeventsData(Guid id)
		{
			var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == id)).FirstOrDefault();
			ViewBag.Data_FormPageName = thisPage.Name;
			ViewBag.Data_FormPageId = thisPage.Uid;
			ViewBag.Data_FormId = thisPage.Form.Uid;

			#region lista de eventos asociadas

			List<PageEvent> _PageEventResult = new List<PageEvent>();


			List<PageEvent> _PageEvent = _stpcFormsPage.PageEvent.Expand("FormPage").Where(e => e.FormPage.Uid == id).ToList();


			ViewBag.StrategyList = _PageEvent;

			//get ListenerField associated
			foreach (PageEvent item in _PageEvent)
			{


				List<SelectListItem> categoryOptionsList = GetOptionsListByCategory(item.PageFieldUid);

				SelectListItem catOption = categoryOptionsList.FirstOrDefault(o => o.Value == item.FieldValue);
				if (catOption != null)
				{
					ViewData["selectCategoryOptions" + item.Uid] = catOption.Text;
				}
				else
				{
					ViewData["selectCategoryOptions" + item.Uid] = item.FieldValue;
				}

				try
				{
					Panel panel = _stpcFormsPage.Panels.Where(x => x.Uid == item.ListenerFieldId).FirstOrDefault();
					ViewData["listenerControlName" + item.Uid] = " Panel " + panel.Name;
				}

				catch (System.Data.Services.Client.DataServiceQueryException)
				{
					try
					{
						var pagefield = _stpcFormsPage.PageFields.Where(e => e.Uid == item.ListenerFieldId).FirstOrDefault();
						ViewData["listenerControlName" + item.Uid] = "Control " + pagefield.FormFieldName;
					}
					catch (System.Data.Services.Client.DataServiceQueryException)
					{
						//TODO: for this case to do nothing?
					}
				}
				catch (Exception ex)
				{
					throw ex;
				}

			}

			#endregion lista de estrategias asociadas
			return _PageEvent;

		}



		public ActionResult Index(Guid id)
		{

			List<PageEvent> _PageEvent = LoadIeventsData(id);


			return View(_PageEvent);
		}



		[HttpPost]
		public ActionResult GetIndex(PageEvent model, FormCollection par)
		{
			List<PageEvent> _PageEvent = LoadIeventsData(Guid.Parse(par["Data_FormPageId"]));

			return PartialView("_Index", _PageEvent);
		}



		public IEnumerable<SelectListItem> ListaEventos()
		{
			return new List<SelectListItem>
                       {
                           new SelectListItem {Value = "Hide", Text = "Ocultar"},
                           new SelectListItem {Value ="Disabled", Text = "Desactivar"} ,
                           new SelectListItem {Value ="Cascade", Text = "Cascada"}                                                                               
                       };
		}
		public ActionResult Create(Guid idPage)
		{
			LoadData(idPage);


			return PartialView();
		}

		private void LoadData(Guid idPage)
		{
			var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == idPage)).FirstOrDefault();
			ViewBag.Panels = thisPage.Panels.ToList().OrderBy(e => e.SortOrder);
			ViewBag.Data_FormPageId = thisPage.Uid;
			ViewBag.Data_FormId = thisPage.Form.Uid;
			ViewBag.FormPages = _stpcForms.FormPages.ToList();
			ViewBag.Eventos = ListaEventos();

		}

		[HttpPost]
		public ActionResult Create(PageEvent model, FormCollection par)
		{
			try
			{
				string EnabledNameControlName = string.Empty;
				ViewBag.Data_FormPageId = par[0];
				var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == Guid.Parse(par[0]))).FirstOrDefault();

				PageField pageFieldEnabled = null;
				var optionChecked = par["SelectedPanel"].Split(',');

				if (optionChecked.Length == 1)
				{
					pageFieldEnabled = _stpcForms.PageFields.Where(e => e.Uid == model.ListenerFieldId).FirstOrDefault();

					if (pageFieldEnabled != null)
						model.ListenerField = pageFieldEnabled.FormFieldName;
				}
				else
				{
					model.ListenerFieldId = Guid.Parse(par["ddlPanelsResult"]);
					//model.ListenerField = par["txtPanelText"];
				}

				if (par["Evento"] != string.Empty)
					model.EventType = par["Evento"];


				pageFieldEnabled = _stpcForms.PageFields.Where(e => e.Uid == model.PageFieldUid).FirstOrDefault();

				if (pageFieldEnabled != null)
					EnabledNameControlName = pageFieldEnabled.FormFieldName;

				string FIeldTriggerControlName = _stpcForms.PageFields.Where(e => e.Uid == model.PageFieldUid).FirstOrDefault().FormFieldName;


				model.SourceField = FIeldTriggerControlName;

				if (par["Opciones"] != null)
					model.FieldValue = par["Opciones"];
				else
					model.FieldValue = par["FieldValue"];

				model.FormPageUid = Guid.Parse(par[0]);//Guid.Parse(par["ddlPage"]);

				_stpcForms.AddObject("PageEvent", model);
				// guardar cambios
				_stpcForms.SaveChanges();

				return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{

				return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
			}
		}

		[HttpPost]
		public ActionResult Edit(PageEvent model, FormCollection par)
		{
			PageEvent _PageEvent = _stpcForms.PageEvent.Where(e => e.Uid == model.Uid).FirstOrDefault();

			try
			{
				string EnabledNameControlName = string.Empty;
				ViewBag.Data_FormPageId = par[0];
				var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == Guid.Parse(par[0]))).FirstOrDefault();

				PageField pageFieldEnabled = null;
				var optionChecked = par["SelectedPanel"].Split(',');

				if (optionChecked.Length == 1)
				{
					pageFieldEnabled = _stpcForms.PageFields.Where(e => e.Uid == model.ListenerFieldId).FirstOrDefault();

					if (pageFieldEnabled != null)
						model.ListenerField = pageFieldEnabled.FormFieldName;
				}
				else
				{
					model.ListenerFieldId = Guid.Parse(par["ddlPanelsResult"]);
					model.ListenerField = null;
					//model.ListenerField = par["txtPanelText"];
				}



				pageFieldEnabled = _stpcForms.PageFields.Where(e => e.Uid == model.PageFieldUid).FirstOrDefault();

				if (pageFieldEnabled != null)
					EnabledNameControlName = pageFieldEnabled.FormFieldName;

				string FIeldTriggerControlName = _stpcForms.PageFields.Where(e => e.Uid == model.PageFieldUid).FirstOrDefault().FormFieldName;


				model.SourceField = FIeldTriggerControlName;


				model.FormPageUid = Guid.Parse(par[0]);//Guid.Parse(par["ddlPage"]);


				_PageEvent.FieldValue = model.FieldValue;
				_PageEvent.EventType = model.EventType;
				_PageEvent.FormPage = model.FormPage;
				_PageEvent.FormPageUid = model.FormPageUid;
				_PageEvent.ListenerField = model.ListenerField;
				_PageEvent.ListenerFieldId = model.ListenerFieldId;
				_PageEvent.PageFieldUid = model.PageFieldUid;
				_PageEvent.SourceField = model.SourceField;

				_stpcForms.UpdateObject(_PageEvent);
				_stpcForms.SaveChanges();

				return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{

				return Json(JsonResponseFactory.ErrorResponse(ex.Message), JsonRequestBehavior.AllowGet);
			}


			return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
		}

		public ActionResult EditIEvent(Guid id, Guid idPage)
		{

			PageEvent _PageEvent = _stpcForms.PageEvent.Where(e => e.Uid == id).FirstOrDefault();
			LoadData(idPage);

			// 1) GET the trigger data
			var triggerPagefield = _stpcFormsPage.PageFields.Where(e => e.Uid == _PageEvent.PageFieldUid).FirstOrDefault();
			ViewBag.selectedTriggerPanelValue = triggerPagefield.PanelUid;
			ViewBag.triggerPageFieldsList = _stpcForms.PageFields.Where(e => e.PanelUid == triggerPagefield.PanelUid).ToList();

			List<SelectListItem> categoryOptionsList = GetOptionsListByCategory(_PageEvent.PageFieldUid);
			ViewBag.categoryOptionsList = categoryOptionsList;


			if (categoryOptionsList.Count(o => o.Value == _PageEvent.FieldValue) > 0)
			{
				ViewBag.selectCategoryOptionsList = _PageEvent.FieldValue;
				_PageEvent.FieldValue = "";
			}
			else
			{
				ViewBag.selectCategoryOptionsList = "";
			}



			//2) GET the AsignedControlData



			//if the listener if a panel
			try
			{
				Panel panel = _stpcFormsPage.Panels.Expand("Page").Where(x => x.Uid == _PageEvent.ListenerFieldId).FirstOrDefault();
				ViewBag.selectedAssignedPanelValue = panel.Uid;

				ViewBag.isListenerPanel = true;
				ViewBag.assignedPageFieldsList = _stpcForms.PageFields.Where(e => e.PanelUid == panel.Uid).ToList();

				//get de selected page of the control
				var page = _stpcForms.FormPages.Where(p => p.Uid == panel.Page.Uid).FirstOrDefault();
				ViewBag.selectedAssignedPageId = page.Uid;

				//get de panel List
				ViewBag.assignedPanelList = _stpcForms.Panels.Where(e => e.Page.Uid == page.Uid).ToList();
			}

			// else if the listener if a control page
			catch (System.Data.Services.Client.DataServiceQueryException)
			{
				try
				{
					//get the control data
					var pagefield = _stpcFormsPage.PageFields.Where(e => e.Uid == _PageEvent.ListenerFieldId).FirstOrDefault();
					ViewBag.selectedAssignedPanelValue = pagefield.PanelUid;

					ViewBag.isListenerPanel = false;
					ViewBag.assignedPageFieldsList = _stpcForms.PageFields.Where(e => e.PanelUid == pagefield.PanelUid).ToList();


					//get de selected page of the control
					Panel panel = _stpcFormsPage.Panels.Expand("Page").Where(x => x.Uid == pagefield.PanelUid).FirstOrDefault();
					var page = _stpcForms.FormPages.Where(p => p.Uid == panel.Page.Uid).FirstOrDefault();
					ViewBag.selectedAssignedPageId = page.Uid;

					//get de panel List
					ViewBag.assignedPanelList = _stpcForms.Panels.Where(e => e.Page.Uid == page.Uid).ToList();

				}
				catch (System.Data.Services.Client.DataServiceQueryException)
				{
					ViewBag.isListenerPanel = false;
					ViewBag.assignedPageFieldsList = new List<PageField>();
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return PartialView(_PageEvent);
		}

		[HttpPost]
		public ActionResult Delete(Guid id)
		{

			try
			{
				PageEvent _PageEvent = _stpcForms.PageEvent.Where(e => e.Uid == id).FirstOrDefault();

				if (_PageEvent != null)
				{
					_stpcForms.DeleteObject(_PageEvent);
					// guardar cambios
					_stpcForms.SaveChanges();
				}
				return Json(new { Success = true });
			}
			catch
			{
				return Json(new { Success = true });
			}
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public JsonResult GetFieldByPanel(Guid id)
		{
			List<PageField> modelList = _stpcForms.PageFields.Where(e => e.PanelUid == id).ToList();

			var modelData = modelList.Select(m => new SelectListItem()
			{
				Text = m.FormFieldName,
				Value = m.Uid.ToString(),

			});

			return Json(modelData, JsonRequestBehavior.AllowGet);
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public JsonResult GetPanelsByPage(Guid id)
		{
			List<Panel> modelList = _stpcForms.Panels.Where(e => e.Page.Uid == id).ToList();

			var modelData = modelList.Select(m => new SelectListItem()
			{
				Text = m.Description,
				Value = m.Uid.ToString(),

			});

			return Json(modelData, JsonRequestBehavior.AllowGet);
		}


		private List<SelectListItem> GetOptionsListByCategory(Guid id)
		{

			PageField modelList;

			try
			{
				modelList = _stpcForms.PageFields.Where(e => e.Uid == id).FirstOrDefault();
			}

			catch (System.Data.Services.Client.DataServiceQueryException)
			{
				return new List<SelectListItem>();
			}

			if (modelList.OptionsCategoryName != null)
			{
				var TheOptions = _stpcForms.Options.Where(cn => cn.Category_Uid == Convert.ToInt32(modelList.OptionsCategoryName)).ToList();
				List<SelectListItem> modelData = TheOptions.Select(m => new SelectListItem()
		  {
			  Text = m.Value,
			  Value = m.Uid.ToString(),

		  }).ToList();
				return modelData;
			}
			return new List<SelectListItem>();
		}


		[AcceptVerbs(HttpVerbs.Get)]
		public JsonResult GetOptionsByCategory(Guid id)
		{
			var OptionsList = GetOptionsListByCategory(id);

			if (OptionsList.Count() > 0)
			{
				return Json(OptionsList, JsonRequestBehavior.AllowGet);
			}

			else
			{
				return Json("Empty", JsonRequestBehavior.AllowGet);
				//return Json(new JsonResult() { Data = new { Text = "Fernando", Value = "Rodriguez" } }, JsonRequestBehavior.AllowGet);
			}
		}

	}
}
