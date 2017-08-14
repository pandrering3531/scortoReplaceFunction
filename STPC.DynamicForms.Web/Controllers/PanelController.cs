using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using STPC.DynamicForms.Web.Helpers;
using STPC.DynamicForms.Web.Models;
using STPC.DynamicForms.Web.Services.Entities;
using STPC.DynamicForms.Core.Fields;
using System.Text;
using STPC.DynamicForms.DecisionEngine;
using System.Configuration;
using STPC.DynamicForms.Web.Common;

namespace STPC.DynamicForms.Web.Controllers
{

    [Authorize]
    public class PanelController : Controller
    {
        #region Constants & Attributes

        private IDecisionEngine _decisionEngine;
        private string _ROLE_SEPARATOR_CHARACTER_ = ",";

        //TODO: Sacar la URI del servicio al web.config
        Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
        CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;

        #region Constantes tipo control

        // tipos de control
        public const string _TIPO_TEXTO = "Texto";
        public const string _TIPO_NUMERO = "Numero";
        public const string _TIPO_FECHA = "Fecha";
        public const string _TIPO_CORREO = "Correo Electronico";
        public const string _TIPO_LISTA_UNICA = "Lista de opciones unicas";
        public const string _TIPO_LISTA_MULTIPLE = "Lista de opciones multiples";
        public const string _TIPO_TEXTAREA = "Area de texto";
        public const string _TIPO_LISTA_SELECT = "Lista de seleccion";
        public const string _TIPO_CHECK = "Caja de chequeo";
        public const string _TIPO_FILE = "Subir Archivo";
        public const string _TIPO_LITERAL = "Texto literal";
        public const string _TIPO_LINK = "Hipervinculo";
        public const string _TIPO_MONEDA = "Moneda";
        public const string _TIPO_DECIMAL = "Decimal";
        public const string _TIPO_SOLO_TEXTO = "Solo Texto";


        // tamaño maximo por control
        public string _MAX_TEXTO = "512";
        public string _MAX_NUMERO = (((double)(int.MaxValue)) * 2).ToString();
        //public string _MAX_NUMERO = int.MaxValue.ToString();
        public string _MAX_FECHA = DateTime.MaxValue.ToString();
        public string _MAX_CORREO = "512";
        public string _MAX_LISTA_UNICA = "512";
        public string _MAX_LISTA_MULTIPLE = "512";
        public string _MAX_TEXTAREA = "4000";
        public string _MAX_LISTA_SELECT = "512";
        public string _MAX_CHECK = "1";
        public string _MAX_FILE = "2048"; // valor en KBytes
        public string _MAX_LITERAL = "512";
        public string _MAX_LINK = "512";
        public string _MAX_MONEDA = int.MaxValue.ToString();
        public string _MAX_DECIMAL = decimal.MaxValue.ToString();
        public string _MAX_SOLO_TEXTO = "512";


        #endregion Constantes tipo control

        #endregion Constants & Attributes

        #region Constructor

        public PanelController(IDecisionEngine decisionEngine)
        {
            _decisionEngine = decisionEngine;

        }

        public PanelController()
        {
            string user = System.Configuration.ConfigurationManager.AppSettings["UserWs"];
            string pws = System.Configuration.ConfigurationManager.AppSettings["PwsWs"];
            string pollInterval = System.Configuration.ConfigurationManager.AppSettings["pollInterval"];
            string timeOut = System.Configuration.ConfigurationManager.AppSettings["timeOut"];
            STPC.DynamicForms.DecisionEngine.DecisionEngine iEngine = new STPC.DynamicForms.DecisionEngine.DecisionEngine(user, pws, Convert.ToInt32(pollInterval), Convert.ToInt32(timeOut));
            this._decisionEngine = iEngine;

        }

        #endregion

        #region Private Methods

   
        private string GetOptionsForSelector(PageField field, int value, bool isParent)
        {
            if (field == null) throw new ArgumentException("Objeto field nulo");
            if (field.OptionsMode == "local")
            {
                var TheCategory = _stpcForms.Categories.Where(n => n.Uid == Convert.ToInt32(field.OptionsCategoryName)).Single();

                List<Option> TheOptions;
                if (isParent) TheOptions = _stpcForms.Options.Where(c => c.Category_Uid == TheCategory.Uid && c.Option_Uid_Parent == value).OrderBy(e => e.Value).ToList();
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
            if (field.OptionsMode == "strat")
            {
                int strategyId;
                if (!int.TryParse(field.OptionsWebServiceUrl, out strategyId)) throw new ArgumentException("Strategy Id invalido para el campo" + field.Uid, "OptionsWebServiceUrl");
                var options = _decisionEngine.GetList(strategyId, "requestId", value.ToString(), "lista", this.User.Identity.Name.ToString());
                //TODO: Refactor codigo repetido con el if anterior
                StringBuilder TheOptionsCommaSeparated = new StringBuilder();
                foreach (var option in options)
                {
                    TheOptionsCommaSeparated.Append(option);
                    TheOptionsCommaSeparated.Append(',');
                }
                if (TheOptionsCommaSeparated.Length > 0)
                    TheOptionsCommaSeparated.Remove(TheOptionsCommaSeparated.Length - 1, 1);
                return TheOptionsCommaSeparated.ToString();
            }
            if (string.IsNullOrEmpty(field.Options)) return string.Empty;
            return field.Options.Replace(Environment.NewLine, ",");
        }
        private string GetOptionsForSelector(PageField field)
        {
            var TheCategory = _stpcForms.Categories.Where(n => n.Uid == Convert.ToInt32(field.OptionsCategoryName)).Single();
            var optionList = _stpcForms.Options.Where(c => c.Category_Uid == TheCategory.Uid).ToList();
            return GetOptionsForSelector(field, 200, false); //What is the Value? 200?

        }

  
        private void DeleteFormFieldsNotInThePost(FormViewModel viewModel)
        {
            //TODO : RG Change logic to scan hidden fields in listfields
            if (!string.IsNullOrEmpty(Request["ListFields"]))
            {
                var listFields = Request["ListFields"].Split(',');
                foreach (var listField in listFields)
                {
                    var listFieldUid = new Guid(listField);
                    if (viewModel.formfields.Where(fields => fields.Uid == listFieldUid).Count() == 0)
                    {
                        var obj = _stpcForms.PageFields.Where(field => field.Uid == listFieldUid).FirstOrDefault();
                        _stpcForms.DeleteObject(obj);
                    }
                }
            }
        }

        protected List<SelectListItem> GetFormFieldTypes(bool includeEmptyChoice = false)
        {
            var formFieldTypeList = new List<SelectListItem>();
            if (includeEmptyChoice)
                formFieldTypeList.Add(new SelectListItem { Selected = true, Text = "--Select a Field Type--", Value = string.Empty });
            //Retain the foreach since a LINQ statement bombs on the .ToString for Uid
            foreach (var formFieldType in _stpcForms.PageFieldTypes.OrderBy(formfield => formfield.FieldTypeName))
            {
                formFieldTypeList.Add(new SelectListItem { Text = formFieldType.FieldTypeName, Value = formFieldType.Uid.ToString() });
            }
            return formFieldTypeList;
        }

        /// <summary>
        /// Obtener el valor maximo de acuerdo al tipo de control
        /// Número y Moneda: Cantidad máxima
        /// Texto, Hyperlink, email, área de texto, texto literal: Cantidad máxima de carácteres.
        /// FileUpload: Tamaño máximo en KBytes.
        /// </summary>
        /// <returns></returns>
        private string GetMaxSize(PageFieldType iFieldType)
        {
            string result = null;

            switch (iFieldType.FieldTypeName)
            {
                case _TIPO_CHECK:
                    result = _MAX_CHECK;
                    break;

                case _TIPO_CORREO:
                    result = _MAX_CORREO;
                    break;

                case _TIPO_DECIMAL:
                    result = _MAX_DECIMAL;
                    break;

                case _TIPO_FECHA:
                    result = _MAX_FECHA;
                    break;

                case _TIPO_FILE:
                    result = _MAX_FILE;
                    break;

                case _TIPO_LINK:
                    result = _MAX_LINK;
                    break;

                case _TIPO_LISTA_MULTIPLE:
                    result = _MAX_LISTA_MULTIPLE;
                    break;

                case _TIPO_LISTA_SELECT:
                    result = _MAX_LISTA_SELECT;
                    break;

                case _TIPO_LISTA_UNICA:
                    result = _MAX_LISTA_UNICA;
                    break;

                case _TIPO_LITERAL:
                    result = _MAX_TEXTAREA;
                    // se modifica esta linea para que el literal 
                    // soporte hasta el maximo de caracteres 4000
                    //result = _MAX_LITERAL;
                    break;

                case _TIPO_MONEDA:
                    result = _MAX_MONEDA;
                    break;

                case _TIPO_NUMERO:
                    result = _MAX_NUMERO;
                    break;

                case _TIPO_TEXTAREA:
                    result = _MAX_TEXTAREA;
                    break;

                case _TIPO_TEXTO:
                    result = _MAX_TEXTO;
                    break;

                case _TIPO_SOLO_TEXTO:
                    result = _MAX_SOLO_TEXTO;
                    break;
            }

            return result;
        }

        #endregion Private Methods

        public PartialViewResult NewFormField(string formId, Guid selectedFieldType, Guid panelUid)
        {
            var fieldType = _stpcForms.PageFieldTypes.Where(type => type.Uid == selectedFieldType).FirstOrDefault();
            ViewBag.CategoriesSelect = new List<SelectListItem>();

            List<SelectListItem> StrategiesSelect = new List<SelectListItem>();

            StrategiesSelect.Add(
              new SelectListItem
              {
                  Selected = true,
                  Text = "---Seleccione estrategia---",
                  Value = "0"
              }
              );

            foreach (var category in _stpcForms.Categories)
                ViewBag.CategoriesSelect.Add(new SelectListItem { Text = category.Name, Value = category.Uid.ToString() });

            ViewBag.StrategiesSelect = new List<SelectListItem>();
            foreach (var strategy in _decisionEngine.GetStrategyList())
                StrategiesSelect.Add(new SelectListItem { Text = strategy.Name, Value = strategy.Id.ToString() });

            ViewBag.StrategiesSelect = StrategiesSelect;


            var viewModel = new FormFieldViewModel
            {
                Uid = Guid.NewGuid(),
                ShowDelete = false, // se modifica el valor por defecto a false, para que la primera vez no lo muestre
                FormFieldTypes = GetFormFieldTypes(),
                SelectedFormFieldType = selectedFieldType.ToString(),
                //TODO: Not sure if this is per field type, but it shouldn't matter 
                //Create a default radiobutton selection
                Orientation = "vertical",
                AvailableRoles = Roles.GetAllRoles(),
                PanelUid = panelUid,
                SelectedViewRoles = new List<string>(),
                SelectedEditRoles = new List<string>(),
                MaxSizeBD = GetMaxSize(fieldType),
                ControlTypeName = fieldType.FieldTypeName,
                ControlType = fieldType.ControlType,
            };
            ViewBag.DefaultFieldStyle = System.Configuration.ConfigurationManager.AppSettings["DefaultFieldStyle"];
            #region procesar los roles

            // procesar los roles
            foreach (string _role_ in viewModel.AvailableRoles)
            {
                #region adicionar los view roles

                viewModel.SelectedViewRoles.Add(_role_ + _ROLE_SEPARATOR_CHARACTER_ + false.ToString());

                #endregion adicionar los view roles

                #region adicionar los edit roles

                viewModel.SelectedEditRoles.Add(_role_ + _ROLE_SEPARATOR_CHARACTER_ + false.ToString());

                #endregion adicionar los edit roles
            }

            #endregion procesar los roles
            viewModel.Style = System.Configuration.ConfigurationManager.AppSettings["DefaultFieldStyle"];
            return PartialView("_" + fieldType.ControlType, viewModel);// { UpdateValidationForFormId = formId };
        }

        public ActionResult DeleteFormField(Guid formfieldUid, Guid panelUid)
        {

            try
            {
                var field = _stpcForms.PageFields.Where(pf => pf.Uid == formfieldUid).FirstOrDefault();

                if (field != null)
                {
                    _stpcForms.DeleteObject(field);
                    _stpcForms.SaveChanges();
                }

                return RedirectToAction("Edit", new { id = panelUid, colunmNumber = field.PanelColumn });
            }
            catch (Exception Ex)
            {
                string error = Ex.Message;

                return RedirectToAction("Edit", new { id = panelUid, colunmNumber = 0 });
            }

        }

        public ActionResult Edit(Guid id, int colunmNumber)
        {
            var item = _stpcForms.Panels.Expand("PanelFields").Expand("Page").Where((x => x.Uid == id)).FirstOrDefault();

            ViewBag.CategoriesSelect = new List<SelectListItem>();

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

            foreach (var category in _stpcForms.Categories)
                ViewBag.CategoriesSelect.Add(new SelectListItem { Text = category.Name, Value = category.Uid.ToString() });
            ViewBag.StrategiesSelect = new List<SelectListItem>();
            foreach (var strategy in _decisionEngine.GetStrategyList().OrderBy(str => str.Name))
                StrategiesSelect.Add(new SelectListItem { Text = strategy.Name, Value = strategy.Id.ToString() });

            ViewBag.StrategiesSelect = StrategiesSelect;

            #endregion cargar la lista de estrategias

            // adiciona el numero de columna seleccionado
            ViewBag.colunmNumber = colunmNumber;

            var viewModel = new FormViewModel();
            viewModel.panel = item;

            // almacena el  panel Uid en el viewbag
            ViewBag.thisPanelUid = item.Uid;

            var formFields = from items in item.PanelFields
                             where items.IsHidden == false
                             && items.PanelColumn == colunmNumber
                             orderby items.SortOrder
                             select items;

            if (formFields.Count() > 0)
            {
                var countFields = 1;
                var listFields = String.Empty;
                foreach (var formField in formFields)
                {
                    var thisFieldType = _stpcForms.PageFieldTypes.Where(formft => formft.Uid == formField.FormFieldType_Uid).First();

                    #region cargar datos del campo

                    var editFormField = new FormFieldViewModel
                    {
                        Uid = formField.Uid,
                        FormFieldName = formField.FormFieldName,
                        FormFieldPrompt = formField.FormFieldPrompt,
                        ControlType = thisFieldType.ControlType,
                        ControlTypeName = thisFieldType.FieldTypeName,
                        SelectedFormFieldType = thisFieldType.Uid.ToString(),
                        IsRequired = Convert.ToBoolean(formField.IsRequired),
                        ShowDelete = formField.ShowDelete,
                        MinSize = formField.MinSize,
                        FormFieldTypes = GetFormFieldTypes(),
                        //TODO: Not sure if this is per field type, but it shouldn't matter 
                        Options = formField.Options,
                        Orientation = formField.Orientation,
                        IsMultipleSelect = Convert.ToBoolean(formField.IsMultipleSelect),
                        ListSize = formField.ListSize,
                        IsEmptyOption = Convert.ToBoolean(formField.IsEmptyOption),
                        EmptyOption = formField.EmptyOption,
                        Rows = formField.Rows,
                        Cols = formField.Cols,
                        ValidExtensions = formField.ValidExtensions,
                        ErrorExtensions = formField.ErrorExtensions,
                        MaxSize = formField.MaxSize,
                        MaxSizeBD = formField.MaxSizeBD,
                        LiteralText = formField.LiteralText,
                        OptionsMode = formField.OptionsMode,
                        OptionsCategoryName = formField.OptionsCategoryName,
                        OptionsWebServiceUrl = formField.OptionsCategoryName == "ws" ? formField.OptionsWebServiceUrl : "",
                        AvailableRoles = Roles.GetAllRoles(),
                        PanelUid = formField.PanelUid,
                        ValidationStrategyId = formField.ValidationStrategyID.ToString(),
                        ViewRoles = formField.ViewRoles != null ? formField.ViewRoles : string.Empty,
                        EditRoles = formField.EditRoles != null ? formField.EditRoles : string.Empty,
                        SelectedEditRoles = new List<string>(),
                        SelectedViewRoles = new List<string>(),
                        Style = formField.Style,
                        ToolTip = formField.ToolTip,
                        Queryable = formField.Queryable
                    };


                    // load Option Strategy
                    ViewData["OptionStrategyValue" + formField.Uid.ToString()] = formField.OptionsWebServiceUrl;

                    #region procesar los roles

                    // procesar los roles
                    foreach (string _role_ in editFormField.AvailableRoles)
                    {
                        #region adicionar los view roles

                        if (editFormField.ViewRoles.Contains(_role_))
                            editFormField.SelectedViewRoles.Add(_role_ + _ROLE_SEPARATOR_CHARACTER_ + true.ToString());

                        else
                            editFormField.SelectedViewRoles.Add(_role_ + _ROLE_SEPARATOR_CHARACTER_ + false.ToString());

                        #endregion adicionar los view roles

                        #region adicionar los edit roles

                        if (editFormField.EditRoles.Contains(_role_))
                            editFormField.SelectedEditRoles.Add(_role_ + _ROLE_SEPARATOR_CHARACTER_ + true.ToString());

                        else
                            editFormField.SelectedEditRoles.Add(_role_ + _ROLE_SEPARATOR_CHARACTER_ + false.ToString());

                        #endregion adicionar los edit roles
                    }

                    #endregion procesar los roles

                    viewModel.formfields.Add(editFormField);
                    listFields += "," + formField.Uid;
                    countFields++;

                    #endregion cargar datos del campo
                }
                ViewBag.ListFields = listFields.Substring(1); //Starts at 0; remove the first ','
            }
            else
            {
                var newField = new FormFieldViewModel { FormFieldTypes = GetFormFieldTypes() };
                viewModel.formfields.Add(newField);
            }
            ViewBag.DefaultFieldStyle = System.Configuration.ConfigurationManager.AppSettings["DefaultFieldStyle"];
            return View(viewModel);
        }

        public ActionResult Respond(Guid id)
        {
            try
            {
                var thisPanel = _stpcForms.Panels.Expand("PanelFields").Expand("Page").Where((x => x.Uid == id)).FirstOrDefault();
                var formFields = new List<PageField>();
                var indexPanel = thisPanel.SortOrder;

                foreach (var field in thisPanel.PanelFields.Where(f => !string.IsNullOrEmpty(f.ViewRoles)).OrderBy(f => f.PanelColumnSortOrder).ThenBy(f => f.PanelColumn))
                {
                    field.FormFieldType = _stpcForms.PageFieldTypes.Where(u => u.Uid == field.FormFieldType_Uid).FirstOrDefault();
                    formFields.Add(field);
                }

                //TODO: Consider switching to conditionally build views using partial views with the KnockOutJS library for custom validation
                var currentUser = provider.GetUser(User.Identity.Name);

                #region Validar si el panel contiene campos

                //Busca el appSetting que indique el nombre para el tipo de campo numerico y poder validar si un campos es de tipo numerico y agregarle

                // carga el tipo un atributo para que solo acepte valores numericos
                string FieldTypeNumericName = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameNumeric"]) ? System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameNumeric"].ToString() : string.Empty;
                string FieldTypeCurrency = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameCurrency"]) ? System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameCurrency"].ToString() : string.Empty;
                string _dateFormat = System.Configuration.ConfigurationManager.AppSettings["FormatoFecha"];

                if (formFields.Count() > 0)
                {
                    var dynamicFormFields = new List<Field>();
                    string value = string.Empty;
                    int columnOrder = 1;
                    int newIndexPanel = 0;

                    foreach (var formField in formFields)
                    {

                        var field = formField;
                        var thisFieldType = field.FormFieldType;

                        #region validar si contiene rol de visualizacion

                        // validar si contiene rol de visualizacion
                        if (!string.IsNullOrEmpty(field.ViewRoles))
                        {
                            if (field.PanelColumnSortOrder <= 9)
                                newIndexPanel = indexPanel * 100;
                            else
                                newIndexPanel = indexPanel * 10;

                            #region validar si tiene un trigger asociado

                            // consultar por el GUID de la pagina y el GUID del campo si es un trigger de estrategias
                            var StrategyList = _stpcForms.PageStrategy.Where(pag => pag.PageId == thisPanel.Page.Uid && pag.TriggerFieldUid == field.Uid);
                            bool isTrigger = StrategyList.Count() > 0 ? true : false;

                            #endregion validar si tiene un trigger asociado

                            #region validar si tiene un campo respuesta asociado

                            // consultar por el GUID de la pagina y el GUID del campo si es un campo de respuesta
                            var responseList = _stpcForms.PageStrategy.Where(pag => pag.PageId == thisPanel.Page.Uid && pag.ResponseFieldUid == field.Uid);
                            bool isResponse = responseList.Count() > 0 ? true : false;

                            #endregion validar si tiene un campo respuesta asociado

                            // se agrupan los tipos de control para facilitar la lectura del codigo
                            #region tipos de control

                            switch (thisFieldType.FieldType)
                            {
                                #region Literal

                                case ("Literal"):
                                    dynamicFormFields.Add(new Literal()
                                    {
                                        //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),

                                        Index = newIndexPanel.ToString() + columnOrder.ToString(),
                                        IsTriggerField = isTrigger,
                                        IsResponseField = isResponse,
                                        PageStrategyUid = thisPanel.Page.Uid,
                                        Key = field.Uid.ToString(),
                                        PanelColumn = field.PanelColumn,
                                        PanelColumnSortOrder = field.PanelColumnSortOrder,
                                        Text = field.LiteralText,
                                        //Template =
                                        //    String.Format("<p>{0}</p>",
                                        //                  field.LiteralText.KillHtml()),
                                        DisplayOrder = columnOrder,
                                        ViewRoles = field.ViewRoles,
                                        EditRoles = field.EditRoles,
                                        User = this.User.Identity.Name,
                                        UserRoles = Roles.GetRolesForUser(this.User.Identity.Name).ToList()
                                        // se cambia la propiedad de orden
                                        //DisplayOrder = field.SortOrder
                                    });
                                    break;

                                #endregion

                                #region TextBox

                                case ("TextBox"):
                                    dynamicFormFields.Add(new TextBox()
                                    {
                                        //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),

                                        Index = newIndexPanel.ToString() + columnOrder.ToString(),
                                        MinSize = formField.MinSize,
                                        IsNumber = (formField.FormFieldType.FieldTypeName == FieldTypeNumericName ? true : false),
                                        IsCurrency = (formField.FormFieldType.FieldTypeName.Equals(FieldTypeCurrency) ? true : false),
                                        IsText = (formField.FormFieldType.FieldTypeName.Equals("Solo Texto") ? true : false),
                                        MaxSize = !string.IsNullOrEmpty(field.MaxSize) ? field.MaxSize : field.MaxSizeBD,
                                        IsTriggerField = isTrigger,
                                        IsResponseField = isResponse,
                                        PageStrategyUid = thisPanel.Page.Uid,
                                        Key = field.Uid.ToString(),
                                        Value = value,
                                        idStrategy = field.ValidationStrategyID,
                                        PanelColumn = field.PanelColumn,
                                        PanelColumnSortOrder = field.PanelColumnSortOrder,
                                        ResponseTitle = field.FormFieldName,
                                        Prompt =
                                             (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                                  ? field.FormFieldPrompt
                                                  : null,
                                        DisplayOrder = columnOrder,

                                        // se cambia la propiedad de orden
                                        //DisplayOrder = field.SortOrder
                                        Required = Convert.ToBoolean(field.IsRequired),
                                        IsRequeried = Convert.ToBoolean(field.IsRequired),
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
                                        UserRoles = Roles.GetRolesForUser(this.User.Identity.Name).ToList()
                                    });
                                    break;

                                #endregion

                                #region Calendar

                                case ("Calendar"):
                                    if (string.IsNullOrEmpty(value))
                                        dynamicFormFields.Add(new Calendar()
                                        {
                                            //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),

                                            Index = newIndexPanel.ToString() + columnOrder.ToString(),
                                            Key = field.Uid.ToString(),
                                            ResponseTitle = field.FormFieldName,
                                            PanelColumn = field.PanelColumn,
                                            PanelColumnSortOrder = field.PanelColumnSortOrder,
                                            idStrategy = field.ValidationStrategyID,
                                            IsTriggerField = isTrigger,
                                            IsResponseField = isResponse,
                                            PageStrategyUid = thisPanel.Page.Uid,
                                            Prompt =
                                                 (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                                      ? field.FormFieldPrompt
                                                      : null,
                                            DisplayOrder = columnOrder,
                                            // se cambia la propiedad de orden
                                            //DisplayOrder = field.SortOrder
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
                                                      : string.Empty,
                                            ViewRoles = field.ViewRoles,
                                            EditRoles = field.EditRoles,
                                            User = this.User.Identity.Name,
                                            UserRoles = Roles.GetRolesForUser(this.User.Identity.Name).ToList()
                                        });
                                    else
                                        dynamicFormFields.Add(new Calendar()
                                        {
                                            //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),
                                            Value = (Convert.ToDateTime(value).ToShortDateString() == "01/01/1900" ? "" : DateTime.Parse(value).ToString(_dateFormat)),

                                            Index = newIndexPanel.ToString() + columnOrder.ToString(),
                                            Key = field.Uid.ToString(),
                                            ResponseTitle = field.FormFieldName,
                                            PanelColumn = field.PanelColumn,
                                            PanelColumnSortOrder = field.PanelColumnSortOrder,
                                            idStrategy = field.ValidationStrategyID,
                                            IsTriggerField = isTrigger,
                                            IsResponseField = isResponse,
                                            PageStrategyUid = thisPanel.Page.Uid,
                                            Prompt =
                                                 (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                                      ? field.FormFieldPrompt
                                                      : null,
                                            DisplayOrder = columnOrder,
                                            // se cambia la propiedad de orden
                                            //DisplayOrder = field.SortOrder
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
                                                      : string.Empty,
                                            ViewRoles = field.ViewRoles,
                                            EditRoles = field.EditRoles,
                                            User = this.User.Identity.Name,
                                            UserRoles = Roles.GetRolesForUser(this.User.Identity.Name).ToList()
                                        });
                                    break;
                                #endregion

                                #region TextArea

                                case ("TextArea"):
                                    var newTextArea = new TextArea()
                                    {
                                        //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),

                                        Index = newIndexPanel.ToString() + columnOrder.ToString(),
                                        MaxSize = !string.IsNullOrEmpty(field.MaxSize) ? field.MaxSize : field.MaxSizeBD,
                                        IsTriggerField = isTrigger,
                                        IsResponseField = isResponse,
                                        PageStrategyUid = thisPanel.Page.Uid,
                                        Key = field.Uid.ToString(),
                                        PanelColumn = field.PanelColumn,
                                        PanelColumnSortOrder = field.PanelColumnSortOrder,
                                        ResponseTitle = field.FormFieldName,
                                        Value = value,
                                        Prompt =
                                             (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                                  ? field.FormFieldPrompt
                                                  : null,
                                        DisplayOrder = columnOrder,
                                        ViewRoles = field.ViewRoles,
                                        EditRoles = field.EditRoles,
                                        User = this.User.Identity.Name,
                                        UserRoles = Roles.GetRolesForUser(this.User.Identity.Name).ToList(),
                                        // se cambia la propiedad de orden
                                        //DisplayOrder = field.SortOrder
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
                                #endregion

                                #region SelectList

                                case ("SelectList"):
                                    var newSelectList = new Select
                                    {
                                        //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),

                                        Index = newIndexPanel.ToString() + columnOrder.ToString(),
                                        PanelColumn = field.PanelColumn,
                                        PanelColumnSortOrder = field.PanelColumnSortOrder,
                                        Key = field.Uid.ToString(),
                                        ResponseTitle = field.FormFieldName,
                                        IsTriggerField = isTrigger,
                                        IsResponseField = isResponse,
                                        PageStrategyUid = thisPanel.Page.Uid,
                                        Prompt =
                                             (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                                  ? field.FormFieldPrompt
                                                  : null,
                                        DisplayOrder = columnOrder,
                                        // se cambia la propiedad de orden
                                        //DisplayOrder = field.SortOrder
                                        Required = Convert.ToBoolean(field.IsRequired),
                                        RequiredMessage =
                                             Convert.ToBoolean(field.IsRequired)
                                                  ? thisFieldType.ErrorMsgRequired.Replace(
                                                        "%FormFieldName%", field.FormFieldName)
                                                  : string.Empty,
                                        ViewRoles = field.ViewRoles,
                                        EditRoles = field.EditRoles,
                                        User = this.User.Identity.Name,
                                        UserRoles = Roles.GetRolesForUser(this.User.Identity.Name).ToList()
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

                                    foreach (var item in newSelectList.Choices)
                                    {
                                        bool re;
                                        re = value.Contains(item.Value);
                                        if (re)
                                            item.Selected = true;
                                    }

                                    dynamicFormFields.Add(newSelectList);
                                    break;

                                #endregion

                                #region CheckBox

                                case ("CheckBox"):
                                    var newCheckBox = new CheckBox
                                    {
                                        //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),

                                        Index = newIndexPanel.ToString() + columnOrder.ToString(),
                                        Key = field.Uid.ToString(),
                                        PanelColumn = field.PanelColumn,
                                        PanelColumnSortOrder = field.PanelColumnSortOrder,
                                        IsTriggerField = isTrigger,
                                        IsResponseField = isResponse,
                                        PageStrategyUid = thisPanel.Page.Uid,
                                        ResponseTitle = field.FormFieldName,
                                        Prompt =
                                             (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                                  ? field.FormFieldPrompt
                                                  : null,
                                        DisplayOrder = columnOrder,
                                        // se cambia la propiedad de orden
                                        //DisplayOrder = field.SortOrder
                                        Required = Convert.ToBoolean(field.IsRequired),
                                        RequiredMessage =
                                             Convert.ToBoolean(field.IsRequired)
                                                  ? thisFieldType.ErrorMsgRequired.Replace(
                                                        "%FormFieldName%", field.FormFieldName)
                                                  : string.Empty,
                                        ViewRoles = field.ViewRoles,
                                        EditRoles = field.EditRoles,
                                        User = this.User.Identity.Name,
                                        UserRoles = Roles.GetRolesForUser(this.User.Identity.Name).ToList()
                                    };

                                    dynamicFormFields.Add(newCheckBox);
                                    break;
                                #endregion

                                #region CheckBoxList

                                case ("CheckBoxList"):
                                    var newCheckBoxList = new CheckBoxList
                                    {
                                        //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),

                                        Index = newIndexPanel.ToString() + columnOrder.ToString(),
                                        Key = field.Uid.ToString(),
                                        ResponseTitle = field.FormFieldName,
                                        PanelColumn = field.PanelColumn,
                                        PanelColumnSortOrder = field.PanelColumnSortOrder,
                                        IsTriggerField = isTrigger,
                                        IsResponseField = isResponse,
                                        PageStrategyUid = thisPanel.Page.Uid,
                                        Prompt =
                                             (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                                  ? field.FormFieldPrompt
                                                  : null,
                                        DisplayOrder = columnOrder,
                                        // se cambia la propiedad de orden
                                        //DisplayOrder = field.SortOrder
                                        Required = Convert.ToBoolean(field.IsRequired),
                                        RequiredMessage =
                                             Convert.ToBoolean(field.IsRequired)
                                                  ? thisFieldType.ErrorMsgRequired.Replace(
                                                        "%FormFieldName%", field.FormFieldName)
                                                  : string.Empty,
                                        CommaDelimitedChoices = GetOptionsForSelector(field),
                                        Orientation = (field.Orientation == "vertical") ? Orientation.Vertical : Orientation.Horizontal,
                                        ViewRoles = field.ViewRoles,
                                        EditRoles = field.EditRoles,
                                        User = this.User.Identity.Name,
                                        UserRoles = Roles.GetRolesForUser(this.User.Identity.Name).ToList()
                                    };


                                    foreach (var item in newCheckBoxList.Choices)
                                    {
                                        bool re;
                                        re = value.Contains(item.Value);
                                        if (re)
                                            item.Selected = true;
                                    }

                                    dynamicFormFields.Add(newCheckBoxList);
                                    break;
                                #endregion

                                #region RadioList

                                case ("RadioList"):
                                    var newRadioList = new RadioList
                                    {
                                        //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),

                                        Index = newIndexPanel.ToString() + columnOrder.ToString(),
                                        Key = field.Uid.ToString(),
                                        ResponseTitle = field.FormFieldName,
                                        PanelColumn = field.PanelColumn,
                                        PanelColumnSortOrder = field.PanelColumnSortOrder,
                                        IsTriggerField = isTrigger,
                                        IsResponseField = isResponse,
                                        PageStrategyUid = thisPanel.Page.Uid,
                                        Prompt =
                                             (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                                  ? field.FormFieldPrompt
                                                  : null,
                                        DisplayOrder = columnOrder,
                                        // se cambia la propiedad de orden
                                        //DisplayOrder = field.SortOrder
                                        Required = Convert.ToBoolean(field.IsRequired),
                                        RequiredMessage =
                                             Convert.ToBoolean(field.IsRequired)
                                                  ? thisFieldType.ErrorMsgRequired.Replace(
                                                        "%FormFieldName%", field.FormFieldName)
                                                  : string.Empty,
                                        CommaDelimitedChoices = GetOptionsForSelector(field),
                                        Orientation = (field.Orientation == "vertical") ? Orientation.Vertical : Orientation.Horizontal,
                                        ViewRoles = field.ViewRoles,
                                        EditRoles = field.EditRoles,
                                        User = this.User.Identity.Name,
                                        UserRoles = Roles.GetRolesForUser(this.User.Identity.Name).ToList()
                                    };

                                    foreach (var item in newRadioList.Choices)
                                    {
                                        if (item.Value == value)
                                            item.Selected = true;
                                    }
                                    dynamicFormFields.Add(newRadioList);
                                    break;
                                #endregion

                                #region FileUpload

                                case ("FileUpload"):
                                    var newFileUpload = new FileUpload
                                    {
                                        //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),

                                        Index = newIndexPanel.ToString() + columnOrder.ToString(),
                                        UriFilePath = value,
                                        MaxSize = !string.IsNullOrEmpty(field.MaxSize) ? field.MaxSize : field.MaxSizeBD,
                                        IsTriggerField = isTrigger,
                                        IsResponseField = isResponse,
                                        PageStrategyUid = thisPanel.Page.Uid,
                                        ValidExtensions = field.ValidExtensions,
                                        ErrorExtensions = field.ErrorExtensions,
                                        Key = field.Uid.ToString(),
                                        PanelColumn = field.PanelColumn,
                                        PanelColumnSortOrder = field.PanelColumnSortOrder,
                                        ResponseTitle = field.FormFieldName,
                                        Prompt =
                                             (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                                  ? field.FormFieldPrompt
                                                  : null,
                                        InvalidExtensionError = field.ErrorExtensions,
                                        Required = Convert.ToBoolean(field.IsRequired),
                                        RequiredMessage =
                                             Convert.ToBoolean(field.IsRequired)
                                                  ? thisFieldType.ErrorMsgRequired.Replace(
                                                        "%FormFieldName%", field.FormFieldName)
                                                  : string.Empty,
                                        DisplayOrder = columnOrder,
                                        ViewRoles = field.ViewRoles,
                                        EditRoles = field.EditRoles,
                                        User = this.User.Identity.Name,
                                        UserRoles = Roles.GetRolesForUser(this.User.Identity.Name).ToList()
                                        // se cambia la propiedad de orden
                                        //DisplayOrder = field.SortOrder
                                    };

                                    if (!string.IsNullOrEmpty(field.ValidExtensions))
                                        newFileUpload.ValidExtensions = "." +
                                                                                  field.ValidExtensions.Replace(Environment.NewLine,
                                                                                                                          ",.");
                                    //var user = Membership.GetUser(User.Identity.Name);
                                    //UserKey = user.ProviderUserKey.ToString();
                                    //newFileUpload.Validated += FileValidated;
                                    //newFileUpload.Posted += FilePosted;
                                    dynamicFormFields.Add(newFileUpload);
                                    break;
                                #endregion

                                #region LHyperLink

                                case ("LHyperLink"):
                                    dynamicFormFields.Add(new LHyperLink()
                                    {
                                        //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),

                                        Index = newIndexPanel.ToString() + columnOrder.ToString(),
                                        Key = field.Uid.ToString(),
                                        PanelColumn = field.PanelColumn,
                                        PanelColumnSortOrder = field.PanelColumnSortOrder,
                                        IsTriggerField = isTrigger,
                                        IsResponseField = isResponse,
                                        PageStrategyUid = thisPanel.Page.Uid,
                                        //Template = String.Format("<a href={0}>{1}</a>", "\"http://" + field.LiteralText + "\"", field.FormFieldName),
                                        DisplayOrder = columnOrder,
                                        Text = field.FormFieldName,
                                        Target = field.LiteralText,
                                        Display = true,
                                        ViewRoles = field.ViewRoles,
                                        EditRoles = field.EditRoles,
                                        User = this.User.Identity.Name,
                                        UserRoles = Roles.GetRolesForUser(this.User.Identity.Name).ToList()
                                    });
                                    break;
                                #endregion

                            }
                            #endregion tipos de control

                            columnOrder++;
                        }

                        #endregion validar si contiene rol de visualizacion

                    }
                    var dynamicForm = new global::STPC.DynamicForms.Core.Form();
                    dynamicForm.AddFields(dynamicFormFields.ToArray());
                    dynamicForm.Serialize = true;
                    dynamicForm.NumColumnsPanel = thisPanel.Columns;

                    return PartialView(dynamicForm);
                }

                #endregion Validar si el panel contiene campos

                #region panel vacio

                else
                {
                    //return PartialView("Error");
                    // 04/02/2013 
                    // se modifca la visualizacion de esta vista parcial
                    // debido a que daña el esquema 
                    // esto se debe a que el panel no contiene campos para desplegar

                    var emptyList = new List<Field>();
                    emptyList.Add(new TextBox()
                    {
                        Key = new Guid().ToString(),
                        Template =
                             String.Format("<h4>{0}</h4>", "Este Panel no contiene campos"),
                        DisplayOrder = 1
                    });

                    var emptyForm = new global::STPC.DynamicForms.Core.Form();
                    emptyForm.AddFields(emptyList.ToArray());
                    emptyForm.Serialize = true;
                    return PartialView(emptyForm);
                }

                #endregion panel vacio
            }
            catch (Exception Ex)
            {
                #region Validacion de errores

                bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
                Guid correlationID = Guid.NewGuid();

                ViewBag.correlationID = correlationID;

                // enmascarar la excepcion
                #region Exception, No existe store procedure

                if (Ex.Message.Contains("Could not find stored procedure"))
                {
                    // crear registro de error en el visor de sucesos
                    EventLogRegister.WriteEventLog(string.Format("Ha ocurrido un error, el o los objetos {0} no existen o no han sido creados en la base de datos. Por favor comunicarse con el administrador", Ex.Message.Split(' ')[Ex.Message.Split(' ').Length - 1]), "PanelController", "Respond", correlationID);

                    if (ShowErrorDetail)
                        return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(string.Format("Ha ocurrido un error, el o los objetos {0} no existen o no han sido creados en la base de datos. Por favor comunicarse con el administrador", Ex.Message.Split(' ')[Ex.Message.Split(' ').Length - 1])), "Panel Controller", "Respond"));

                    return PartialView("_ErrorGeneral", "Ha ocurrido un error procesando su solicitud. Por favor comunicarse con el administrador" + "\n" + "Código del error: " + correlationID.ToString());
                }

                #endregion Exception, No existe store procedure

                #region Exception, No existe tabla

                else if (Ex.Message.Contains("Invalid object name"))
                {
                    // crear registro de error en el visor de sucesos
                    EventLogRegister.WriteEventLog(string.Format("Ha ocurrido un error, el o los objetos {0} no existen o no han sido creados en la base de datos. Por favor comunicarse con el administrador", Ex.Message.Split(' ')[Ex.Message.Split(' ').Length - 1]), "PanelController", "Respond", correlationID);

                    if (ShowErrorDetail)
                        return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(string.Format("Ha ocurrido un error, el o los objetos {0} no existen o no han sido creados en la base de datos. Por favor comunicarse con el administrador", Ex.Message.Split(' ')[Ex.Message.Split(' ').Length - 1])), "Panel Controller", "Respond"));

                    return PartialView("_ErrorGeneral", "Ha ocurrido un error procesando su solicitud. Por favor comunicarse con el administrador" + "\n" + "Código del error: " + correlationID.ToString());
                }

                #endregion Exception, No existe tabla

                #region Excepcion no clasificada

                else
                {
                    // crear registro de error en el visor de sucesos
                    EventLogRegister.WriteEventLog(Ex.Message, "PanelController", "Respond", Guid.NewGuid());

                    if (ShowErrorDetail)
                        return PartialView("_ErrorDetail", new HandleErrorInfo(Ex, "Panel Controller", "Respond"));

                    return PartialView("_ErrorGeneral", "Ha ocurrido un error procesando su solicitud. Por favor comunicarse con el administrador" + "\n" + "Código del error: " + correlationID.ToString());
                }

                #endregion Excepcion no clasificada

                #endregion Validacion de errores
            }

        }

        [HttpPost]
        public ActionResult Edit(FormViewModel viewModel, FormCollection collection)
        {
            //TODO: With the complexity of what we're sending back in the viewModel, the ModelState.
            //IsValid breaks down ... need to re-evaluate
            //if (ModelState.IsValid)
            //{
            try
            {


                _stpcForms.IgnoreResourceNotFoundException = true;
                var thePanel = _stpcForms.Panels.Expand("Page").Where(i => i.Uid == viewModel.panel.Uid).FirstOrDefault();
                if (thePanel == null) return View("Error");
                DeleteFormFieldsNotInThePost(viewModel);

                #region almacenar los campos del panel

                int sortOrderDT = 1; // este indice manejara la visualizacion en designer
                int sortOrderRT = 1; // este indice manejara la visualizacion en runtime, depende de permisos

                // TODO:
                // asi como se recorren los formfields, hay que recorrer con un for i
                // los atributos del formcollection MaxSize y MaxSizeBD ya que vienen concatenados!!!
                int currentField = 0;


                foreach (var formField in viewModel.formfields)
                {
                    var newMaxSizeBD = collection["formfields[" + collection["formfields.index"].Split(',')[currentField].ToString() + "].MaxSizeBD"] != null ? collection["formfields[" + collection["formfields.index"].Split(',')[currentField].ToString() + "].MaxSizeBD"].Split(',').Reverse().ToArray()[0] : string.Empty;
                    var newMaxSize = collection["formfields[" + collection["formfields.index"].Split(',')[currentField].ToString() + "].MaxSize"] != null ? collection["formfields[" + collection["formfields.index"].Split(',')[currentField].ToString() + "].MaxSize"].Split(',').Reverse().ToArray()[0] : string.Empty;

                    var newMinSize = collection["formfields[" + collection["formfields.index"].Split(',')[currentField].ToString() + "].MinSize"] != null ? collection["formfields[" + collection["formfields.index"].Split(',')[currentField].ToString() + "].MinSize"].Split(',').Reverse().ToArray()[0] : string.Empty;

                    //get the Option Strategy selected
                    var OptionStrategy = collection["formfields[" + collection["formfields.index"].Split(',')[currentField].ToString() + "].Strategies"];

                    var thisFormFieldUid = new Guid(formField.SelectedFormFieldType);
                    var formFieldType = _stpcForms.PageFieldTypes.Where(formft => formft.Uid == thisFormFieldUid).First();

                    if (formFieldType.FieldTypeName.Equals(_TIPO_NUMERO) || formFieldType.FieldTypeName.Equals(_TIPO_DECIMAL) || formFieldType.FieldTypeName.Equals(_TIPO_MONEDA))
                    {
                        if (double.Parse(string.IsNullOrEmpty(newMaxSize) ? "0" : newMaxSize) > double.Parse(string.IsNullOrEmpty(newMaxSizeBD) ? "0" : newMaxSizeBD))
                            return View("Error");

                    }

                    //Determine if this is an existing form item or not
                    var oldFormField = _stpcForms.PageFields.Expand("ViewR").Where(item => item.Uid == formField.Uid).FirstOrDefault();

                    #region Campo existente

                    if (oldFormField != null)
                    {
                        // -----------------------------------------------------
                        // adiciono estos campos que estan en la seccion de nuevo
                        var newFormField = new Services.Entities.PageField();
                        oldFormField.Panel = thePanel;
                        oldFormField.FormFieldType = formFieldType;

                        // Se adicionan estas propiedades para administrar la columna
                        // del panel a la cual pertenece el campo
                        // y el orden configurado
                        oldFormField.PanelColumn = int.Parse(collection["colunmNumber"]);
                        oldFormField.PanelColumnSortOrder = sortOrderRT;
                        oldFormField.PanelUid = thePanel.Uid;

                        oldFormField.ShowDelete = true;

                        // -----------------------------------------------------

                        if (!string.IsNullOrEmpty(formField.FormFieldName)) oldFormField.FormFieldName = formField.FormFieldName.Replace(" ", "");

                        oldFormField.FormFieldPrompt = formField.FormFieldPrompt;
                        oldFormField.IsRequired = formField.IsRequired;
                        oldFormField.Style = formField.Style;
                        oldFormField.Queryable = formField.Queryable;
                        oldFormField.SortOrder = sortOrderDT;

                        if (string.IsNullOrEmpty(formField.ValidationStrategyId))
                            oldFormField.ValidationStrategyID = null;
                        else
                            oldFormField.ValidationStrategyID = Convert.ToInt32(formField.ValidationStrategyId);

                        //TODO: Not sure if this is per field type, but it shouldn't matter if validation works and nulls don't matter
                        oldFormField.Options = formField.Options;
                        oldFormField.Orientation = formField.Orientation;
                        oldFormField.IsMultipleSelect = formField.IsMultipleSelect;
                        oldFormField.ListSize = formField.ListSize;
                        oldFormField.IsEmptyOption = formField.IsEmptyOption;
                        oldFormField.EmptyOption = formField.EmptyOption;
                        oldFormField.Rows = formField.Rows;
                        oldFormField.Cols = formField.Cols;
                        oldFormField.ValidExtensions = formField.ValidExtensions;
                        oldFormField.ErrorExtensions = formField.ErrorExtensions;
                        oldFormField.MaxSizeBD = newMaxSizeBD;
                        oldFormField.MinSize = newMinSize;
                        oldFormField.ToolTip = formField.ToolTip;
                        oldFormField.MaxSize = newMaxSize;

                        //oldFormField.MaxSizeBD = collection["formfields[" + collection["formfields.index"].Split(',')[currentField].ToString() + "].MaxSizeBD"].Split(',').Reverse().ToArray()[0];
                        //oldFormField.MaxSize = collection["formfields[" + collection["formfields.index"].Split(',')[currentField].ToString() + "].MaxSize"].Split(',').Reverse().ToArray()[0];

                        oldFormField.LiteralText = formField.LiteralText;

                        oldFormField.OptionsMode = collection["OptionsMode " + formField.Uid.ToString()];

                        //depend of the OptionsMode, select the correct value to save in OptionsWebServiceUrl
                        oldFormField.OptionsWebServiceUrl = oldFormField.OptionsMode == "ws" ? formField.OptionsWebServiceUrl : OptionStrategy;

                        if (formField.Options == null)
                            oldFormField.OptionsCategoryName = formField.OptionsCategoryName;
                        else
                            oldFormField.OptionsCategoryName = null;

                        oldFormField.EditRoles = string.Empty;
                        oldFormField.ViewRoles = string.Empty;

                        #region procesar roles asociados

                        Role theRol = new Role();

                        foreach (string index in collection)
                        {
                            if (collection[index] == formField.FormFieldName)
                            {
                                var result = collection.AllKeys.Where(x => x.Contains(index.Split('.')[0]));

                                #region Rol de visualizacion

                                foreach (string viewR in result.Where(x => x.Contains("View_Rol_")))
                                {
                                    var theRoleName = viewR.Substring(viewR.IndexOf("View_Rol_") + 9);
                                    theRol = _stpcForms.Roles.Expand("ViewFields").Where(n => n.Rolename == theRoleName).FirstOrDefault();
                                    if (theRol != null)
                                    {
                                        if (collection[viewR].Contains("true"))
                                        {
                                            oldFormField.ViewR.Add(theRol);
                                            oldFormField.ViewRoles = oldFormField.ViewRoles + theRoleName + _ROLE_SEPARATOR_CHARACTER_;
                                        }

                                        else
                                        {
                                            oldFormField.ViewR.Remove(theRol);
                                        }
                                    }
                                }

                                #endregion

                                #region Rol de edicion

                                foreach (string editR in result.Where(x => x.Contains("Edit_Rol")))
                                {
                                    var theRoleName = editR.Substring(editR.IndexOf("Edit_Rol_") + 9);
                                    theRol = _stpcForms.Roles.Where(n => n.Rolename == theRoleName).FirstOrDefault();
                                    if (theRol != null)
                                    {
                                        if (collection[editR].Contains("true"))
                                        {
                                            oldFormField.EditR.Add(theRol);
                                            oldFormField.EditRoles = oldFormField.EditRoles + theRoleName + _ROLE_SEPARATOR_CHARACTER_;

                                        }

                                        else
                                        {
                                            oldFormField.EditR.Remove(theRol);
                                        }
                                    }
                                }

                                #endregion Rol de edicion

                                // TODO: se adiciona esta linea para romper el ciclo una vez encuentra el registro 
                                // para que no continue buscando en el resto de la coleccion
                                break;
                            }
                        }

                        #endregion procesar roles asociados

                        if (!string.IsNullOrEmpty(oldFormField.ViewRoles) || formFieldType.ControlType.Equals("Blank"))
                            sortOrderRT++;

                        _stpcForms.UpdateObject(oldFormField);
                    }

                    #endregion Campo existente

                    #region Campo nuevo

                    else
                    {
                        // ----------------------------------------------------
                        var newFormField = new Services.Entities.PageField();
                        newFormField.Uid = Guid.NewGuid();
                        newFormField.Panel = thePanel;
                        newFormField.FormFieldType = formFieldType;
                        if (!string.IsNullOrEmpty(formField.FormFieldName)) newFormField.FormFieldName = formField.FormFieldName.Replace(" ", "");

                        newFormField.FormFieldPrompt = formField.FormFieldPrompt;
                        newFormField.IsHidden = false;
                        newFormField.IsRequired = formField.IsRequired;
                        newFormField.Style = formField.Style;
                        newFormField.Queryable = formField.Queryable;
                        newFormField.SortOrder = sortOrderDT;
                        newFormField.Timestamp = DateTime.Now;
                        //TODO: Not sure if this is per field type, but it shouldn't matter if validation works and nulls don't matter
                        newFormField.Options = formField.Options;
                        newFormField.Orientation = formField.Orientation;
                        newFormField.IsMultipleSelect = formField.IsMultipleSelect;
                        newFormField.ListSize = formField.ListSize;
                        newFormField.IsEmptyOption = formField.IsEmptyOption;
                        newFormField.EmptyOption = formField.EmptyOption;
                        newFormField.Rows = formField.Rows;
                        newFormField.Cols = formField.Cols;
                        newFormField.ValidExtensions = formField.ValidExtensions;
                        newFormField.ErrorExtensions = formField.ErrorExtensions;
                        newFormField.MaxSizeBD = newMaxSizeBD;
                        newFormField.MaxSize = newMaxSize;
                        newFormField.MinSize = newMinSize;
                        //newFormField.MaxSizeBD = collection["formfields[" + collection["formfields.index"].Split(',')[currentField].ToString() + "].MaxSizeBD"].Split(',').Reverse().ToArray()[0];
                        //newFormField.MaxSize = collection["formfields[" + collection["formfields.index"].Split(',')[currentField].ToString() + "].MaxSize"].Split(',').Reverse().ToArray()[0];
                        newFormField.ToolTip = formField.ToolTip;
                        newFormField.LiteralText = formField.LiteralText;
                        newFormField.FormFieldType_Uid = formFieldType.Uid;
                        newFormField.OptionsMode = collection["OptionsMode " + formField.Uid.ToString()];

                        //depend of the OptionsMode, select the correct value to save in OptionsWebServiceUrl
                        newFormField.OptionsWebServiceUrl = newFormField.OptionsMode == "ws" ? formField.OptionsWebServiceUrl : ViewBag.SelectedStrategyValue;

                        if (formField.Options == null)
                            newFormField.OptionsCategoryName = formField.OptionsCategoryName;

                        // -----------------------------------------------------          
                        // Se adicionan estas propiedades para administrar la columna
                        // del panel a la cual pertenece el campo
                        // y el orden configurado
                        newFormField.PanelColumn = int.Parse(collection["colunmNumber"]);
                        newFormField.PanelColumnSortOrder = sortOrderDT;
                        newFormField.PanelUid = thePanel.Uid;
                        newFormField.ShowDelete = true;

                        // -----------------------------------------------------

                        #region procesar roles asociados

                        foreach (string index in collection)
                        {
                            if (collection[index] == formField.FormFieldName)
                            {
                                var result = collection.AllKeys.Where(x => x.Contains(index.Split('.')[0]));

                                #region Rol de visualizacion

                                foreach (string viewR in result.Where(x => x.Contains("View_Rol_")))
                                {
                                    var theRoleName = viewR.Substring(viewR.IndexOf("View_Rol_") + 9);
                                    var theRol = _stpcForms.Roles.Expand("ViewFields").Where(n => n.Rolename == theRoleName).FirstOrDefault();
                                    if (theRol != null)
                                    {
                                        if (collection[viewR].Contains("true"))
                                        {
                                            newFormField.ViewR.Add(theRol);
                                            newFormField.ViewRoles = newFormField.ViewRoles + theRoleName + _ROLE_SEPARATOR_CHARACTER_;
                                        }
                                    }
                                }

                                #endregion

                                #region Rol de edicion

                                foreach (string editR in result.Where(x => x.Contains("Edit_Rol")))
                                {
                                    var theRoleName = editR.Substring(editR.IndexOf("Edit_Rol_") + 9);
                                    var theRol = _stpcForms.Roles.Where(n => n.Rolename == theRoleName).FirstOrDefault();
                                    if (theRol != null)
                                    {
                                        if (collection[editR].Contains("true"))
                                        {
                                            newFormField.EditR.Add(theRol);
                                            newFormField.EditRoles = newFormField.EditRoles + theRoleName + _ROLE_SEPARATOR_CHARACTER_;
                                        }
                                    }
                                }

                                #endregion Rol de visualizacion

                                // TODO: se adiciona esta linea para romper el ciclo una vez encuentra el registro 
                                // para que no continue buscando en el resto de la coleccion
                                break;

                            }
                        }

                        #endregion procesar roles asociados

                        if (!string.IsNullOrEmpty(newFormField.ViewRoles))
                            sortOrderRT++;

                        _stpcForms.AddRelatedObject(thePanel, "PanelFields", newFormField);
                    }

                    #endregion Campo nuevo

                    currentField++;
                    sortOrderDT++;

                }

                #endregion almacenar los campos del panel

                thePanel.Name = viewModel.panel.Name;
                thePanel.Description = viewModel.panel.Description;
                _stpcForms.UpdateObject(thePanel);
                _stpcForms.SaveChanges();

                // se modifica esta linea para que regrese al listado de columnas
                return RedirectToAction("GetColumns", "Panel", new { id = thePanel.Uid });
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        private JsonResult ValidateData(FormViewModel viewModel, FormCollection collection)
        {
            return Json(new { success = true });
        }

        public ActionResult Delete(Guid id)
        {
            if (id == null) return View("Error");
            Guid thePageOfThePanel;
            var thePanel = _stpcForms.Panels.Expand("Page").Where(i => i.Uid == id).FirstOrDefault();
            if (thePanel != null)
            {
                thePageOfThePanel = thePanel.Page.Uid;
                _stpcForms.DeleteObject(thePanel);
                _stpcForms.SaveChanges();
                return RedirectToAction("Edit", "FormPage", new { id = thePageOfThePanel });
            }
            return View("Error");
        }

        [HttpPost]
        public ActionResult DeletePost(Guid id)
        {
            if (id == null) return View("Error");
            Guid thePageOfThePanel;
            var thePanel = _stpcForms.Panels.Expand("Page").Where(i => i.Uid == id).FirstOrDefault();
            if (thePanel != null)
            {
                thePageOfThePanel = thePanel.Page.Uid;
                _stpcForms.DeleteObject(thePanel);
                _stpcForms.SaveChanges();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        [HttpPost]
        public JsonResult Create(string name, string desc, int columns, string stylesheet, Guid formpageId)
        {
            var theFormPage = _stpcForms.FormPages.Expand("Panels").Where(i => i.Uid == formpageId).FirstOrDefault();
            int theOrder;
            if (theFormPage.Panels.Count == 0) theOrder = 1;
            else
                theOrder = theFormPage.Panels.Select(m => m.SortOrder).Max() + 1;
            var theNewPanel = new STPC.DynamicForms.Web.Services.Entities.Panel
            {
                Name = name,
                Description = desc,
                Columns = columns,
                DivCssStyle = stylesheet,
                Timestamp = DateTime.Now,
                SortOrder = theOrder
            };
            _stpcForms.AddRelatedObject(theFormPage, "Panels", theNewPanel);
            System.Data.Services.Client.DataServiceResponse response = _stpcForms.SaveChanges();
            Guid? newPanelid = null;
            foreach (System.Data.Services.Client.ChangeOperationResponse change in response)
            {
                System.Data.Services.Client.EntityDescriptor descriptor = change.Descriptor as System.Data.Services.Client.EntityDescriptor;
                if (descriptor != null)
                {
                    STPC.DynamicForms.Web.Services.Entities.Panel addedPanel = descriptor.Entity as STPC.DynamicForms.Web.Services.Entities.Panel;

                    if (addedPanel != null)
                    {
                        newPanelid = addedPanel.Uid;
                    }
                }
            }
            return Json(new { success = true, Name = name, Desc = desc, Columns = columns, CssStyle = stylesheet, uid = newPanelid });
        }

        public ActionResult GetColumns(Guid id)
        {
            var item = _stpcForms.Panels.Expand("PanelFields").Expand("Page").Where((x => x.Uid == id)).FirstOrDefault();

            ViewBag.CategoriesSelect = new List<SelectListItem>();
            foreach (var category in _stpcForms.Categories)
                ViewBag.CategoriesSelect.Add(new SelectListItem { Text = category.Name, Value = category.Uid.ToString() });
            ViewBag.StrategiesSelect = new List<SelectListItem>();
            foreach (var strategy in _decisionEngine.GetStrategyList())
                ViewBag.StrategiesSelect.Add(new SelectListItem { Text = strategy.Name, Value = strategy.Id.ToString() });


            var viewModel = new FormViewModel();
            viewModel.panel = item;

            var formFields = from items in item.PanelFields
                             where items.IsHidden == false
                             orderby items.SortOrder
                             select items;

            if (formFields.Count() > 0)
            {
                var countFields = 1;
                var listFields = String.Empty;
                foreach (var formField in formFields)
                {
                    var thisField = formField;
                    var thisFieldType = _stpcForms.PageFieldTypes.Where(formft => formft.Uid == formField.FormFieldType_Uid).First();
                    var editFormField = new FormFieldViewModel
                    {
                        Uid = formField.Uid,
                        FormFieldName = formField.FormFieldName,
                        FormFieldPrompt = formField.FormFieldPrompt,
                        ControlType = thisFieldType.ControlType,
                        SelectedFormFieldType = thisFieldType.Uid.ToString(),
                        IsRequired = Convert.ToBoolean(formField.IsRequired),
                        ShowDelete = (countFields > 1) ? true : false,
                        FormFieldTypes = GetFormFieldTypes(),
                        //TODO: Not sure if this is per field type, but it shouldn't matter 
                        Options = formField.Options,
                        Orientation = formField.Orientation,
                        IsMultipleSelect = Convert.ToBoolean(formField.IsMultipleSelect),
                        ListSize = formField.ListSize,
                        IsEmptyOption = Convert.ToBoolean(formField.IsEmptyOption),
                        EmptyOption = formField.EmptyOption,
                        Rows = formField.Rows,
                        Cols = formField.Cols,
                        ValidExtensions = formField.ValidExtensions,
                        ErrorExtensions = formField.ErrorExtensions,
                        MaxSize = formField.MaxSize,
                        LiteralText = formField.LiteralText,
                        OptionsMode = formField.OptionsMode,
                        OptionsCategoryName = formField.OptionsCategoryName,
                        OptionsWebServiceUrl = formField.OptionsWebServiceUrl,
                        AvailableRoles = Roles.GetAllRoles()
                    };
                    viewModel.formfields.Add(editFormField);
                    listFields += "," + thisField.Uid;
                    countFields++;
                }
                ViewBag.ListFields = listFields.Substring(1); //Starts at 0; remove the first ','
            }
            else
            {
                var newField = new FormFieldViewModel { FormFieldTypes = GetFormFieldTypes() };
                viewModel.formfields.Add(newField);
            }
            return View(viewModel);
        }
    }
}
