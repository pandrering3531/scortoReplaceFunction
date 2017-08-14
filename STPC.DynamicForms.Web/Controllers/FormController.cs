using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using STPC.DynamicForms.DecisionEngine;
using STPC.DynamicForms.Web.Helpers;
using STPC.DynamicForms.Web.Models;
using STPC.DynamicForms.Infraestructure;
using STPC.DynamicForms.Web.Services.ScriptGenerator;
using STPC.DynamicForms.Web.Services.Entities;
using System.Configuration;
using STPC.DynamicForms.Web.Common;

namespace STPC.DynamicForms.Web.Controllers
{
	
	public class FormController : Controller
	{
		#region Atributos y Constantes

		CustomMembershipProvider UsersProvider;
		STPC_FormsFormEntities _stpcForms;
		private IDecisionEngine _decisionEngine;
		ScriptGeneratorServiceClient _ScriptGenerator;

		public string user = System.Configuration.ConfigurationManager.AppSettings["UserWs"];
		public string pws = System.Configuration.ConfigurationManager.AppSettings["PwsWs"];
		public string pollInterval = System.Configuration.ConfigurationManager.AppSettings["pollInterval"];
		public string timeOut = System.Configuration.ConfigurationManager.AppSettings["timeOut"];

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor por Defecto
		/// Initializes a new instance of the <see cref="FormController" /> class.
		/// </summary>
		/// <param name="iEngine">The decision engine.</param>
		public FormController(IDecisionEngine iEngine)
		{
			this._ScriptGenerator = new ScriptGeneratorServiceClient();
			this.UsersProvider = (CustomMembershipProvider)Membership.Provider;
			this._stpcForms = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
			this._decisionEngine = iEngine;
		}
		public FormController()
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
		#endregion

		public ActionResult Respond(Guid id)
		{
			//Provisional - Generate stript call
			//string val = GenerateScriptString(id);
			var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == id).FirstOrDefault();
			if (thisForm == null) return View("Error");
			ViewBag.FormName = thisForm.Name;
			return View(thisForm.Pages.OrderBy(o => o.DisplayOrder));
			//return View(thisForm.Pages.OrderBy(o => o.SortOrder));
		}

		/****************************************************
     
		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult GenerateScriptResult(string id)
		{
		  //string retv = this.GenerateScriptString(id);
		  return Json(id, JsonRequestBehavior.AllowGet);
		}

		****************************************************/

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

		public ActionResult Edit(Guid id)
		{
			var item = _stpcForms.Forms.Expand("Pages").Where((x => x.Uid == id)).FirstOrDefault();
			//TODO: Refactor código repetido en otro controller
			ViewBag.StrategiesSelect = new List<SelectListItem>();

			#region cargar la lista de estrategias

			List<SelectListItem> StrategiesSelect = new List<SelectListItem>();

			StrategiesSelect.Add(
			  new SelectListItem
			  {
				  Selected = true,
				  Text = "---Seleccione estrategia---",
				  Value = "0"
			  }
			  );

			foreach (var strategy in _decisionEngine.GetStrategyList().OrderBy(str => str.Name))
				StrategiesSelect.Add(new SelectListItem { Text = strategy.Name, Value = strategy.Id.ToString() });

			ViewBag.StrategiesSelect = StrategiesSelect;

			#endregion cargar la lista de estrategias

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
		public JsonResult Create(string name)
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
			return Json(new { success = true });
		}

		public ActionResult Delete(Guid id)
		{
			if (id == null) return View("Error");
			var theForm = _stpcForms.Forms.Where(i => i.Uid == id).FirstOrDefault();
			if (theForm != null)
			{
				_stpcForms.DeleteObject(theForm);
				_stpcForms.SaveChanges();
				return RedirectToAction("List");
			}
			return View("Error");
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
				return Json(new { Success = true });
			}
			return Json(new { Success = false });
		}

		[HttpPost]
		public JsonResult SaveOrder(string iNewOrderList, string iGuidList)
		{
			string[] arrayGuid = iGuidList.Split('/');
			string[] arrayOrder = iNewOrderList.Replace("TheTable[]=", "").Split('&');

			if (arrayOrder != null && arrayOrder.Count() > 0 && arrayGuid != null && arrayGuid.Count() > 0)
			{
				for (int index = 0; index < arrayOrder.Length; index++)
				{
					if (!string.IsNullOrEmpty(arrayOrder[index]))
					{
						// buscar el Guid correcto en el array de guid
						foreach (var itemGUID in arrayGuid)
						{
							if (!string.IsNullOrEmpty(itemGUID))
							{
								if (itemGUID.Contains(arrayOrder[index].ToString()))
								{
									// actualizar el nuevo order
									var pag = _stpcForms.FormPages.Where(fp => fp.Uid == Guid.Parse(itemGUID)).FirstOrDefault();
									if (pag != null)
									{
										pag.DisplayOrder = index;

										// actualizar objeto modificado
										_stpcForms.UpdateObject(pag);
										System.Diagnostics.Debug.WriteLine(pag.Name + "[" + pag.DisplayOrder + "]");
									}

									break;
								}
							}
						}

					}
				}
			}

			_stpcForms.SaveChanges();

			return Json(new { success = true });
		}


		#region Clase Auxiliar

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

		#endregion Clase Auxiliar

	}
}