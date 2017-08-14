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
using System.IO;

namespace STPC.DynamicForms.Web.Controllers
{
	public class CampaignController : Controller
	{
		Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
		Services.Entities.STPC_FormsFormEntities _stpcFormsPage = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
		public static string nodeString;

		private IDecisionEngine _decisionEngine;
		CustomMembershipProvider UsersProvider;
		ScriptGeneratorServiceClient _ScriptGenerator;

		public CampaignController()
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


		public JsonResult GetchildrenCountAndLevelName(int id)
		{

			string srHierarchies = id + getParentName(id);
			var TheHierarchy = _stpcForms.Hierarchies.Expand("Children").Where(i => i.Id == id).FirstOrDefault();
			nodeString = string.Empty;
			return Json(id, JsonRequestBehavior.AllowGet);

		}

		private string getParentName(int idNode)
		{

			var model = _stpcForms.Hierarchies.Expand("Parent").ToList();

			foreach (var item in model)
			{
				if (item.Id == idNode)
				{
					nodeString += "/" + item.Name;
					if (item.Parent != null)
						getParentName(item.Parent.Id);
				}
			}

			return nodeString;
		}
		public CampaignController(IDecisionEngine iEngine)
		{
			this._ScriptGenerator = new ScriptGeneratorServiceClient();
			this.UsersProvider = (CustomMembershipProvider)Membership.Provider;
			this._stpcForms = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
			this._decisionEngine = iEngine;
		}


		//
		// GET: /Strategy/
		[Authorize]
		public ActionResult List()
		{
			List<AdCampaign> thisCampaign = _stpcForms.AdCampaign.Expand("Hierarchy").ToList();

			return View("Index", thisCampaign);
		}



		[HttpPost]
		public ActionResult GetIndex(AdCampaign model, FormCollection par)
		{

			List<AdCampaign> thisCampaign = _stpcForms.AdCampaign.Expand("Hierarchy").ToList();

			return PartialView("_Index", thisCampaign);
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
		public ActionResult Create()
		{

			AdCampaign _newCampaign = new AdCampaign();
			_newCampaign.BeginDate = DateTime.Now;
			_newCampaign.EndDate = DateTime.Now;

			List<Hierarchy> listHierarchi = _stpcForms.Hierarchies.Expand("Parent").ToList();

			foreach (var item in listHierarchi)
			{
				_newCampaign.HierarchyByCampaign.Add(item);
			}


			return PartialView(_newCampaign);
		}

		[HttpPost]
		public ActionResult Create(AdCampaign model, FormCollection par)
		{

			if (par["Jerarquia"] != string.Empty)
			{
				model.Hierarchy_id = Convert.ToInt32(par["Jerarquia"]);

			}


			//model.BeginDate = Convert.ToDateTime(par["BeginDate"]);
			//model.EndDate = Convert.ToDateTime(par["EndDate"]);

			_stpcForms.AddObject("AdCampaign", model);
			// guardar cambios
			_stpcForms.SaveChanges();

			foreach (string inputTagName in Request.Files)
			{
				HttpPostedFileBase file = Request.Files[inputTagName];

				if (file != null && file.ContentLength > 0)
				{
					// extract only the fielname
					var fileName = Path.GetFileName(file.FileName);
					string CampaignImageFolder = System.Configuration.ConfigurationManager.AppSettings["CampaignImagePhysicaFolder"];

					string[] nameFile = fileName.Split(new char[] { '.' });

					if (nameFile.Length > 1)
					{
						// store the file inside ~/App_Data/uploads folder
						var path = CampaignImageFolder + nameFile[0] + "_" + model.Uid + "_." + nameFile[1];
						model.Image = System.Configuration.ConfigurationManager.AppSettings["CampaignImageFolder"] + nameFile[0] + "_" + model.Uid + "_." + nameFile[1];
						file.SaveAs(path);
						_stpcForms.UpdateObject(model);
						_stpcForms.SaveChanges();
					}
				}
			}
			//return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
			return Json(JsonResponseFactory.SuccessResponse(), "text/html", System.Text.Encoding.UTF8,
							  JsonRequestBehavior.AllowGet);
		}

		public ActionResult Edit(Guid id)
		{

			AdCampaign _EditCampaign = _stpcForms.AdCampaign.Expand("Hierarchy").Where(e => e.Uid == id).FirstOrDefault();

			List<Hierarchy> listHierarchi = _stpcForms.Hierarchies.Expand("Parent").ToList();

			foreach (var item in listHierarchi)
			{
				_EditCampaign.HierarchyByCampaign.Add(item);
			}


			return PartialView(_EditCampaign);
		}

		[HttpPost]
		public ActionResult Edit(AdCampaign model, FormCollection par)
		{
			AdCampaign _AdCampaign = _stpcForms.AdCampaign.Expand("Hierarchy").Where(e => e.Uid == model.Uid).FirstOrDefault();

			if (par["Jerarquia"] != string.Empty)
			{
				model.Hierarchy_id = Convert.ToInt32(par["Jerarquia"]);

			}


			_AdCampaign.ApplyToChilds = model.ApplyToChilds;
			_AdCampaign.Uid = model.Uid;
			_AdCampaign.Text = model.Text;
			_AdCampaign.Image = model.Image;
			_AdCampaign.BeginDate = model.BeginDate;
			_AdCampaign.EndDate = model.EndDate;
			_AdCampaign.Url = model.Url;
			_AdCampaign.Hierarchy_id = model.Hierarchy_id;



			_stpcForms.UpdateObject(_AdCampaign);
			// guardar cambios
			_stpcForms.SaveChanges();

			foreach (string inputTagName in Request.Files)
			{
				HttpPostedFileBase file = Request.Files[inputTagName];

				if (file != null && file.ContentLength > 0)
				{
					// extract only the fielname
					var fileName = Path.GetFileName(file.FileName);
					string CampaignImageFolder = System.Configuration.ConfigurationManager.AppSettings["CampaignImagePhysicaFolder"];

					string[] nameFile = fileName.Split(new char[] { '.' });

					if (nameFile.Length > 1)
					{
						// store the file inside ~/App_Data/uploads folder
						var path = CampaignImageFolder + nameFile[0] + "_" + _AdCampaign.Uid + "_." + nameFile[1];
						_AdCampaign.Image = System.Configuration.ConfigurationManager.AppSettings["CampaignImageFolder"] + nameFile[0] + "_" + _AdCampaign.Uid + "_." + nameFile[1];
						file.SaveAs(path);
						_stpcForms.UpdateObject(_AdCampaign);
						_stpcForms.SaveChanges();
					}
				}
			}

			return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
		}





		[HttpPost]
		public ActionResult Delete(Guid id)
		{

			try
			{
				AdCampaign _AdCampaign = _stpcForms.AdCampaign.Where(e => e.Uid == id).FirstOrDefault();

				if (_AdCampaign != null)
				{
					_stpcForms.DeleteObject(_AdCampaign);
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
		public JsonResult GetOptionsByCategory(Guid id)
		{
			PageField modelList = _stpcForms.PageFields.Where(e => e.Uid == id).FirstOrDefault();
			List<Option> TheOptions = new List<Option>();

			if (modelList.OptionsCategoryName != null)
			{
				TheOptions = _stpcForms.Options.Where(cn => cn.Category_Uid == Convert.ToInt32(modelList.OptionsCategoryName)).ToList();

				var modelData = TheOptions.Select(m => new SelectListItem()
				{
					Text = m.Value,
					Value = m.Uid.ToString(),

				});
				return Json(modelData, JsonRequestBehavior.AllowGet);
			}

			else
			{
				return Json("Empty", JsonRequestBehavior.AllowGet);
				//return Json(new JsonResult() { Data = new { Text = "Fernando", Value = "Rodriguez" } }, JsonRequestBehavior.AllowGet);
			}
		}
	}
}
