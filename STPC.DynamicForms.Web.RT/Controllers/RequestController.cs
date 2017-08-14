using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using STPC.DynamicForms.Core;
using STPC.DynamicForms.Core.Fields;
using STPC.DynamicForms.DecisionEngine;
using STPC.DynamicForms.Web.RT.Helpers;
using STPC.DynamicForms.Web.RT.Models;
using STPC.DynamicForms.Web.RT.Services.Entities;
using STPC.DynamicForms.Web.RT.Services.Request;
using System.Configuration;
using STPC.DynamicForms.Web.Common;
using System.Net;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Common.Messages;

namespace STPC.DynamicForms.Web.RT.Controllers
{
	[Authorize]
    
    
	public class RequestController : Controller
	{

		public static string nodeString;
		STPC_FormsFormEntities _stpcForms;
		STPC_FormsFormEntities _stpcForms2;
		List<Hierarchy> listHierarchy = null;
		List<string> listHierarchylistTemp = null;

		CustomMembershipProvider UsersProvider = (CustomMembershipProvider)Membership.Provider;
		STPC_FormsFormEntities db = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
		//Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri("http://localhost:6615/FormsPersistenceService.svc/"));
		CustomRequestProvider _RequestServiceClient = new CustomRequestProvider();

		private IDecisionEngine _decisionEngine;

		public RequestController(IDecisionEngine decisionEngine)
		{
			_decisionEngine = decisionEngine;
			this._stpcForms = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
			this._stpcForms2 = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

		}

		public RequestController()
		{
			string user = System.Configuration.ConfigurationManager.AppSettings["UserWs"];
			string pws = System.Configuration.ConfigurationManager.AppSettings["PwsWs"];
			string pollInterval = System.Configuration.ConfigurationManager.AppSettings["pollInterval"];
			string timeOut = System.Configuration.ConfigurationManager.AppSettings["timeOut"];
			STPC.DynamicForms.DecisionEngine.DecisionEngine iEngine = new STPC.DynamicForms.DecisionEngine.DecisionEngine(user, pws, Convert.ToInt32(pollInterval), Convert.ToInt32(timeOut));
			this._decisionEngine = iEngine;

			this._stpcForms = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
			this._stpcForms2 = new STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

			_stpcForms.IgnoreResourceNotFoundException = true;
			_stpcForms2.IgnoreResourceNotFoundException = true;


		}

		public ActionResult List()
		{
			try
			{
				_stpcForms.IgnoreResourceNotFoundException = true;
				System.Data.Services.Client.DataServiceQuery<STPC.DynamicForms.Web.RT.Services.Entities.Category> query =
					  _stpcForms.Categories.Expand("Options");

				List<STPC.DynamicForms.Web.RT.Services.Entities.Category> categoryTiposDocumento = query.Where(n => n.Name == "Tipo de documentos").ToList();
				List<STPC.DynamicForms.Web.RT.Services.Entities.Category> categoryEstadosSolicitud = query.Where(n => n.Name == "Estados solicitud").ToList();


				if (categoryTiposDocumento.Count() > 0)
				{
					ViewBag.LisTiposDocumento = categoryTiposDocumento.FirstOrDefault().Options;
					ViewBag.LisEstadosSolicitud = categoryEstadosSolicitud.FirstOrDefault().Options;

				}

				//valida si es singleTenant y carga los usuarios dependiendo del aplicationId
				setViewBagUsers();

				ViewBag.FormStates = _stpcForms.FormStates.ToList().OrderBy(e => e.StateName);

				ViewBag.lisFormsType = _stpcForms.Forms.OrderBy(e => e.Name);
				var model = _stpcForms.Hierarchies.Expand("Parent").Where(jer => jer.IsActive == true).ToList();
				return View(model);
			}
			catch (Exception ex)
			{
				return DefaultActionErrorHandling(ex, "List");
			}
		}

		private void setViewBagUsers()
		{
			//Valida si es multiempresa
			if (ConfigurationManager.AppSettings["IsSingleTenant"] != null)
			{
				string IsSingleTenant = ConfigurationManager.AppSettings["IsSingleTenant"].ToString();

				if (IsSingleTenant == "1")
				{
					var theUser = new CustomMembershipProvider().GetUser(this.User.Identity.Name);
					ViewBag.LisUsers = UsersProvider.GetAllUsersByAplicationName(Convert.ToInt32(theUser.AplicationNameId)).OrderBy(e => e.GivenName);
				}
				else
					ViewBag.LisUsers = UsersProvider.GetAllUsersCompleteName().OrderBy(e => e.GivenName);
			}
			else
				ViewBag.LisUsers = UsersProvider.GetAllUsersCompleteName().OrderBy(e => e.GivenName);
		}


		public ActionResult LoadFieldsByForm(string uid)
		{

			var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == Guid.Parse(uid)).FirstOrDefault();
			List<PageField> listPageFields = new List<PageField>();

			foreach (Services.Entities.FormPage fp in thisForm.Pages)
			{
				var formPage = _stpcForms2.FormPages.Expand("Panels").Where(fpage => fpage.Uid == fp.Uid).FirstOrDefault();
				foreach (Services.Entities.Panel p in formPage.Panels)
				{
					var fields = _stpcForms.PageFields.Expand("FormFieldType").Where(xp => xp.Panel.Uid == p.Uid && xp.Queryable == true).ToList();
					var flist = fields.ToList();

					foreach (var item in flist)
					{
						listPageFields.Add(item);
					}
				}
			}
			var dynamicFormFields = new List<Field>();
			if (listPageFields.Count() > 0)
			{

				foreach (var formField in listPageFields.OrderBy(e => e.FormFieldPrompt))
				{
					var field = formField;
					var thisFieldType = field.FormFieldType;
					string FieldTypeNumericName = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameNumeric"]) ? System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameNumeric"].ToString() : string.Empty;
					string GuidFormFieldTypeEmail = System.Configuration.ConfigurationManager.AppSettings["GuidFormFieldTypeEmail"];
					string FieldTypeCurrency = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameCurrency"]) ? System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameCurrency"].ToString() : string.Empty;

					switch (thisFieldType.FieldType)
					{
						case ("Literal"):
							dynamicFormFields.Add(new TextBox()
							{
								Key = field.Uid.ToString(),
								Template =
									 String.Format("<p>{0}</p>",
														field.LiteralText.KillHtml()),
								DisplayOrder = field.SortOrder
							});
							break;
						case ("TextBox"):
							dynamicFormFields.Add(new TextBox()
							{

								isEmail = field.FormFieldType.Uid == Guid.Parse(GuidFormFieldTypeEmail) ? true : false,
								ToolTip = field.ToolTip,
								MinSize = field.MinSize,
								//mathExpresion = _PageMathOperation == null ? string.Empty: _PageMathOperation.Expression,																
								IsNumber = (field.FormFieldType.FieldTypeName == FieldTypeNumericName ? true : false),
								IsCurrency = (field.FormFieldType.FieldTypeName.Equals(FieldTypeCurrency) ? true : false),
								IsText = (field.FormFieldType.FieldTypeName.Equals("Solo Texto") ? true : false),
								MaxSize = !string.IsNullOrEmpty(field.MaxSize) ? field.MaxSize : field.MaxSizeBD,
								Key = field.Uid.ToString(),

								ResponseTitle = field.FormFieldName,
								Prompt =
									 (!string.IsNullOrEmpty(field.FormFieldPrompt))
										  ? field.FormFieldPrompt
										  : null,


								// se cambia la propiedad de orden
								//DisplayOrder = field.SortOrder


								RequiredMessage =
									 Convert.ToBoolean(field.IsRequired)
										  ? thisFieldType.ErrorMsgRequired.Replace(
												"%FormFieldName%", field.FormFieldName)
										  : string.Empty,
								RegularExpression =
									 (!string.IsNullOrEmpty(thisFieldType.RegExDefault))
										  ? thisFieldType.RegExDefault
										  : string.Empty,
								RegexMessage =
									 (!string.IsNullOrEmpty(thisFieldType.RegExDefault))
										  ? thisFieldType.ErrorMsgRegEx.Replace(
												"%FormFieldName%", field.FormFieldName)
										  : string.Empty,
								ViewRoles = field.ViewRoles,
								EditRoles = field.EditRoles,
								User = this.User.Identity.Name,

							});
							break;
						case ("Calendar"):
							dynamicFormFields.Add(new Calendar()
							{
								Key = field.Uid.ToString(),
								ResponseTitle = field.FormFieldName,
								Prompt =
									 (!string.IsNullOrEmpty(field.FormFieldPrompt))
										  ? field.FormFieldPrompt
										  : null,
								DisplayOrder = field.SortOrder,
								Required = Convert.ToBoolean(field.IsRequired),
								RequiredMessage =
									 Convert.ToBoolean(field.IsRequired)
										  ? thisFieldType.ErrorMsgRequired.Replace(
												"%FormFieldName%", field.FormFieldName)
										  : string.Empty,
								RegularExpression =
									 (!string.IsNullOrEmpty(thisFieldType.RegExDefault))
										  ? thisFieldType.RegExDefault
										  : string.Empty,
								RegexMessage =
									 (!string.IsNullOrEmpty(thisFieldType.RegExDefault))
										  ? thisFieldType.ErrorMsgRegEx.Replace(
												"%FormFieldName%", field.FormFieldName)
										  : string.Empty
							});
							break;
						case ("TextArea"):
							var newTextArea = new TextArea()
							{
								Key = field.Uid.ToString(),
								ResponseTitle = field.FormFieldName,
								Prompt =
									 (!string.IsNullOrEmpty(field.FormFieldPrompt))
										  ? field.FormFieldPrompt
										  : null,
								DisplayOrder = field.SortOrder,
								Required = Convert.ToBoolean(field.IsRequired),
								RequiredMessage =
									 Convert.ToBoolean(field.IsRequired)
										  ? thisFieldType.ErrorMsgRequired.Replace(
												"%FormFieldName%", field.FormFieldName)
										  : string.Empty,
								RegularExpression =
									 (!string.IsNullOrEmpty(thisFieldType.RegExDefault))
										  ? thisFieldType.RegExDefault
										  : string.Empty,
								RegexMessage =
									 (!string.IsNullOrEmpty(thisFieldType.RegExDefault))
										  ? thisFieldType.ErrorMsgRegEx.Replace(
												"%FormFieldName%",
												field.FormFieldName)
										  : string.Empty
							};
							int number;
							if (Int32.TryParse(field.Rows.ToString(), out number))
								newTextArea.InputHtmlAttributes.Add("rows", field.Rows.ToString());
							if (Int32.TryParse(field.Cols.ToString(), out number))
								newTextArea.InputHtmlAttributes.Add("cols", field.Cols.ToString());
							dynamicFormFields.Add(newTextArea);
							break;
						case ("SelectList"):
							var newSelectList = new Select
							{
								Key = field.Uid.ToString(),
								ResponseTitle = field.FormFieldName,
								Prompt =
									 (!string.IsNullOrEmpty(field.FormFieldPrompt))
										  ? field.FormFieldPrompt
										  : null,
								DisplayOrder = field.SortOrder,
								Required = Convert.ToBoolean(field.IsRequired),
								RequiredMessage =
									 Convert.ToBoolean(field.IsRequired)
										  ? thisFieldType.ErrorMsgRequired.Replace(
												"%FormFieldName%", field.FormFieldName)
										  : string.Empty
							};
							if (Convert.ToBoolean(field.IsMultipleSelect))
							{
								newSelectList.MultipleSelection = true;
								newSelectList.Size = Convert.ToInt32(field.ListSize);
								newSelectList.CommaDelimitedChoices = GetOptionsForSelector(field);
							}
							else
							{
								newSelectList.ShowEmptyOption = Convert.ToBoolean(field.IsEmptyOption);
								newSelectList.EmptyOption = (Convert.ToBoolean(field.IsEmptyOption))
																		  ? field.EmptyOption
																		  : null;
								newSelectList.CommaDelimitedChoices = GetOptionsForSelector(field);
							}
							dynamicFormFields.Add(newSelectList);
							break;
						case ("CheckBox"):
							var newCheckBox = new CheckBox
							{
								Key = field.Uid.ToString(),
								ResponseTitle = field.FormFieldName,
								Prompt =
									 (!string.IsNullOrEmpty(field.FormFieldPrompt))
										  ? field.FormFieldPrompt
										  : null,
								DisplayOrder = field.SortOrder,
								Required = Convert.ToBoolean(field.IsRequired),
								RequiredMessage =
									 Convert.ToBoolean(field.IsRequired)
										  ? thisFieldType.ErrorMsgRequired.Replace(
												"%FormFieldName%", field.FormFieldName)
										  : string.Empty
							};
							dynamicFormFields.Add(newCheckBox);
							break;
						case ("CheckBoxList"):
							var newCheckBoxList = new CheckBoxList
							{
								Key = field.Uid.ToString(),
								ResponseTitle = field.FormFieldName,
								Prompt =
									 (!string.IsNullOrEmpty(field.FormFieldPrompt))
										  ? field.FormFieldPrompt
										  : null,
								DisplayOrder = field.SortOrder,
								Required = Convert.ToBoolean(field.IsRequired),
								RequiredMessage =
									 Convert.ToBoolean(field.IsRequired)
										  ? thisFieldType.ErrorMsgRequired.Replace(
												"%FormFieldName%", field.FormFieldName)
										  : string.Empty,
								CommaDelimitedChoices = GetOptionsForSelector(field),
								Orientation = (field.Orientation == "vertical") ? Orientation.Vertical : Orientation.Horizontal
							};
							dynamicFormFields.Add(newCheckBoxList);
							break;
						case ("RadioList"):
							var newRadioList = new RadioList
							{
								Key = field.Uid.ToString(),
								ResponseTitle = field.FormFieldName,
								Prompt =
									 (!string.IsNullOrEmpty(field.FormFieldPrompt))
										  ? field.FormFieldPrompt
										  : null,
								DisplayOrder = field.SortOrder,
								Required = Convert.ToBoolean(field.IsRequired),
								RequiredMessage =
									 Convert.ToBoolean(field.IsRequired)
										  ? thisFieldType.ErrorMsgRequired.Replace(
												"%FormFieldName%", field.FormFieldName)
										  : string.Empty,
								CommaDelimitedChoices = GetOptionsForSelector(field),
								Orientation = (field.Orientation == "vertical") ? Orientation.Vertical : Orientation.Horizontal
							};
							dynamicFormFields.Add(newRadioList);
							break;

					}
				}
			}
			var dynamicForm = new global::STPC.DynamicForms.Core.Form();
			dynamicForm.AddFields(dynamicFormFields.ToArray());
			dynamicForm.Serialize = true;
			return PartialView("FieldsRequest", dynamicForm);

			//return PartialView("FieldsRequest", listPageFields);
		}

		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult LoadFieldsByForm(FormCollection par)
		{


			Dictionary<string, string> dicRequest = new Dictionary<string, string>();

			dicRequest.Add(FieldNameDynamicQuery.REQUEST_TYPE.ToString(), "1");
			dicRequest.Add(FieldNameDynamicQuery.DOCUMENT_NUMBER.ToString(), "1");
			dicRequest.Add(FieldNameDynamicQuery.REQUEST_CREATED_DATE.ToString(), "1");
			dicRequest.Add(FieldNameDynamicQuery.REQUEST_HIERARCHIES.ToString(), "1");
			dicRequest.Add(FieldNameDynamicQuery.REQUEST_NUMBER.ToString(), "1");
			dicRequest.Add(FieldNameDynamicQuery.REQUEST_STATE.ToString(), "1");

			List<STPC.DynamicForms.Web.RT.Services.Request.Request> requestList = new List<STPC.DynamicForms.Web.RT.Services.Request.Request>();

			requestList = _RequestServiceClient.GetRequest(dicRequest).ToList();

			return PartialView("_QuerySolicitude", requestList);
		}


		private string GetOptionsForSelector(PageField field, bool isParent = false)
		{
			if (field == null) throw new ArgumentException("Objeto field nulo");
			if (field.OptionsMode == "local")
			{
				var TheCategory = _stpcForms.Categories.Where(n => n.Uid == Convert.ToInt32(field.OptionsCategoryName)).Single();

				List<STPC.DynamicForms.Web.RT.Services.Entities.Option> TheOptions;
				if (isParent) TheOptions = _stpcForms.Options.Where(c => c.Category_Uid == TheCategory.Uid && c.Option_Uid_Parent == 1).OrderBy(e => e.Value).ToList();
				else TheOptions = _stpcForms.Options.Where(c => c.Category_Uid == TheCategory.Uid).OrderBy(e => e.Value).ToList();

				StringBuilder TheOptionsCommaSeparated = new StringBuilder();
				foreach (var option in TheOptions)
				{
					TheOptionsCommaSeparated.Append(option.Uid + "|" + option.Value);
					TheOptionsCommaSeparated.Append(',');
				}
				if (TheOptionsCommaSeparated.Length > 0)
					TheOptionsCommaSeparated.Remove(TheOptionsCommaSeparated.Length - 1, 1);
				return TheOptionsCommaSeparated.ToString();
			}
			if (field.OptionsMode == "ws")
			{
				//TODO: Url Web Services 
				return "Web Service";
			}
			//if (field.OptionsMode == "strat")
			//{
			//	int strategyId;
			//	if (!int.TryParse(field.OptionsWebServiceUrl, out strategyId)) throw new ArgumentException("Strategy Id invalido para el campo" + field.Uid, "OptionsWebServiceUrl");
			//	var options = _decisionEngine.GetList(strategyId, "requestId", value.ToString(), "lista");
			//	//TODO: Refactor codigo repetido con el if anterior
			//	StringBuilder TheOptionsCommaSeparated = new StringBuilder();
			//	foreach (var option in options)
			//	{
			//		TheOptionsCommaSeparated.Append(option);
			//		TheOptionsCommaSeparated.Append(',');
			//	}
			//	if (TheOptionsCommaSeparated.Length > 0)
			//		TheOptionsCommaSeparated.Remove(TheOptionsCommaSeparated.Length - 1, 1);
			//	return TheOptionsCommaSeparated.ToString();
			//}
			if (string.IsNullOrEmpty(field.Options)) return string.Empty;
			return field.Options.Replace(Environment.NewLine, ",");
		}

		public JsonResult GetchildrenCountAndLevelName(int id)
		{

			string srHierarchies = id + getParentName(id);
			var TheHierarchy = _stpcForms.Hierarchies.Expand("Children").Where(i => i.Id == id).FirstOrDefault();
			nodeString = string.Empty;
			return Json(srHierarchies, JsonRequestBehavior.AllowGet);

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

		[HttpPost]
		public ActionResult IndexConsulta(
		  string srNumRequest, string srNode,
		  string requesUpdateDate, string requesCreateDate,
		  string requesCreatedBy, string requesUpdatedBy,
		  string par, string srRequestType, string requesUpdateDateEnd,
		  string requesCreateDateEnd, string srRequestName, string uidFormState

		  )
		{
			try
			{
				string[] parts = par.Split('&');

				Dictionary<string, string> dicRequest = new Dictionary<string, string>();
				string srNameField;
				string srValCampo;
				PageField _PageField = new PageField();
				string srNamePage = string.Empty;
				List<ReportParameters> listParameters = new List<ReportParameters>();

				//Recorre los campos del htmlForm
				for (int i = 0; i < parts.Length; i++)
				{

					string[] FieldsValue = parts[i].Split('=');

					if (FieldsValue[0].Contains("STPC_DFi_"))
					{
						srNameField = FieldsValue[0].Substring(9);

						//Consulta el campo y carga el panel al cual pertenece
						var fields = _stpcForms.PageFields.Expand("FormFieldType").Expand("Panel").Where(xp => xp.Uid == Guid.Parse(srNameField)).FirstOrDefault();

						//Con base en el panel, consulta la pagina a la cual pertenece el cada uno de los campos.
						var thisPanel = _stpcForms2.Panels.Expand("PanelFields").Expand("Page").Where((x => x.Uid == fields.Panel.Uid)).FirstOrDefault();
						srNamePage = thisPanel.Page.Name;

						if (!string.IsNullOrEmpty(FieldsValue[1]))
						{

							listParameters.Add(new ReportParameters()
							{
								NameField = fields.FormFieldName,
								NamePage = srNamePage,
								CaptionField = fields.FormFieldPrompt,
								value = FieldsValue[1].Replace("%2F", "/").Trim()
							}
							);

							srValCampo = FieldsValue[1];
						}
					}
				}
				StringBuilder strCreationDate = new StringBuilder();
				StringBuilder strUpdateDate = new StringBuilder();
				StringBuilder strCreationDateEnd = new StringBuilder();
				StringBuilder strUpdateDateEnd = new StringBuilder();
				DateTime date;

				if (!string.IsNullOrEmpty(requesCreateDate))
				{
					date = DateTime.ParseExact(requesCreateDate, "MM/dd/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
					strCreationDate.AppendFormat("{0:yyyy/MM/dd}", date);
				}

				if (!string.IsNullOrEmpty(requesUpdateDate))
				{
					date = DateTime.ParseExact(requesUpdateDate, "MM/dd/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
					strUpdateDate.AppendFormat("{0:yyyy/MM/dd}", date);

				}
				if (!string.IsNullOrEmpty(requesCreateDateEnd))
				{
					date = DateTime.ParseExact(requesCreateDateEnd, "MM/dd/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
					strCreationDateEnd.AppendFormat("{0:yyyy/MM/dd}", date.AddHours(24));
				}

				if (!string.IsNullOrEmpty(requesUpdateDateEnd))
				{
					date = DateTime.ParseExact(requesUpdateDateEnd, "MM/dd/yyyy", System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat);
					strUpdateDateEnd.AppendFormat("{0:yyyy/MM/dd}", date.AddHours(24));

				}

				dicRequest.Add(FieldNameDynamicQuery.REQUEST_CREATED_DATE, strCreationDate.ToString());
				dicRequest.Add(FieldNameDynamicQuery.REQUEST_UPDATED_DATE, strUpdateDate.ToString());
				dicRequest.Add(FieldNameDynamicQuery.REQUEST_CREATED_DATE_END, (strCreationDateEnd.ToString() == string.Empty ? strCreationDate.ToString() : strCreationDateEnd.ToString()));
				dicRequest.Add(FieldNameDynamicQuery.REQUEST_UPDATED_DATE_END, (strUpdateDateEnd.ToString() == string.Empty ? strUpdateDate.ToString() : strUpdateDateEnd.ToString()));
				dicRequest.Add(FieldNameDynamicQuery.REQUEST_NUMBER, srNumRequest);
				dicRequest.Add(FieldNameDynamicQuery.REQUEST_CREATEDBY, requesCreatedBy);
				dicRequest.Add(FieldNameDynamicQuery.REQUEST_UPDATEDBY, requesUpdatedBy);
				dicRequest.Add(FieldNameDynamicQuery.REQUEST_TYPE.ToString(), srRequestType);
				dicRequest.Add(FieldNameDynamicQuery.REQUEST_WORK_FLOWS_STATE, uidFormState.ToString());
				dicRequest.Add(FieldNameDynamicQuery.REQUEST_USERNAME, this.User.Identity.Name);

				//Asigna el discionario a la lista de parametros estaticos
				listHierarchy = _stpcForms.Hierarchies.Expand("Children").ToList();
				listHierarchylistTemp = new List<string>();


				if (!srNode.Equals(""))
				{
					GetHierarchyChild(Convert.ToInt32(srNode));
					if (listHierarchylistTemp.Count > 0)
					{
						string commaSeparatedList = commaSeparatedList = listHierarchylistTemp.Aggregate((a, x) => a + ", " + x);
						srNode = srNode + ", " + commaSeparatedList;
					}
				}
				ViewData["ShowPrintButton"] = "1";
				dicRequest.Add(FieldNameDynamicQuery.REQUEST_HIERARCHIES.ToString(), srNode);
				return PartialView("_QuerySolicitude", _RequestServiceClient.GetRequestDynamicSearcher(listParameters, dicRequest, srRequestName.Replace(" ", "").ToString()));
			}
			catch (Exception ex)
			{
				return DefaultActionErrorHandling(ex, "IndexConsulta");
			}
		}

		private void GetHierarchyChild(int idHierarchy)
		{
			Hierarchy childHierarchy = new Hierarchy();


			childHierarchy = listHierarchy.Where(e => e.Id == idHierarchy).FirstOrDefault();

			foreach (var item in childHierarchy.Children)
			{
				listHierarchylistTemp.Add(item.Id.ToString());
				GetHierarchyChild(item.Id);
			}

		}


		public bool IsValueType<T>()
		{
			return typeof(T).IsValueType;
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

		public ActionResult MyRequest()
		{
			Dictionary<string, string> values = new Dictionary<string, string>();
			values.Add(Constants.CREATE_BY, this.User.Identity.Name);
			values.Add(FieldNameDynamicQuery.REQUEST_CREATED_DATE.ToString(), string.Empty);
			values.Add(FieldNameDynamicQuery.REQUEST_UPDATED_DATE.ToString(), string.Empty);
			values.Add(FieldNameDynamicQuery.REQUEST_HIERARCHIES.ToString(), string.Empty);
			ViewData["ShowPrintButton"] = "0";
			//var request = _RequestServiceClient.GetRequest(values).ToList();
			var request = _RequestServiceClient.GetRequestDinamic(new List<ReportParameters>(), values, string.Empty);
			return View("_QuerySolicitude", request);
		}

		public ActionResult RequestByState(string GuidState)
		{
			//Dictionary<string, string> values = new Dictionary<string, string>();
			////values.Add(Constants.CREATE_BY, this.User.Identity.Name);
			//values.Add(FielNameDynamicQuery.REQUEST_CREATED_DATE.ToString(), string.Empty);
			//values.Add(FielNameDynamicQuery.REQUEST_UPDATED_DATE.ToString(), string.Empty);
			//values.Add(FielNameDynamicQuery.REQUEST_HIERARCHIES.ToString(), string.Empty);
			//values.Add(FielNameDynamicQuery.REQUEST_WORK_FLOWS_STATE.ToString(), GuidState);
			////var request = _RequestServiceClient.GetRequest(values).ToList();
			//var request = _RequestServiceClient.GetRequestDinamic(new List<ReportParameters>(), values, string.Empty);
			//return View("_QuerySolicitude", request);

			string storeProcedureName = System.Configuration.ConfigurationManager.AppSettings["RequestsVerifyingProcedureName"];

			// usar el servicio para consultar el store procedure
			var request = _RequestServiceClient.GetRequestByProcedure(storeProcedureName);
			return View("_QuerySolicitude", request);


		}

		//TODO: Modify to pass the parameters 
		public ActionResult RequestByProcedure(string sp)
		{
			//string storeProcedureName = System.Configuration.ConfigurationManager.AppSettings["InterviewsProcedureName"];
			var theUser = new CustomMembershipProvider().GetUser(this.User.Identity.Name);

			// usar el servicio para consultar el store procedure
			var request = _RequestServiceClient.GetRequestByProcedure(sp);
			return View("_QuerySolicitude", request);
		}

		[HttpPost]
		[ValidateAntiForgeryTokenAttribute]
		public ActionResult RequestsByParamProcedurePost(Guid itemId, string parameters, int source)
		{
			try
			{
				//string storeProcedureName = System.Configuration.ConfigurationManager.AppSettings["InterviewsProcedureName"];
				var theUser = new CustomMembershipProvider().GetUser(this.User.Identity.Name);
				int fetch = 20;

				if (System.Web.Configuration.WebConfigurationManager.AppSettings["RecordCountByPage"] != null)
				{
					fetch = Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["RecordCountByPage"].ToString());
				}



				// usar el servicio para consultar el store procedure
				var request = _RequestServiceClient.GetRequestsByParamProcedure(parameters, theUser.Username, 0, fetch, "");


				int totlaCount = 0;
				if (request.Rows.Count > 0)
				{
					foreach (var item in request.Rows[0].Values)
					{
						if (item.ColumnName == "TotalCount")
						{
							totlaCount = Convert.ToInt32(item.Value);
						}
					}
				}



				ViewBag.nameSp = parameters;
				ViewBag.PageCount = totlaCount / fetch;
				ViewBag.fetchCount = fetch.ToString();


				return PartialView("_QuerySolicitude", request);
			}

			catch (Exception ex)
			{
				return DefaultActionErrorHandling(ex, "RequestsByParamProcedurePost");
			}
		}


		public ActionResult RequestsByParamProcedure(Guid itemId, string parameters, int source)
		{
			try
			{
				//string storeProcedureName = System.Configuration.ConfigurationManager.AppSettings["InterviewsProcedureName"];
				var theUser = new CustomMembershipProvider().GetUser(this.User.Identity.Name);
				int fetch = 20;

				if (System.Web.Configuration.WebConfigurationManager.AppSettings["RecordCountByPage"] != null)
				{
					fetch = Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["RecordCountByPage"].ToString());
				}



				// usar el servicio para consultar el store procedure
				var request = _RequestServiceClient.GetRequestsByParamProcedure(parameters, theUser.Username, 0, fetch, "");


				int totlaCount = 0;
				if (request.Rows.Count > 0)
				{
					foreach (var item in request.Rows[0].Values)
					{
						if (item.ColumnName == "TotalCount")
						{
							totlaCount = Convert.ToInt32(item.Value);
						}
					}
				}



				ViewBag.nameSp = parameters;
				ViewBag.PageCount = totlaCount / fetch;
				ViewBag.fetchCount = fetch.ToString();


				return View("_QuerySolicitude", request);
			}

			catch (Exception ex)
			{
				return DefaultActionErrorHandling(ex, "RequestsByParamProcedure");
			}
		}


		[HttpPost]
		public ActionResult RequestsByParamProcedurePaged(string parameters, int offSet, string filter)
		{


			//string storeProcedureName = System.Configuration.ConfigurationManager.AppSettings["InterviewsProcedureName"];
			var theUser = new CustomMembershipProvider().GetUser(this.User.Identity.Name);

			int fetch = 20;
			//Valida en el config si está configurado el total de registros por página			
			if (System.Web.Configuration.WebConfigurationManager.AppSettings["RecordCountByPage"] != null)
			{
				fetch = Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["RecordCountByPage"].ToString());
			}

			ViewBag.fetchCount = fetch.ToString();
			// usar el servicio para consultar el store procedure
			var request = _RequestServiceClient.GetRequestsByParamProcedure(parameters, theUser.Username, offSet, fetch, filter);



			int totlaCount = 0;
			if (request.Rows.Count > 0)
			{
				foreach (var item in request.Rows[0].Values)
				{
					if (item.ColumnName == "TotalCount")
					{
						totlaCount = Convert.ToInt32(item.Value);
					}
				}
			}

			ViewBag.nameSp = parameters;
			ViewBag.PageCount = (totlaCount / fetch).ToString();
			ViewBag.fetchCount = fetch.ToString();

			return PartialView("_Paged_list", request);
		}

		public JsonResult GetBlob(string blobName)
		{
			BlobService blob = new BlobService();
			var result = blob.GetBlob(blobName);

			return Json(result, JsonRequestBehavior.AllowGet);

		}


		[HttpPost]
		public ActionResult GetReport(string formUid, int requestId)
		{
			string parameters = getUrlReportByForm(formUid);
			string userName = User.Identity.Name;
			if (!parameters.Equals(""))
			{
				ViewData["userName"] = userName;
				ViewData["parameters"] = parameters;
				ViewData["requestId"] = requestId;
			}
			else
			{
				HttpContext.Response.Write("No se ha configurado la ruta del reporte");
			}
			return PartialView("ReportView");
		}

		private string getUrlReportByForm(string formUid)
		{
			STPC.DynamicForms.Web.RT.Services.Entities.Report report = new Services.Entities.Report();

			report = db.Reports.Where(e => e.Form.Uid == Guid.Parse(formUid)).FirstOrDefault();

			if (report != null)
			{
				return report.ReportPath;
			}
			else
				return string.Empty;

		}

		private void DefaultErrorHandling(Exception ex, string triggerAction)
		{
			bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
			Guid correlationID = Guid.NewGuid();

			ILogging eventWriter = LoggingFactory.GetInstance();
			string errorMessage = string.Format(CustomMessages.E0007, "RequestController", triggerAction, correlationID, ex.Message);
			System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
			eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

		}

		private ActionResult DefaultActionErrorHandling(Exception ex, string triggerAction)
		{
			bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
			Guid correlationID = Guid.NewGuid();

			ILogging eventWriter = LoggingFactory.GetInstance();
			string errorMessage = string.Format(CustomMessages.E0007, "RequestController", triggerAction, correlationID, ex.Message);
			System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
			eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
			if (ShowErrorDetail)
			{
				return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(errorMessage), "RequestController", triggerAction));
			}
			else
			{
				return PartialView("_ErrorGeneral", string.Format(CustomMessages.E0001, correlationID.ToString()));
			}
		}


	}
}
