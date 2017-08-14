using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using STPC.DynamicForms.Core.Fields;
using STPC.DynamicForms.Web.Helpers;
using STPC.DynamicForms.Web.Services.Entities;
using System.Configuration;
using STPC.DynamicForms.DecisionEngine;
using STPC.DynamicForms.Web.Services.ScriptGenerator;
using System.Web.Security;
using STPC.DynamicForms.Web.Common;

namespace STPC.DynamicForms.Web.Controllers
{
	public class JsonResponseFactory
	{
		public static object ErrorResponse(string error, string codError, bool showCaptacha = false)
		{
			return new
			{
				Success = false,
				ErrorMessage = error,
				ShowCaptcha = showCaptacha,
				codError = codError
			};
		}
		public static object ErrorResponse(string error, bool showCaptacha = false)
		{
			return new
			{
				Success = false,
				ErrorMessage = error,
				ShowCaptcha = showCaptacha
			};
		}
		public static object ErrorResponse()
		{
			return new
			{
				Success = false
			};
		}

		public static object SuccessResponse()
		{
			return new
			{
				Success = true
			};
		}

		public static object SuccessResponse(object referenceObject)
		{
			return new
			{
				Success = true,
				Object = referenceObject
			};
		}
	}

	[Authorize]
	public class FormPageController : Controller
	{
		private IDecisionEngine _decisionEngine;
		CustomMembershipProvider UsersProvider;
		ScriptGeneratorServiceClient _ScriptGenerator;
		//TODO: Sacar la URI del servicio al web.config
		Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
		Services.Entities.STPC_FormsFormEntities _stpcForms2 = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

		public FormPageController(IDecisionEngine iEngine)
		{
			this._ScriptGenerator = new ScriptGeneratorServiceClient();
			this.UsersProvider = (CustomMembershipProvider)Membership.Provider;
			this._stpcForms = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
			this._decisionEngine = iEngine;
		}

		public FormPageController()
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

		public ActionResult Edit(Guid id)
		{

			var item = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == id)).FirstOrDefault();
			ViewBag.FormPageName = item.Name;
			ViewBag.FormPageId = item.Uid;
			ViewBag.FormId = item.Form.Uid;
			ViewBag.DefaultPanelStyle = System.Configuration.ConfigurationManager.AppSettings["DefaultPanelStyle"];
			return View(item.Panels.OrderBy(o => o.SortOrder));
		}

		public ActionResult Respond(Guid id)
		{
			var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == id)).FirstOrDefault();

			if (thisPage == null) return View("Error");
			ViewData["FormUid"] = thisPage.Form.Uid;
			ViewData["PageName"] = thisPage.Name;
			ViewData["FormPageUid"] = thisPage.Uid;
			ViewData["PagePanel"] = id;
			ViewData["Paramsvalues"] = "";
			return PartialView("_Respond", thisPage.Panels.OrderBy(o => o.SortOrder));
		}

		[HttpPost]
		public ActionResult Respond(FormPage thisPage, FormCollection campos, Panel panel)
		{
			// recreate the page and set the keys
			//var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == id)).FirstOrDefault();
			//if (thisPage == null) return View("Error");
			//Demo3SetKeys(thisPage);
			//Guid g = Guid.Parse(Request.Params["FormPageUid"]);
			//var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == g)).FirstOrDefault();
			// set user input on recreated form
			//UpdateModel(thisPage);
			StringBuilder sb = new StringBuilder();
			string srNameForm = campos["FormUid"];
			Guid uidPanel = Guid.Parse(campos["PagePanel"]);
			Guid uidFormPage = Guid.Parse(campos["FormPageUid"]);
			string fieldNameUid;
			Guid idForm = new Guid();
			string srNameField;
			PageField _PageField = new PageField();
			string srValCampo;

			if (!string.IsNullOrEmpty(srNameForm))
			{
				STPC.DynamicForms.Web.Models.FormDataToXml objectForm = new Models.FormDataToXml(); //Objeto que almacena la data del formulario

				objectForm.IdForm = Guid.Parse(srNameForm);
				objectForm.Idpanel = uidPanel;
				objectForm.IdFormPage = uidFormPage;

				idForm = Guid.Parse(srNameForm);

				STPC.DynamicForms.Web.Models.FormDataToXml _formDataToXml = new Models.FormDataToXml();
				objectForm.FieldsByPage = new Dictionary<string, string>();

				for (int i = 0; i < campos.Count; i++)
				{
					fieldNameUid = ((System.Collections.Specialized.NameValueCollection)(campos)).AllKeys[i];

					if (fieldNameUid.Contains("STPC_DFi_"))
					{
						srNameField = fieldNameUid.Substring(9);
						_PageField = _stpcForms.PageFields.Where(e => e.Uid == Guid.Parse(srNameField)).FirstOrDefault();
						srValCampo = campos[i];
						objectForm.FieldsByPage.Add(_PageField.FormFieldName, srValCampo);

					}
				}
				convertToXmlFormData(objectForm);


				//foreach (var p in Request.Params.AllKeys)
				//{
				//  sb.Append("Name:" + p + ":" + Request.Params[p] + ";" + "\n");
				//}
				//ViewData["Paramsvalues"] = sb.ToString();
				////if (thisPage.Validate()) // input is valid
				////    return View("Responses", form);
				//var a = STPC.DynamicForms.Core.Utilities.SerializationUtility.Deserialize<STPC.DynamicForms.Core.Form>(Request.Params["STPC_DynamicSerializedForm"]);
				////// input is not valid
				////return View("Demo", form);

			}
			return View("_Respond", thisPage.Panels);
		}

		private string convertToXmlFormData(Models.FormDataToXml form)
		{
			DataContractSerializer serializer = new DataContractSerializer(form.GetType());

			using (StringWriter sw = new StringWriter())
			{
				using (XmlTextWriter writer = new XmlTextWriter(sw))
				{
					// add formatting so the XML is easy to read in the log
					writer.Formatting = Formatting.Indented;

					serializer.WriteObject(writer, form);

					writer.Flush();

					return sw.ToString();
				}
			}
		}
		//private XmlDocument convertToXmlFormData(Models.formDataToXml form)
		//{
		//  StringWriter stringWriter = new StringWriter();
		//  XmlDocument xmlDoc = new XmlDocument();

		//  XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);

		//  XmlSerializer serializer = new XmlSerializer(typeof(Models.formDataToXml));



		//  serializer.Serialize(xmlWriter, form);

		//  string xmlResult = stringWriter.ToString();

		//  xmlDoc.LoadXml(xmlResult);

		//  return xmlDoc;

		//}
		void Demo3SetKeys(FormPage page)
		{
			foreach (var p in page.Panels)
			{
				foreach (var field in p.PanelFields)
				{
					field.Uid = Guid.NewGuid();
				}
			}
		}

		public ActionResult CreateFromTemplate()
		{
			STPC.DynamicForms.Web.Models.FormPageViewModel fpvm = new Models.FormPageViewModel();
			var templ = from items in _stpcForms.PageTemplates.Expand("PanelTemplates")
							select items;
			fpvm.Templates = templ.ToList();
			return View();
		}

		public ActionResult Delete(Guid id)
		{
			if (id == null) return View("Error");
			Guid theFormOfThePage;
			var thePage = _stpcForms.FormPages.Expand("Form").Where(i => i.Uid == id).FirstOrDefault();
			if (thePage != null)
			{
				theFormOfThePage = thePage.Form.Uid;
				_stpcForms.DeleteObject(thePage);
				_stpcForms.SaveChanges();
				return RedirectToAction("Edit", "Form", new { id = theFormOfThePage });
			}
			return View("Error");
		}

		[HttpPost]
		public ActionResult DeletePost(Guid id)
		{
			if (id == null) return View("Error");
			Guid theFormOfThePage;
			var thePage = _stpcForms.FormPages.Expand("Form").Where(i => i.Uid == id).FirstOrDefault();
			if (thePage != null)
			{
				theFormOfThePage = thePage.Form.Uid;
				_stpcForms.DeleteObject(thePage);
				_stpcForms.SaveChanges();
				return Json(new { Success = true });
			}
			return Json(new { Success = false });
		}

		[HttpPost]
		public JsonResult Create(string name, string desc, int strategy, Guid formId)
		{
			var theForm = _stpcForms.Forms.Expand("Pages").Where(i => i.Uid == formId).FirstOrDefault();
			int theOrder;
			if (theForm.Pages.Count == 0) theOrder = 0;
			else
				theOrder = theForm.Pages.Select(m => m.DisplayOrder).Max() + 1;

			//theOrder = theForm.Pages.Select(m => m.SortOrder).Max() + 1;

			var theNewPage = new STPC.DynamicForms.Web.Services.Entities.FormPage
			{
				Name = name,
				Description = desc,
				StrategyID = strategy,
				Timestamp = DateTime.Now,
				SortOrder = theOrder,
				ShortPath = RandomString(5),
				DisplayOrder = theOrder,
			};


			_stpcForms.AddRelatedObject(theForm, "Pages", theNewPage);

			System.Data.Services.Client.DataServiceResponse response = _stpcForms.SaveChanges();
			Guid? newPageFormid = null;

			foreach (System.Data.Services.Client.ChangeOperationResponse change in response)
			{
				System.Data.Services.Client.EntityDescriptor descriptor = change.Descriptor as System.Data.Services.Client.EntityDescriptor;
				if (descriptor != null)
				{
					STPC.DynamicForms.Web.Services.Entities.FormPage addedFormPage = descriptor.Entity as STPC.DynamicForms.Web.Services.Entities.FormPage;

					if (addedFormPage != null)
					{
						newPageFormid = addedFormPage.Uid;

						List<FormPageActions> listFormPageActions = GetListFormPageActions(addedFormPage.Uid);
						foreach (var item in listFormPageActions)
						{

							var theNewPageAction = new STPC.DynamicForms.Web.Services.Entities.FormPageActions
							{
								Caption = item.Caption,
								Description = item.Description,
								IsAssociated = false,
								Name = item.Name,
								PageId = item.PageId
							};
							_stpcForms.AddObject("FormPageActions", theNewPageAction);

							//_stpcForms.AddRelatedObject(TheFormPage, "FormPageActions", theNewPageAction);

							System.Data.Services.Client.DataServiceResponse response2 = _stpcForms.SaveChanges();
						}
					}
				}
			}
			return Json(new { success = true, Name = name, Desc = desc, Strategy = strategy, uid = newPageFormid });
		}

		public string RandomString(int size)
		{
			var builder = new StringBuilder();
			var random = new Random();
			for (var i = 0; i < size; i++)
			{
				var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
				builder.Append(ch);
			}
			return builder.ToString().ToLowerInvariant();
		}


		// accion para cargar los botones del page
		public ActionResult FormPageAction(Guid id)
		{
			#region Consultar Acciones por pagina

			// se adiciona a la consulta ordernar la nueva columna SortOrder [11/02/2013]
			//List<FormPageActions> listActions = _stpcForms.FormPageActions.Expand("FormPageActionsRolesList").Expand("FormPageActionsByStatesList").Where(i => i.PageId == id).OrderBy(fpg => fpg.DisplayOrder).ToList();
			_stpcForms.IgnoreResourceNotFoundException = true;
			List<FormPageActions> listActions = _stpcForms.FormPageActions.Expand("FormPageActionsRolesList").Expand("FormPageActionsByStatesList/FormStates").Expand("FormStates").Where(i => i.PageId == id).OrderBy(fpg => fpg.DisplayOrder).ToList();

			//foreach (var action in listActions)
			//{
			//  List<FormPageActionsRoles> listFormPageActions = _stpcForms.FormPageActionsRoles.Where(e => e.FormPageActionsUid == action.Uid).ToList();
			//  foreach (var actionRole in listFormPageActions)
			//  {
			//    action.FormPageActionsRolesList.Add(actionRole);

			//  }
			//}
			ViewBag.listFormPageActions = GetListFormPageActions(id).ToList();


			List<FormPageActions> lstFormPageActions = listActions;

			#endregion Consultar Acciones por pagina


			var item = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == id)).FirstOrDefault();
			ViewBag.FormPageName = item.Name;
			ViewBag.FormPageId = item.Uid;
			ViewBag.FormId = item.Form.Uid;
			ViewBag.StrategiesSelect = new List<SelectListItem>();

			ViewBag.StrategiesSelect.Add(new SelectListItem { Text = "---Seleccione estrategia---", Value = "0", Selected = (item.StrategyID == 0 ? true : false) });

			foreach (var strategy in _decisionEngine.GetStrategyList().OrderBy(str => str.Name))
				ViewBag.StrategiesSelect.Add(new SelectListItem { Text = strategy.Name, Value = strategy.Id.ToString(), Selected = (item.StrategyID == strategy.Id ? true : false) });


			ViewBag.PageStrategy = item.StrategyID.ToString();
			List<string> roles = Roles.GetAllRoles().ToList();
			ViewBag.roles = roles.OrderBy(q => q);

			ViewBag.states = _stpcForms.FormStates.OrderBy(e => e.StateName).ToList();
			//ViewBag.FormStates = _stpcForms2.FormPageActionsByStates.Where().Expand("FormStates").ToList();
			//var aaaa = from e in _stpcForms2.FormPageActionsByStates
			//			  where idActions.Contains(e.FormPageActionsUid)
			//			  select e;

			return View(lstFormPageActions);


		}



		// accion para guardar el orden de las acciones de page
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
									var action = _stpcForms.FormPageActions.Where(fpg => fpg.Uid == Guid.Parse(itemGUID)).FirstOrDefault();

									if (action != null)
									{
										action.DisplayOrder = index;

										// actualizar objeto modificado
										_stpcForms.UpdateObject(action);
										System.Diagnostics.Debug.WriteLine(action.Name + "[" + action.DisplayOrder + "]");
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

		[HttpPost]
		public JsonResult SavePanelOrder(string iNewOrderList, string iGuidList)
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
									var pan = _stpcForms.Panels.Where(p => p.Uid == Guid.Parse(itemGUID)).FirstOrDefault();
									if (pan != null)
									{
										pan.SortOrder = index;

										// actualizar objeto modificado
										_stpcForms.UpdateObject(pan);
										System.Diagnostics.Debug.WriteLine(pan.Name + "[" + pan.SortOrder + "]");
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


		/// <summary>
		/// Servicio que retorna la lista de acciones por defecto posibles de una pagina
		/// </summary>
		/// <returns></returns>
		public List<FormPageActions> GetListFormPageActions(Guid pageId)
		{
			List<FormPageActions> lstFormPageActions = new List<FormPageActions>
      {
        new FormPageActions{ Uid = new Guid(), Name="Guardar", Description="Guardar cambios", Caption="Guardar", IsAssociated=true, PageId = pageId,Save=true},
        new FormPageActions{ Uid = new Guid(), Name="Cancelar", Description="Cancelar", Caption="Cancelar", IsAssociated=false, PageId = pageId},
        new FormPageActions{ Uid = new Guid(), Name="Siguiente", Description="Página siguiente", Caption="Página siguiente", IsAssociated=true, PageId = pageId},
        new FormPageActions{ Uid = new Guid(), Name="Anterior", Description="Página anterior", Caption="Página anterior", IsAssociated=true, PageId = pageId},
        new FormPageActions{ Uid = new Guid(), Name="IrPaginaEspecifica", Description="Ir a", Caption="Ir a", IsAssociated=true, PageId = pageId},
        new FormPageActions{ Uid = new Guid(), Name="CerrarFormulario", Description="Cerrar formulario", Caption="Cerrar formulario", IsAssociated=true, PageId = pageId},
        new FormPageActions{ Uid = new Guid(), Name="CerraSesión", Description="Cerrar sesión", Caption="Cerrar sesión", IsAssociated=true, PageId = pageId},
		  new FormPageActions{ Uid = new Guid(), Name="VerReporte", Description="Ver reporte", Caption="Ver reporte", IsAssociated=true, PageId = pageId},

      };
			return lstFormPageActions;
		}

		[HttpPost]
		public ActionResult UpdateActions(FormCollection par, List<FormPageActions> model)
		{
			string[] Acciones = null;
			string[] arrRoles = null;
			string[] arrEstados = null;
			string[] arrGoToPage = null;
			FormPageActions _FormPageAction = null;
			int posArrayCaption = 0;


			string[] AccionesAsociadas = null;
			string[] EjecutaEstrategia = null;
			string[] Guarda = null;
			Acciones = par["action.Uid"].Split(new Char[] { ',' });
			string[] arrayOrder = par["arrayOrder"].Replace("TheTable[]=", "").Split('&');
			string srActionsOrder = string.Empty;

			if (arrayOrder != null && arrayOrder.Count() > 0 && Acciones != null && Acciones.Count() > 0)
			{
				for (int index = 0; index < arrayOrder.Length; index++)
				{
					if (!string.IsNullOrEmpty(arrayOrder[index]))
					{
						// buscar el Guid correcto en el array de guid
						foreach (var itemGUID in Acciones)
						{
							if (!string.IsNullOrEmpty(itemGUID))
							{
								if (itemGUID.Contains(arrayOrder[index].ToString()))
								{
									// actualizar el nuevo order
									var action = _stpcForms.FormPageActions.Where(fpg => fpg.Uid == Guid.Parse(itemGUID)).FirstOrDefault();

									if (action != null)
									{
										srActionsOrder += "/" + action.Uid;
									}

									break;
								}
							}
						}

					}
				}
			}

			if (!string.IsNullOrEmpty(srActionsOrder))
				Acciones = srActionsOrder.Split(new Char[] { '/' });



			string[] listRoles = Roles.GetAllRoles();
			for (int i = 0; i < Acciones.Length; i++)
			{
				if (Acciones[i] != "")
				{

					AccionesAsociadas = par["Asociar" + Acciones[i]].Split(new Char[] { ',' });

					if (par["Guarda" + Acciones[i]] != null)
						Guarda = par["Guarda" + Acciones[i]].Split(new Char[] { ',' });


					if (par["EjecutaEstrategia" + Acciones[i]] != null)

						EjecutaEstrategia = par["EjecutaEstrategia" + Acciones[i]].Split(new Char[] { ',' });

					_FormPageAction = _stpcForms2.FormPageActions.Expand("FormPageActionsByStatesList/FormStates").Where(e => e.PageId == Guid.Parse(par["PageId"]) && e.Uid == Guid.Parse(Acciones[i])).FirstOrDefault();

					if (AccionesAsociadas.Length == 2)
					{
						if (_FormPageAction != null)
						{
							_FormPageAction.IsAssociated = true;

						}
					}
					else
					{
						if (_FormPageAction != null)
						{
							_stpcForms2.DeleteObject(_FormPageAction);

							//foreach (var item in _FormPageAction.FormPageActionsByStatesList)
							//{
							//  _stpcForms.DeleteObject(item);
							//}
							_stpcForms2.SaveChanges();
							continue;
						}
					}

					if (par["EjecutaEstrategia" + Acciones[i]] != null)
						if (EjecutaEstrategia.Length == 2)
						{
							if (_FormPageAction != null)
							{
								_FormPageAction.IsExecuteStrategy = true;

							}
						}
						else
						{
							if (_FormPageAction != null)
							{
								_FormPageAction.IsExecuteStrategy = false;

							}
						}

					if (par["Guarda" + Acciones[i]] != null)
						if (Guarda.Length == 2)
						{
							if (_FormPageAction != null)
							{
								_FormPageAction.Save = true;

							}
						}
						else
						{
							if (_FormPageAction != null)
							{
								_FormPageAction.Save = false;

							}
						}

					string[] AccionesCaption = null;

					AccionesCaption = par["action.Caption"].Split(new Char[] { ',' });

					//bool a = Array.Exists(par.AllKeys, element => element == "ActionId");

					if ((par[_FormPageAction.Uid + "GoToPage"]) != string.Empty)
					{
						arrGoToPage = par[_FormPageAction.Uid + "GoToPage"].Split(new Char[] { '/' });

						if (Guid.Parse(arrGoToPage[1]) == _FormPageAction.Uid)
						{
							if (arrGoToPage[0] != "")
								_FormPageAction.GoToPageId = Guid.Parse(arrGoToPage[0]);
							else
								_FormPageAction.GoToPageId = null;
						}
					}

					if ((par[_FormPageAction.Uid + "StateGuid"]) != string.Empty)
					{
						arrEstados = par[_FormPageAction.Uid + "StateGuid"].Split(new Char[] { '/' });

						if (Guid.Parse(arrEstados[1]) == _FormPageAction.Uid)
						{
							if (arrEstados[0] != "")
								_FormPageAction.FormStatesUid = Guid.Parse(arrEstados[0]);
							else
								_FormPageAction.FormStatesUid = null;
						}
					}


					_FormPageAction.Caption = AccionesCaption[i];
					_stpcForms2.UpdateObject(_FormPageAction);
					posArrayCaption++;

				}

			}
			//Actualiza el id de la estrategia
			string strategyId = par["PageStrategy"];

			FormPage _FormPage = _stpcForms.FormPages.Where(e => e.Uid == Guid.Parse(par["PageId"])).FirstOrDefault();

			if (_FormPage != null)
				if (!string.IsNullOrEmpty(strategyId))
				{
					_FormPage.StrategyID = Convert.ToInt32(strategyId);
					_stpcForms.UpdateObject(_FormPage);
				}

			_stpcForms.SaveChanges();
			_stpcForms2.SaveChanges();
			return RedirectToAction("FormPageAction", "FormPage", new { id = _FormPage.Uid });
		}


		public ActionResult GetPagesOfForm(Guid FormPageId, Guid FormId, Guid FormaActionId)
		{

			Form item = _stpcForms.Forms.Expand("Pages").Where((x => x.Uid == FormId)).ToList().FirstOrDefault();
			List<FormPage> listFormPage = item.Pages.Where(x => x.Uid != FormPageId).ToList();
			listFormPage = listFormPage.Where(x => x.Uid != FormPageId).ToList();
			ViewBag.ActionId = FormaActionId;
			return PartialView(listFormPage);
		}

		[HttpPost]
		public ActionResult GetPagesOfForm(Guid id, Guid actionId)
		{
			FormPageActions _formPageAction = null;
			_formPageAction = _stpcForms.FormPageActions.Where(e => e.Uid == actionId).FirstOrDefault();

			if (_formPageAction != null)
			{
				_formPageAction.GoToPageId = id;
				_stpcForms.UpdateObject(_formPageAction);
				_stpcForms.SaveChanges();
			}
			return Json(new { Success = true, name = id.ToString(), ActionId = actionId });
		}

		public ActionResult GetPagesStates(Guid FormPageId, Guid FormId, Guid FormaActionId)
		{
			FormPageActions _formPageAction = null;
			_formPageAction = _stpcForms.FormPageActions.Where(e => e.Uid == FormaActionId).FirstOrDefault();

			List<FormStates> _FormStates = new List<FormStates>();
			ViewBag.ActionId = FormaActionId;
			ViewBag.formState = _formPageAction.FormStatesUid;

			_FormStates = _stpcForms.FormStates.ToList();

			return PartialView(_FormStates);
		}

		[HttpPost]
		public ActionResult GetPagesStates(Guid id, Guid actionId, string stateName)
		{
			FormPageActions _formPageAction = null;
			_formPageAction = _stpcForms.FormPageActions.Where(e => e.Uid == actionId).FirstOrDefault();

			if (_formPageAction != null)
			{
				_formPageAction.FormStatesUid = id;
				_stpcForms.UpdateObject(_formPageAction);
				_stpcForms.SaveChanges();
				ViewBag.IdAction = actionId;
			}
			return Json(new { Success = true, name = id.ToString(), ActionId = actionId, NameEstado = stateName });
		}

		public ActionResult AddAction(Guid FormPageId)
		{
			List<FormPageActions> listActions = GetListFormPageActions(FormPageId).ToList();
			return PartialView(listActions);
		}

		[HttpPost]
		public ActionResult SaveAction(string srNameAction, Guid FormPageId)
		{
			List<FormPageActions> listActions = _stpcForms.FormPageActions.Where(i => i.PageId == FormPageId).OrderBy(fpg => fpg.DisplayOrder).ToList();

			List<FormPageActions> listFormPageActions = GetListFormPageActions(FormPageId);
			FormPageActions _FormPageActions = listFormPageActions.Where(e => e.Name == srNameAction).FirstOrDefault();

			var theNewPageAction = new STPC.DynamicForms.Web.Services.Entities.FormPageActions
			{
				Caption = _FormPageActions.Caption,
				Description = _FormPageActions.Description,
				IsAssociated = true,

				Name = _FormPageActions.Name,
				PageId = _FormPageActions.PageId,
				//DisplayOrder = _order,  // se adiciona la nueva columna SortOrder [11/02/2013]

			};
			_stpcForms.AddObject("FormPageActions", theNewPageAction);

			//_stpcForms.AddRelatedObject(TheFormPage, "FormPageActions", theNewPageAction);
			listActions.Add(theNewPageAction);

			System.Data.Services.Client.DataServiceResponse response2 = _stpcForms.SaveChanges();

			//return PartialView("_FormPageActions");
			return Json(new { Success = true, name = srNameAction });
		}

		[HttpPost]
		public ActionResult UpdateSaveAction(Guid FormaActionId, bool state)
		{
			FormPageActions _formPageAction = null;
			_formPageAction = _stpcForms.FormPageActions.Where(e => e.Uid == FormaActionId).FirstOrDefault();

			if (_formPageAction != null)
			{
				_formPageAction.Save = state;
				_stpcForms.UpdateObject(_formPageAction);
				_stpcForms.SaveChanges();
				ViewBag.IdAction = FormaActionId;
			}
			return Json(new { Success = true, name = FormaActionId.ToString(), ActionId = FormaActionId });
		}

		[HttpPost]
		public ActionResult UpdateExecuteStrategy(Guid FormaActionId, bool state)
		{
			FormPageActions _formPageAction = null;
			_formPageAction = _stpcForms.FormPageActions.Where(e => e.Uid == FormaActionId).FirstOrDefault();

			if (_formPageAction != null)
			{
				_formPageAction.IsExecuteStrategy = state;
				_stpcForms.UpdateObject(_formPageAction);
				_stpcForms.SaveChanges();
				ViewBag.IdAction = FormaActionId;
			}
			return Json(new { Success = true, name = FormaActionId.ToString(), ActionId = FormaActionId });
		}

		[HttpPost]
		public ActionResult UpdateRoolByAction(string RollName, Guid FormaActionId, bool state)
		{
			try
			{



				FormPageActionsRoles _FormPageActionsRoles = null;
				_FormPageActionsRoles = _stpcForms.FormPageActionsRoles.Where(e => e.Rolename == RollName && e.FormPageActionsUid == FormaActionId).FirstOrDefault();

				if (_FormPageActionsRoles == null)
				{

					var _FormPageActionsRolesNew = new STPC.DynamicForms.Web.Services.Entities.FormPageActionsRoles
					{
						Rolename = RollName,
						FormPageActionsUid = FormaActionId
						//DisplayOrder = _order,  // se adiciona la nueva columna SortOrder [11/02/2013]

					};

					_stpcForms.AddObject("FormPageActionsRoles", _FormPageActionsRolesNew);

					//_stpcForms.AddRelatedObject(TheFormPage, "FormPageActions", theNewPageAction);


					ViewBag.IdAction = FormaActionId;

				}
				else
				{
					if (!state)
						_stpcForms.DeleteObject(_FormPageActionsRoles);
				}
				_stpcForms.SaveChanges();
				return Json(new { Success = true, name = FormaActionId.ToString(), ActionId = FormaActionId });
			}
			catch (Exception)
			{

				throw;
			}
		}

		[HttpPost]
		public ActionResult UpdateStateByAction(Guid StateId, Guid FormaActionId, bool state)
		{
			try
			{



				FormPageActionsByStates _FormPageActionsByStates = null;
				_FormPageActionsByStates = _stpcForms.FormPageActionsByStates.Where(e => e.FormStatesUid == StateId && e.FormPageActionsUid == FormaActionId).FirstOrDefault();

				if (_FormPageActionsByStates == null)
				{

					var _FormPageActionsByStatesNew = new STPC.DynamicForms.Web.Services.Entities.FormPageActionsByStates
					{
						FormStatesUid = StateId,
						FormPageActionsUid = FormaActionId
						//DisplayOrder = _order,  // se adiciona la nueva columna SortOrder [11/02/2013]

					};

					_stpcForms.AddObject("FormPageActionsByStates", _FormPageActionsByStatesNew);

					//_stpcForms.AddRelatedObject(TheFormPage, "FormPageActions", theNewPageAction);


					ViewBag.IdAction = FormaActionId;

				}
				else
				{
					if (!state)
						_stpcForms.DeleteObject(_FormPageActionsByStates);
				}
				_stpcForms.SaveChanges();
				return Json(new { Success = true, name = FormaActionId.ToString(), ActionId = FormaActionId });
			}
			catch (Exception)
			{

				throw;
			}
		}
	}
}
