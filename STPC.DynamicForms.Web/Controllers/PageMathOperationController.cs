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
using System.Net;

namespace STPC.DynamicForms.Web.Controllers
{

	public class StructureMathOperation
	{
		public string FieldName { get; set; }
		public string FieldGuid { get; set; }
		public Boolean isConstant { get; set; }
	}
	public class PageMathOperationController : Controller
	{
		Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
		Services.Entities.STPC_FormsFormEntities _stpcFormsPage = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

		private IDecisionEngine _decisionEngine;
		CustomMembershipProvider UsersProvider;
		ScriptGeneratorServiceClient _ScriptGenerator;

		public PageMathOperationController()
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

		public PageMathOperationController(IDecisionEngine iEngine)
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
			List<PageMathOperation> listPageMathOperation = GetDataIndex(id, thisPage);

			return View(listPageMathOperation);
		}

		private List<PageMathOperation> GetDataIndex(Guid id, FormPage thisPage)
		{
			ViewBag.Data_FormPageName = thisPage.Name;
			ViewBag.Data_FormPageId = thisPage.Uid;
			ViewBag.Data_FormId = thisPage.Form.Uid;

			ViewBag.Data_FormPageId = id;



			var palensIds = thisPage.Panels.ToArray();

			List<PageMathOperation> listPageMathOperation = _stpcForms.PageMathOperation.ToList().Where(p => palensIds.ToList().Any(t => t.Uid == p.PanelUid)).ToList();
			List<PageField> ListFieldByPage = _stpcForms.PageFields.ToList().Where(p => palensIds.ToList().Any(t => t.Uid == p.PanelUid)).ToList();

			//List<PageMathOperation> listPageMathOperation = _stpcForms.PageMathOperation.Where(e => e.FormPageUid == id).ToList();
			foreach (var pageMathOperation in listPageMathOperation)
			{
				foreach (var field in ListFieldByPage)
				{
					pageMathOperation.Expression = pageMathOperation.Expression.Replace(field.Uid.ToString(), field.FormFieldName);

					if (pageMathOperation.Trigger != null)
						pageMathOperation.Trigger = pageMathOperation.Trigger.Replace(field.Uid.ToString(), field.FormFieldName);

					if (field.Uid == pageMathOperation.ResultField)
					{
						ViewData["ResultField"] = field.FormFieldName;
					}
				}
			}
			return listPageMathOperation;
		}

		[HttpPost]
		public ActionResult GetIndex(PageMathOperation model, FormCollection par)
		{
			var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == Guid.Parse(par["pageId"]))).FirstOrDefault();
			List<PageMathOperation> listPageMathOperation = GetDataIndex(Guid.Parse(par["pageId"]), thisPage);

			

			return PartialView("_Index", listPageMathOperation);
		}


		public ActionResult Create(Guid idPage)
		{
			var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == idPage)).FirstOrDefault();
			ViewBag.Panels = thisPage.Panels.ToList().OrderBy(e => e.SortOrder);
			ViewBag.Data_FormPageId = thisPage.Uid;
			ViewBag.Data_FormId = thisPage.Form.Uid;

			List<FormStates> _FormStates = _stpcFormsPage.FormStates.ToList();
			ViewBag.FormStates = _FormStates;

			var palensIds = thisPage.Panels.ToArray();

			if (palensIds != null)
			{

				ViewBag.PanelId = thisPage.Panels[0].Uid;
			}
			List<PageField> ListFieldByPage = _stpcForms.PageFields.Expand("FormFieldType").ToList().Where(
				p => palensIds.ToList().Any(t => t.Uid == p.PanelUid )).ToList();

			//List<PageField> ListFieldByPage = _stpcForms.PageFields.ToList();
			ViewBag.fieldList = ListFieldByPage.Where(e => e.FormFieldType.FieldTypeName == "Número" || e.FormFieldType.FieldTypeName == "Moneda");

			return PartialView();
		}

		[HttpPost]
		public ActionResult Create(string expretion, Guid resultField, Guid panelUid)
		{
			try
			{
				PageMathOperation _PageMathOperation = new PageMathOperation();
				_PageMathOperation.Expression = expretion;
				_PageMathOperation.ResultField = resultField;
				_PageMathOperation.PanelUid = panelUid;

				_stpcForms.AddObject("PageMathOperation", _PageMathOperation);
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


		public ActionResult Edit(Guid id, Guid idPage)
		{

			var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == idPage)).FirstOrDefault();
			ViewBag.Panels = thisPage.Panels.ToList().OrderBy(e => e.SortOrder);
			ViewBag.Data_FormPageId = thisPage.Uid;
			ViewBag.Data_FormId = thisPage.Form.Uid;
			StructureMathOperation _StructureMathOperation = new StructureMathOperation();

			List<FormStates> _FormStates = _stpcFormsPage.FormStates.ToList();
			ViewBag.FormStates = _FormStates;

			var palensIds = thisPage.Panels.ToArray();
			List<PageField> ListFieldByPage = _stpcForms.PageFields.Expand("FormFieldType").ToList().Where(
				p => palensIds.ToList().Any(t => t.Uid == p.PanelUid)).ToList();

			//List<PageField> ListFieldByPage = _stpcForms.PageFields.ToList();
			ViewBag.fieldList = ListFieldByPage.Where(e => e.FormFieldType.FieldTypeName == "Número" || e.FormFieldType.FieldTypeName == "Moneda");

			List<PageMathOperation> listPageMathOperation = _stpcForms.PageMathOperation.ToList().Where(pmo => pmo.Uid == id).ToList();
			ViewBag.pageMathOperationId = id;
			List<StructureMathOperation> listField = new List<StructureMathOperation>();
			string[,] operations;
			int posIni = -1;

			if (listPageMathOperation.Count > 0)
			{
				ViewBag.resultFieldId = listPageMathOperation[0].ResultField;
				ViewBag.resultFieldName = ListFieldByPage.Where(uid => uid.Uid == listPageMathOperation[0].ResultField).FirstOrDefault().FormFieldName;
			}

			foreach (var pageMathOperation in listPageMathOperation)
			{
				int countFields = 0;
				operations = new string[ListFieldByPage.Count, 2];

				foreach (var field in ListFieldByPage)
				{
					#region busca Campos
					posIni = 0;
					while (posIni != -1)
					{
						posIni = pageMathOperation.Expression.IndexOf(field.Uid.ToString(), posIni);

						if (posIni >= 0)
						{
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni, "|");
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni + 37, "|");
							posIni += 37;
						}
					}
					#endregion

					#region agrupadores

					posIni = 0;
					while (posIni != -1)
					{
						posIni = pageMathOperation.Expression.IndexOf("(", posIni);

						if (posIni >= 0)
						{
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni, "|");
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni + 2, "|");
							posIni += 2;
						}
					}

					posIni = 0;
					while (posIni != -1)
					{
						posIni = pageMathOperation.Expression.IndexOf(")", posIni);

						if (posIni >= 0)
						{
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni, "|");
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni + 2, "|");
							posIni += 2;
						}
					}
					#endregion

					#region Operadores

					posIni = 0;
					while (posIni != -1)
					{
						posIni = pageMathOperation.Expression.IndexOf("+", posIni);

						if (posIni >= 0)
						{
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni, "|");
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni + 2, "|");
							posIni += 2;
						}
					}

					posIni = 0;
					while (posIni != -1)
					{
						posIni = pageMathOperation.Expression.IndexOf("*", posIni);

						if (posIni >= 0)
						{
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni, "|");
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni + 2, "|");
							posIni += 2;
						}
					}

					posIni = 0;
					while (posIni != -1)
					{
						posIni = pageMathOperation.Expression.IndexOf("/", posIni);

						if (posIni >= 0)
						{
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni, "|");
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni + 2, "|");
							posIni += 2;
						}
					}
					posIni = 0;
					while (posIni != -1)
					{
						posIni = pageMathOperation.Expression.IndexOf("%", posIni);

						if (posIni >= 0)
						{
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni, "|");
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni + 2, "|");
							posIni += 2;
						}
					}
					posIni = 0;
					while (posIni != -1)
					{
						posIni = pageMathOperation.Expression.IndexOf("^", posIni);

						if (posIni >= 0)
						{
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni, "|");
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni + 2, "|");
							posIni += 2;
						}
					}
					posIni = 0;
					while (posIni != -1)
					{
						posIni = pageMathOperation.Expression.IndexOf(" - ", posIni);

						if (posIni >= 0)
						{
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni, "|");
							pageMathOperation.Expression = pageMathOperation.Expression.Insert(posIni + 2, "|");
							posIni += 2;
						}
					}
					#endregion

				}

				string[] arrayGuid;
				arrayGuid = pageMathOperation.Expression.Split('|');


				int j = 0;

				for (int i = 0; i < arrayGuid.Length; i++)
				{
					if (!String.IsNullOrEmpty(arrayGuid[i].Trim()))
					{
						_StructureMathOperation = new StructureMathOperation();
						_StructureMathOperation.isConstant = true;

						if (arrayGuid[i].Length > 35)
						{
							PageField _field = ListFieldByPage.Where(uid => uid.Uid == Guid.Parse(arrayGuid[i])).FirstOrDefault();
							_StructureMathOperation.FieldGuid = arrayGuid[i];
							_StructureMathOperation.FieldName = _field.FormFieldName.ToString();
							_StructureMathOperation.isConstant = false;
							listField.Add(_StructureMathOperation);
						}
						else
						{
							_StructureMathOperation.FieldGuid = arrayGuid[i];
							_StructureMathOperation.FieldName = arrayGuid[i];

							double Num;
							bool isNum = double.TryParse(_StructureMathOperation.FieldName, out Num);
							if (isNum)
								_StructureMathOperation.isConstant = true;
							else
								_StructureMathOperation.isConstant = true;

							listField.Add(_StructureMathOperation);
						}
					}

				}

			}
			ViewBag.fieldListMathOperation = listField;
			ViewBag.countField = listField.Count;
			return PartialView();
		}

		[HttpPost]
		public ActionResult Edit(string expretion, Guid resultField, Guid panelUid,Guid id)
		{

			PageMathOperation _PageMathOperation = _stpcForms.PageMathOperation.Where(pmo => pmo.Uid == id).FirstOrDefault();
			_PageMathOperation.Expression = expretion;
			_PageMathOperation.ResultField = resultField;
			
			_stpcForms.UpdateObject(_PageMathOperation);
			// guardar cambios
			_stpcForms.SaveChanges();


			return Json(JsonResponseFactory.SuccessResponse(), JsonRequestBehavior.AllowGet);
		}


		[HttpPost]
		public ActionResult Delete(Guid id)
		{

			try
			{
				PageMathOperation _PageMathOperation = _stpcForms.PageMathOperation.Where(e => e.Uid == id).FirstOrDefault();

				if (_PageMathOperation != null)
				{
					_stpcForms.DeleteObject(_PageMathOperation);
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
