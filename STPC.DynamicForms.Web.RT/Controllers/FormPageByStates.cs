using STPC.DynamicForms.DecisionEngine;
using STPC.DynamicForms.Web.RT.Helpers;
using STPC.DynamicForms.Web.RT.Models;
using STPC.DynamicForms.Web.RT.Services.Entities;
using STPC.DynamicForms.Web.RT.Services.ScriptGenerator;
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
using System.Net;

namespace STPC.DynamicForms.Web.RT.Controllers
{
    
    
	public class FormPageByStatesController : Controller
	{
		Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
		Services.Entities.STPC_FormsFormEntities _stpcFormsPage = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

		private IDecisionEngine _decisionEngine;
		CustomMembershipProvider UsersProvider;
		ScriptGeneratorServiceClient _ScriptGenerator;

		public FormPageByStatesController()
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
			_stpcForms.IgnoreResourceNotFoundException = true;
			_stpcFormsPage.IgnoreResourceNotFoundException = true;
		}

		public FormPageByStatesController(IDecisionEngine iEngine)
		{
			this._ScriptGenerator = new ScriptGeneratorServiceClient();
			this.UsersProvider = (CustomMembershipProvider)Membership.Provider;
			this._stpcForms = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
			this._decisionEngine = iEngine;
		}


		//
		// GET: /Strategy/

		public ActionResult Index(Guid id)
		{
			var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == id)).FirstOrDefault();
			ViewBag.Data_FormPageName = thisPage.Name;
			ViewBag.Data_FormPageId = thisPage.Uid;
			ViewBag.Data_FormId = thisPage.Form.Uid;

			ViewBag.Data_FormPageId = id;
			List<FormPageByStates> listFormPageByStates = _stpcForms.FormPageByStates.Expand("FormPage").Expand("FormStates").Where(e => e.FormPageUid == id).ToList();


			return View(listFormPageByStates);
		}

		[HttpPost]
		[ValidateAntiForgeryTokenAttribute]
		public ActionResult List(Guid id)
		{
			var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == id)).FirstOrDefault();
			ViewBag.Data_FormPageName = thisPage.Name;
			ViewBag.Data_FormPageId = thisPage.Uid;
			ViewBag.Data_FormId = thisPage.Form.Uid;

			ViewBag.Data_FormPageId = id;
			List<FormPageByStates> listFormPageByStates = _stpcForms.FormPageByStates.Expand("FormPage").Expand("FormStates").Where(e => e.FormPageUid == id).ToList();


			return PartialView("Index",listFormPageByStates);
		}

		[HttpPost]
		public ActionResult GetIndex(FormPageByStates model, FormCollection par)
		{
			var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == Guid.Parse(par["Data_FormPageId"]))).FirstOrDefault();
			ViewBag.Data_FormPageName = thisPage.Name;
			ViewBag.Data_FormPageId = thisPage.Uid;
			ViewBag.Data_FormId = thisPage.Form.Uid;

			List<FormPageByStates> listFormPageByStates = _stpcForms.FormPageByStates.Expand("FormPage").Expand("FormStates").Where(e => e.FormPageUid == thisPage.Uid).ToList();

			#region lista de estados asociadas

			
			

			#endregion lista de estrategias asociadas

			return PartialView("_Index", listFormPageByStates);
		}


		//
		// GET: /Strategy/Details/5
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
			var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == idPage)).FirstOrDefault();
			ViewBag.Panels = thisPage.Panels.ToList().OrderBy(e => e.SortOrder);
			ViewBag.Data_FormPageId = thisPage.Uid;
			ViewBag.Data_FormId = thisPage.Form.Uid;

			List<FormStates> _FormStates = _stpcFormsPage.FormStates.ToList();
			ViewBag.FormStates = _FormStates;


			return PartialView();
		}

		[HttpPost]
		public ActionResult Create(FormPageByStates model, FormCollection par)
		{
			try
			{
				Guid _state = Guid.Parse(par["State"]);
				Guid _formPage =Guid.Parse(par["Data_FormPageId"]);

				FormPageByStates _formPageByStates = _stpcForms.FormPageByStates.Where(e => e.FormPageUid == _formPage && e.FormStatesUid == _state).FirstOrDefault();

				if (_formPageByStates != null)
				{

					throw new Exception("Ya se a asignado este estado a la página");
					
					//return Json(JsonResponseFactory.ErrorResponse("Ya se a asignado este estado a la página"), JsonRequestBehavior.AllowGet);

				}
				model.FormStatesUid = _state;
				model.FormPageUid = _formPage;

				_stpcForms.AddObject("FormPageByStates", model);
				// guardar cambios
				_stpcForms.SaveChanges();

				return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				Response.StatusCode = (int)HttpStatusCode.Conflict;
				return Json(ex.Message);
				
			}
		}


		public ActionResult Edit(Guid id)
		{
		
			FormPageByStates _FormPageByStates = _stpcForms.FormPageByStates.Where(e => e.Uid == id).FirstOrDefault();
			ViewBag.Data_FormPageId = _FormPageByStates.FormPageUid;

			List<FormStates> _FormStates = _stpcFormsPage.FormStates.ToList();
			ViewBag.FormStates = _FormStates;


			return PartialView(_FormPageByStates);
		}

		[HttpPost]
		public ActionResult Edit(AdCampaign model, FormCollection par)
		{
			FormPageByStates _FormPageByStates = _stpcForms.FormPageByStates.Where(e => e.Uid == model.Uid).FirstOrDefault();
			Guid _state = Guid.Parse(par["State"]);

			_FormPageByStates.FormStatesUid = _state;




			_stpcForms.UpdateObject(_FormPageByStates);
			// guardar cambios
			_stpcForms.SaveChanges();


			return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
		}


		[HttpPost]
		public ActionResult Delete(Guid id)
		{

			try
			{
				FormPageByStates _FormPageByStates = _stpcForms.FormPageByStates.Where(e => e.Uid == id).FirstOrDefault();

				if (_FormPageByStates != null)
				{
					_stpcForms.DeleteObject(_FormPageByStates);
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
		
	}
}
