using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using STPC.DynamicForms.Web.RT.Helpers;
using STPC.DynamicForms.Web.RT.Models;
using STPC.DynamicForms.Web.RT.Services.Entities;
using STPC.DynamicForms.Core.Fields;
using System.Text;
using STPC.DynamicForms.DecisionEngine;
using System.Configuration;
using STPC.DynamicForms.Core;
using STPC.DynamicForms.Web.RT.Services.Request;
using STPC.DynamicForms.Web.Common;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Common.Messages;
using Newtonsoft.Json;

namespace STPC.DynamicForms.Web.RT.Controllers
{
    
    
  [Authorize]
  public class PanelController : Controller
  {
    public AbcRedisCacheManager _AbcRedisCacheManager = new AbcRedisCacheManager();
    private bool IsNewRequest = false;
    private IDecisionEngine _decisionEngine;
    List<Values> data = null;
    public List<RequestRepository> listRequestRepository = null;
    List<Values> dataPageEvent = null;
    private string _ROLE_SEPARATOR_CHARACTER_ = ",";
    List<ListStrategiesCache> _ListStrategiesCache;

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

    Services.Entities.STPC_FormsFormEntities _stpcForms;
    //Services.Entities.STPC_FormsFormEntities _stpcForms1;
    //Services.Entities.STPC_FormsFormEntities _stpcForms2;
    CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
    CustomRequestProvider requestProvider = new CustomRequestProvider();

    public PanelController(IDecisionEngine decisionEngine)
    {
      _decisionEngine = decisionEngine;
      _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
      //_stpcForms1 = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
      //_stpcForms2 = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
    }

    public PanelController()
    {
      string user = System.Configuration.ConfigurationManager.AppSettings["UserWs"];
      string pws = System.Configuration.ConfigurationManager.AppSettings["PwsWs"];
      string pollInterval = System.Configuration.ConfigurationManager.AppSettings["pollInterval"];
      string timeOut = System.Configuration.ConfigurationManager.AppSettings["timeOut"];
      STPC.DynamicForms.DecisionEngine.DecisionEngine iEngine = new STPC.DynamicForms.DecisionEngine.DecisionEngine(user, pws, Convert.ToInt32(pollInterval), Convert.ToInt32(timeOut));
      this._decisionEngine = iEngine;
      _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
      _stpcForms.IgnoreResourceNotFoundException = true;

      //_stpcForms1 = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
      //_stpcForms2 = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
    }

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

      foreach (var category in _stpcForms.Categories.Where(cat => cat.IsActive == true).OrderBy(e => e.Name))
        ViewBag.CategoriesSelect.Add(new SelectListItem { Text = category.Name, Value = category.Uid.ToString() });

      ViewBag.StrategiesSelect = new List<SelectListItem>();
      foreach (var strategy in _decisionEngine.GetStrategyList())
        StrategiesSelect.Add(new SelectListItem { Text = strategy.Name + " - " + strategy.Id.ToString(), Value = strategy.Id.ToString() });

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
      var availableRoles = Roles.GetAllRoles();
      Panel panel = _stpcForms.Panels.Where(uid => uid.Uid == panelUid).FirstOrDefault();
      var editPanel = new FormFieldViewModel
      {
        AvailableRoles = availableRoles,
        SelectedEditRoles = new List<string>(),
        SelectedViewRoles = new List<string>(),
        ViewRoles = panel.ViewRoles != null ? panel.ViewRoles : string.Empty,
        EditRoles = panel.EditRoles != null ? panel.EditRoles : string.Empty,
      };

      ProcesarRoles(editPanel);
      viewModel.SelectedEditRoles = editPanel.SelectedEditRoles;
      viewModel.SelectedViewRoles = editPanel.SelectedViewRoles;

      return PartialView("_" + fieldType.ControlType, viewModel);// { UpdateValidationForFormId = formId };
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryTokenAttribute]
    public ActionResult ListPanels(Guid id)
    {
      return GetListPanels(id);
    }

    private ActionResult GetListPanels(Guid id)
    {
      _stpcForms.MergeOption = System.Data.Services.Client.MergeOption.PreserveChanges;
      var item = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == id)).FirstOrDefault();
      ViewBag.FormPageName = item.Name;
      ViewBag.FormPageId = item.Uid;
      ViewBag.FormId = item.Form.Uid;
      return PartialView("ListPanels", item.Panels.OrderBy(o => o.SortOrder));
    }


    [HttpPost]
    [ValidateAntiForgeryTokenAttribute]
    public ActionResult ListControls(Guid id, int colunmNumber)
    {
      return getListControls(id, colunmNumber);
    }

    private ActionResult getListControls(Guid id, int colunmNumber)
    {
      _stpcForms.MergeOption = System.Data.Services.Client.MergeOption.PreserveChanges;
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

      foreach (var category in _stpcForms.Categories.Where(cat => cat.IsActive == true).OrderBy(e => e.Name))
        ViewBag.CategoriesSelect.Add(new SelectListItem { Text = category.Name, Value = category.Uid.ToString() });
      ViewBag.StrategiesSelect = new List<SelectListItem>();
      foreach (var strategy in (_decisionEngine.GetStrategyList().OrderBy(str => str.Name).ThenByDescending(str => str.Name)))
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
        var availableRoles = Roles.GetAllRoles();
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
            AvailableRoles = availableRoles,
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
          ProcesarRoles(editFormField);

          #endregion procesar los roles

          viewModel.formfields.Add(editFormField);
          listFields += "," + formField.Uid;
          countFields++;

          #endregion cargar datos del campo
        }
        ViewBag.ListFields = listFields.Substring(1); //Starts at 0; remove the first ','
        var editPanel = new FormFieldViewModel
            {
              AvailableRoles = availableRoles,
              SelectedEditRoles = new List<string>(),
              SelectedViewRoles = new List<string>(),
              ViewRoles = item.ViewRoles != null ? item.ViewRoles : string.Empty,
              EditRoles = item.EditRoles != null ? item.EditRoles : string.Empty,
            };

        ProcesarRoles(editPanel);
        viewModel.SelectedEditRoles = editPanel.SelectedEditRoles;
        viewModel.SelectedViewRoles = editPanel.SelectedViewRoles;
      }
      else
      {
        var newField = new FormFieldViewModel { FormFieldTypes = GetFormFieldTypes() };
        viewModel.formfields.Add(newField);
      }

      ViewBag.DefaultFieldStyle = System.Configuration.ConfigurationManager.AppSettings["DefaultFieldStyle"];
      return PartialView("Edit", viewModel);
    }

    private void ProcesarRoles(FormFieldViewModel editFormField)
    {
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
    }

    //[HttpPost]
    ////[ValidateAntiForgeryTokenAttribute]
    public ActionResult Respond(Guid id, string formName, int requestId, Dictionary<Guid, string> dcParentValue, bool isNewRequest)
    {
#if DEBUG
      DateTime refTime = DateTime.Now;



#endif
      List<global::STPC.DynamicForms.Core.Form> listdynamicForm = new List<Core.Form>();
      var dynamicFormFields = new List<Field>();
      string value = string.Empty;
      string parentValue = string.Empty;
      int columnOrder = 1;
      int newIndexPanel = 0;
      string imageNameState = string.Empty;
      string urlImageState = string.Empty;
      string SharedImagesBlob = System.Configuration.ConfigurationManager.AppSettings["SharedImagesBlob"];
      string FormStateSymbolsFolder = System.Configuration.ConfigurationManager.AppSettings["FormStateSymbolsFolder"];

      List<STPC.DynamicForms.Web.RT.Services.Entities.PageEvent> _PageEvent = null;
      List<STPC.DynamicForms.Web.RT.Services.Entities.PageEvent> _PageEventListener = null;
      List<STPC.DynamicForms.Web.RT.Services.Entities.PageEvent> _PageEventListenerHidden = null;
      PageMathOperation _PageMathOperation = null;
      List<PageMathOperation> listPageMathOperation = _stpcForms.PageMathOperation.ToList();
      List<FormStates> _listFormStates = _stpcForms.FormStates.ToList();
      string FieldTypeNumericName = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameNumeric"]) ? System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameNumeric"].ToString() : string.Empty;
      string FieldTypeCurrency = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameCurrency"]) ? System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameCurrency"].ToString() : string.Empty;
      string _dateFormat = System.Configuration.ConfigurationManager.AppSettings["FormatoFecha"];
      var currentUser = provider.GetUser(User.Identity.Name);


      RequestServiceClient _RequestServiceClient = new RequestServiceClient();
      var thisRequest = _RequestServiceClient.GetRequestById(requestId);
      FormPage page = _stpcForms.FormPages.Expand("Panels").Expand("PageEvents").Where(p => p.Uid == id).FirstOrDefault();
      data = requestProvider.GetPageFlowStepInstance(requestId, page.Uid, thisRequest.PageFlowId, formName);
      FormStates _FormStates = null;
      List<PageStrategy> allStrategiesList = _stpcForms.PageStrategy.Where(pag => pag.PageId == page.Uid).ToList();
      //List<PageEvent> pageEvents = _stpcForms.PageEvent.Expand("FormPage").Where(e => e.FormPageUid == thisPanel.Page.Uid).ToList();


      string aplicationName = System.Configuration.ConfigurationManager.AppSettings["AplicationName"];
      List<STPC.DynamicForms.Web.RT.Services.Entities.PageEvent> _PageEventCache = JsonConvert.DeserializeObject<List<STPC.DynamicForms.Web.RT.Services.Entities.PageEvent>>(_AbcRedisCacheManager["listPageEvent"]);
      //List<STPC.DynamicForms.Web.RT.Services.Entities.PageEvent> _PageEventCachetest = _stpcForms.PageEvent.ToList();


      //List<STPC.DynamicForms.Web.RT.Services.Entities.PageEvent> pageEvents = page.PageEvents.Where(p => p.FormPageUid == page.Uid).ToList();
      //List<PageEvent> pageEvents = _stpcForms.PageEvent.Where(pe => pe.FormPageUid == thisPanel.Page.Uid).ToList();
      bool IsReadOnly = ValidatePageReadOnlyState(thisRequest.WorkFlowState, page.ReadOnlyState, currentUser, page.Uid);
      List<string> userRoles = Roles.GetRolesForUser(this.User.Identity.Name).ToList();
      _ListStrategiesCache = new List<ListStrategiesCache>();
      bool isHidePanel = false;
      bool isDisabledPanel = false;
      bool flagView = false;
      bool flagEdit = false;

      try
      {
        _stpcForms.MergeOption = System.Data.Services.Client.MergeOption.PreserveChanges;


        foreach (var panel in page.Panels.OrderBy(or => or.SortOrder))
        {
          //Valida los permisos del panel al role del usuario 
          //flagEdit = false;
          //flagView = false;
          string eventNamePanel = string.Empty;

          if (userRoles != null && userRoles.Count > 0)
          {
            // recorrer roles asociados a usuario
            foreach (string itemRole in userRoles)
            {
              // view roles
              if (!string.IsNullOrEmpty(panel.ViewRoles) && !panel.ViewRoles.Contains(itemRole))
              {
                eventNamePanel = "Hide";
                break;
              }


            }
            foreach (string itemRole in userRoles)
            {
              // edit roles
              if (!string.IsNullOrEmpty(panel.EditRoles) && !panel.EditRoles.Contains(itemRole))
              {
                if (string.IsNullOrEmpty(eventNamePanel))
                {
                  eventNamePanel = "Disabled";
                }
                break;
              }
            }
          }

          _PageEventListener = _PageEventCache.Where(e => e.ListenerFieldId == panel.Uid && e.EventType != "Cascade").ToList();

          if (_PageEventListener.Count == 0)
            _PageEventListener = _PageEventCache.Where(e => e.ListenerFieldId == panel.Uid && e.EventType != "Cascade").ToList();

          string eventParentPanel = string.Empty;

          if (string.IsNullOrEmpty(eventNamePanel))
          {
            foreach (var pageEventListener in _PageEventListener)
            {

              parentValue = GetValue(requestId, pageEventListener.SourceField, pageEventListener.FormPageUid, thisRequest.PageFlowId, formName);
              eventParentPanel = pageEventListener.EventType;

              if (parentValue.Contains(pageEventListener.FieldValue))
              {
                eventNamePanel = pageEventListener.EventType;
              }

            }
          }
          /*if (flagView)
          {
              eventNamePanel = "Hide";
          }
          else
              if (!flagEdit)
              {
                  eventNamePanel = "Disabled";
              }*/

          var thisPanel = panel;
          var formFields = new List<PageField>();
          IsNewRequest = isNewRequest;
          dynamicFormFields.Clear();
          // consultar el request

#if DEBUG
          System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "PC-Respond-GetRequestById", DateTime.Now.Subtract(refTime).Milliseconds);
          refTime = DateTime.Now;
#endif

          formFields = _stpcForms.PageFields.Expand("FormFieldType").Expand("Panel").Where(e => e.PanelUid == thisPanel.Uid).OrderBy(f => f.PanelColumnSortOrder).ThenBy(f => f.PanelColumn).ToList();//thisPanel.PanelFields.OrderBy(f => f.PanelColumnSortOrder).ThenBy(f => f.PanelColumn).ToList();


#if DEBUG
          System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "PC-Respond-forEach PanelFields setFieldType", DateTime.Now.Subtract(refTime).Milliseconds);
          refTime = DateTime.Now;
#endif
          #region Validar si el panel contiene campos

          if (formFields.Count() > 0)
          {
            //Busca el appSetting que indique el nombre para el tipo de campo numerico y poder validar si un campos es de tipo numerico y agregarle
            // carga el tipo un atributo para que solo acepte valores numericos

#if DEBUG
            System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "PC-Respond-GetUser", DateTime.Now.Subtract(refTime).Milliseconds);
            refTime = DateTime.Now;
#endif

#if DEBUG
            System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "PC-Respond-ValidatePageReadOnly", DateTime.Now.Subtract(refTime).Milliseconds);
            refTime = DateTime.Now;
#endif

#if DEBUG
            System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "PC-Respond-ValidatePageReadOnly", DateTime.Now.Subtract(refTime).Milliseconds);
            refTime = DateTime.Now;
#endif




#if DEBUG
            System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "PC-Respond-allStrategies y PageEvents", DateTime.Now.Subtract(refTime).Milliseconds);
            refTime = DateTime.Now;
#endif


            #region navigate formFields

            foreach (var field in formFields)
            {
              try
              {
                value = GetValue(data, field.FormFieldName);
              }
              catch (Exception ex)
              {
                #region Validacion de errores

                bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
                Guid correlationID = Guid.NewGuid();
                ViewBag.correlationID = correlationID;

                // enmascarar la excepcion
                #region Exception, No existe store procedure

                if (ex.Message.Contains(CustomMessages.EX0001))
                {
                  ILogging eventWriter = LoggingFactory.GetInstance();
                  string errorMessage = string.Format(CustomMessages.E0002, ex.Message.Split(' ')[ex.Message.Split(' ').Length - 1], "PanelController", "Respond", correlationID, ex.Message);
                  eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

                  if (ShowErrorDetail)
                    return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(string.Format(CustomMessages.E0002, ex.Message.Split(' ')[ex.Message.Split(' ').Length - 1])), "Panel Controller", "Respond"));

                  return PartialView("_ErrorGeneral", CustomMessages.E0001 + "\n" + "Código del error: " + correlationID.ToString());
                }

                #endregion Exception, No existe store procedure

                #region Exception, No existe tabla

                else if (ex.Message.Contains(CustomMessages.EX0002))
                {
                  ILogging eventWriter = LoggingFactory.GetInstance();
                  string errorMessage = string.Format(CustomMessages.E0002, ex.Message.Split(' ')[ex.Message.Split(' ').Length - 1], "PanelController", "Respond", correlationID, ex.Message);
                  eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

                  if (ShowErrorDetail)
                    return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(string.Format(CustomMessages.E0002, ex.Message.Split(' ')[ex.Message.Split(' ').Length - 1])), "Panel Controller", "Respond"));

                  return PartialView("_ErrorGeneral", CustomMessages.E0001 + "\n" + "Código del error: " + correlationID.ToString());
                }

                #endregion Exception, No existe tabla

                #region Excepcion no clasificada

                else
                {
                  ILogging eventWriter = LoggingFactory.GetInstance();
                  string errorMessage = string.Format(CustomMessages.E0007, "Panel Controlle", "Respond", correlationID, ex.Message);
                  eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));// crear registro de error en el visor de sucesos

                  if (ShowErrorDetail)
                    return PartialView("_ErrorDetail", new HandleErrorInfo(ex, "Panel Controller", "Respond"));

                  return PartialView("_ErrorGeneral", CustomMessages.E0001 + "\n" + "Código del error: " + correlationID.ToString());
                }

                #endregion Excepcion no clasificada

                #endregion Validacion de errores
              }
              //Valida si el campo Style es null
              if (field.Style == null)
              {
                field.Style = string.Empty;
              }
              var thisFieldType = field.FormFieldType;

              #region validar si contiene rol de visualizacion

              // validar si contiene rol de visualizacion
              if (!string.IsNullOrEmpty(field.ViewRoles) || thisFieldType.ControlType.Equals("Blank"))
              {
                if (field.PanelColumnSortOrder <= 9)
                  newIndexPanel = thisPanel.SortOrder * 100;
                else
                  newIndexPanel = thisPanel.SortOrder * 10;

                #region validar si tiene un trigger asociado

                // consultar por el GUID de la pagina y el GUID del campo si es un trigger de estrategias
                var StrategyList = allStrategiesList.Where(pag => pag.TriggerFieldUid == field.Uid);
                bool isTrigger = StrategyList.Count() > 0 ? true : false;

                #endregion validar si tiene un trigger asociado

                #region validar si tiene un campo respuesta asociado

                // consultar por el GUID de la pagina y el GUID del campo si es un campo de respuesta
                var responseList = allStrategiesList.Where(pag => pag.ResponseFieldUid == field.Uid);
                bool isResponse = responseList.Count() > 0 ? true : false;

                #endregion validar si tiene un campo respuesta asociado

                #region carga nombre imagen del estado

                if (thisRequest.WorkFlowState != null)
                {
                  _FormStates = _listFormStates.Where(e => e.Uid == thisRequest.WorkFlowState).FirstOrDefault();

                }
                else
                {
                  _FormStates = _listFormStates.Where(e => e.Uid == new Guid()).FirstOrDefault();
                }

                //Configura ruta de la imagen en el BLOB

                imageNameState = string.IsNullOrEmpty(_FormStates.StateSymbol) ? string.Empty : _FormStates.StateSymbol;

                if (!imageNameState.Equals(string.Empty))
                {
                  urlImageState = SharedImagesBlob + FormStateSymbolsFolder + imageNameState;
                }

                #endregion
                // se agrupan los tipos de control para facilitar la lectura del codigo

                string ControlsToHide = string.Empty;
                _PageEvent = _PageEventCache.Where(e => e.PageFieldUid == field.Uid).ToList();
                _PageEventListener = _PageEventCache.Where(e => e.ListenerFieldId == field.Uid && e.EventType != "Cascade").ToList();

                if (_PageEventListener.Count == 0)
                  _PageEventListener = _PageEventCache.Where(e => e.ListenerFieldId == field.Uid && e.EventType != "Cascade").ToList();

                _PageEventListenerHidden = _PageEventCache.Where(e => e.ListenerFieldId == field.Uid && e.EventType == "Cascade").ToList();
                _PageMathOperation = listPageMathOperation.Where(e => e.ResultField == field.Uid).FirstOrDefault();


                string listEvent = string.Empty;

                foreach (var item in _PageEvent)
                {
                  listEvent += item.EventType + " ";
                }
                listEvent = listEvent.Trim();
                string eventName = string.Empty;
                string eventParent = string.Empty;

                if (isDisabledPanel)
                  eventName = eventNamePanel;
                else
                {
                  //dataPageEvent = null;
                  if (_PageEventListener.Count > 0)
                  {
                    #region Eventos de pagina
                    foreach (var pageEventListener in _PageEventListener)
                    {

                      parentValue = GetValue(requestId, pageEventListener.SourceField, pageEventListener.FormPageUid, thisRequest.PageFlowId, formName);
                      eventParent = pageEventListener.EventType;

                      if (pageEventListener.FieldValue == parentValue)
                      {
                        eventName = pageEventListener.EventType;

                      }
                      if (eventName != "Disabled" && eventName != "Hide")
                      {
                        //parentValue = GetValue(requestId, pageEventListener.SourceField, pageEventListener.FormPageUid, thisRequest.PageFlowId, formName);
                        if (dcParentValue != null)
                          if (dcParentValue.ContainsKey(_PageEventListener[0].PageFieldUid))
                          {
                            dcParentValue.TryGetValue(_PageEventListener[0].PageFieldUid, out parentValue);
                          }
                          else
                          {
                            if (dcParentValue.Count > 0)
                            {
                              PageField fieldParent = _stpcForms.PageFields.Expand("Panel").Where(e => e.Uid == _PageEventListener[0].PageFieldUid).FirstOrDefault();
                              Panel _panel = _stpcForms.Panels.Expand("Page").Where(e => e.Uid == fieldParent.Panel.Uid).FirstOrDefault();
                              parentValue = GetValue(requestId, _PageEventListener[0].SourceField, _panel.Page.Uid, thisRequest.PageFlowId, formName);
                            }
                          }
                        else
                          parentValue = GetValue(requestId, _PageEventListener[0].SourceField, _PageEventListener[0].FormPageUid, thisRequest.PageFlowId, formName);


                        foreach (var item in _PageEventListener)
                        {
                          if (item.FieldValue != null)
                            if (parentValue.Contains(item.FieldValue))
                            {
                              eventName = item.EventType;
                              break;
                            }
                        }

                      }
                    }
                    #endregion
                  }
                }
                #region tipos de control
                Boolean fieldIsRequeried = false;
                if (eventNamePanel.Equals("Hide") || eventNamePanel.Equals("Disabled") || eventName.Equals("Hide") || eventName.Equals("Disabled"))
                {
                  if (field.IsRequired)
                  {
                    fieldIsRequeried = true;                                        
                  }
                  field.IsRequired = false;
                }
                string GuidFormFieldTypeEmail = System.Configuration.ConfigurationManager.AppSettings["GuidFormFieldTypeEmail"];
                switch (thisFieldType.FieldType)
                {
                  #region Literal

                  case ("Literal"):
                    dynamicFormFields.Add(new Literal()
                    {
                      //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),
                      IsReadOnly = IsReadOnly,
                      Index = newIndexPanel.ToString() + columnOrder.ToString(),
                      IsTriggerField = isTrigger,
                      IsResponseField = isResponse,
                      PageStrategyUid = page.Uid,
                      Key = field.Uid.ToString(),
                      PanelColumn = field.PanelColumn,
                      PanelColumnSortOrder = field.PanelColumnSortOrder,
                      Text = field.LiteralText,
                      Style = field.Style,
                  
                      //Template =
                      //    String.Format("<p>{0}</p>",
                      //                  field.LiteralText.KillHtml()),
                      DisplayOrder = columnOrder,
                      ViewRoles = field.ViewRoles,
                      EditRoles = field.EditRoles,
                      User = this.User.Identity.Name,
                      UserRoles = userRoles
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
                      IsReadOnly = IsReadOnly,
                      listEvents = listEvent,
                      EventParent = eventParent,
                      isEmail = field.FormFieldType.Uid == Guid.Parse(GuidFormFieldTypeEmail) ? true : false,
                      ToolTip = field.ToolTip,
                      IsTriguerEventChange = _PageEvent.Count() > 0 ? true : false,
                      MinSize = field.MinSize,
                      //mathExpresion = _PageMathOperation == null ? string.Empty: _PageMathOperation.Expression,
                      eventName = eventName,
                      ValueWhenHideControl = _PageEvent.Count() > 0 ? _PageEvent[0].FieldValue : string.Empty,
                      IdControlToHide = _PageEvent.Count() > 0 ? "STPC_DFi_" + _PageEvent[0].ListenerFieldId : string.Empty,
                      Index = newIndexPanel.ToString() + columnOrder.ToString(),
                      IsNumber = (field.FormFieldType.FieldTypeName == FieldTypeNumericName ? true : false),
                      IsCurrency = (field.FormFieldType.FieldTypeName.Equals(FieldTypeCurrency) ? true : false),
                      IsText = (field.FormFieldType.FieldTypeName.Equals("Solo Texto") ? true : false),
                      MaxSize = !string.IsNullOrEmpty(field.MaxSize) ? field.MaxSize : field.MaxSizeBD,
                      IsTriggerField = isTrigger,
                      IsResponseField = isResponse,
                      PageStrategyUid = page.Uid,
                      Key = field.Uid.ToString(),
                      Value = value,
                      Style = field.Style,
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
                      IsRequeried =fieldIsRequeried,
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
                      UserRoles = userRoles
                    });
                    break;

                  #endregion

                  #region Calendar

                  case ("Calendar"):

                    if (string.IsNullOrEmpty(value))
                      dynamicFormFields.Add(new Calendar()
                      {
                        //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),
                        ToolTip = field.ToolTip,
                        IsReadOnly = IsReadOnly,
                        eventName = eventName,
                        IsTriguerEventChange = _PageEvent.Count() > 0 ? true : false,
                        ValueWhenHideControl = _PageEvent.Count() > 0 ? _PageEvent[0].FieldValue : string.Empty,
                        IdControlToHide = _PageEvent.Count() > 0 ? "STPC_DFi_" + _PageEvent[0].ListenerFieldId : string.Empty,
                        Index = newIndexPanel.ToString() + columnOrder.ToString(),
                        Key = field.Uid.ToString(),
                        Style = field.Style,
                        ResponseTitle = field.FormFieldName,
                        PanelColumn = field.PanelColumn,
                        PanelColumnSortOrder = field.PanelColumnSortOrder,
                        idStrategy = field.ValidationStrategyID,
                        IsTriggerField = isTrigger,
                        IsResponseField = isResponse,
                        PageStrategyUid = page.Uid,
                         IsRequeried =fieldIsRequeried,
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
                        UserRoles = userRoles
                      });
                    else
                    {
                      if (Convert.ToDateTime(value).ToShortDateString() == "01/01/1900" || Convert.ToDateTime(value).ToShortDateString() == "1/1/1900")
                        value = string.Empty;
                      else
                        value = DateTime.Parse(value).ToString(_dateFormat);
                      dynamicFormFields.Add(new Calendar()
                      {
                        //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),
                        Value = value,
                        IsReadOnly = IsReadOnly,
                        Style = field.Style,
                        eventName = eventName,
                        Index = newIndexPanel.ToString() + columnOrder.ToString(),
                        Key = field.Uid.ToString(),
                        ResponseTitle = field.FormFieldName,
                        PanelColumn = field.PanelColumn,
                        PanelColumnSortOrder = field.PanelColumnSortOrder,
                        idStrategy = field.ValidationStrategyID,
                        IsTriggerField = isTrigger,
                        IsResponseField = isResponse,
                        PageStrategyUid = page.Uid,
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
                        UserRoles = userRoles
                      });
                    }
                    break;
                  #endregion

                  #region TextArea

                  case ("TextArea"):
                    var newTextArea = new TextArea()
                    {
                      //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),
                      eventName = eventName,
                      ToolTip = field.ToolTip,
                      IsReadOnly = IsReadOnly,
                      Index = newIndexPanel.ToString() + columnOrder.ToString(),
                      MaxSize = !string.IsNullOrEmpty(field.MaxSize) ? field.MaxSize : field.MaxSizeBD,
                      IsTriggerField = isTrigger,
                      IsResponseField = isResponse,
                      PageStrategyUid = page.Uid,
                      Key = field.Uid.ToString(),
                      PanelColumn = field.PanelColumn,
                      PanelColumnSortOrder = field.PanelColumnSortOrder,
                      ResponseTitle = field.FormFieldName,
                      Value = value,
                      Style = field.Style,
                      Prompt =
                           (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                ? field.FormFieldPrompt
                                : null,
                      DisplayOrder = columnOrder,
                      ViewRoles = field.ViewRoles,
                      EditRoles = field.EditRoles,
                      User = this.User.Identity.Name,
                      UserRoles = userRoles,
                      // se cambia la propiedad de orden
                      //DisplayOrder = field.SortOrder
                      Required = Convert.ToBoolean(field.IsRequired),
                      IsRequeried = fieldIsRequeried,
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
                      ToolTip = field.ToolTip,
                      listEvents = listEvent,
                      IsReadOnly = IsReadOnly,
                      IsDependency = _PageEventListener.Count > 0 ? true : false,
                      IsTriguerEventChange = _PageEvent.Count() > 0 ? true : false,
                      ValueWhenHideControl = _PageEvent.Count() > 0 ? _PageEvent[0].EventType : string.Empty,
                      IdControlToHide = _PageEvent.Count() > 0 ? "STPC_DFi_" + _PageEvent[0].ListenerFieldId : string.Empty,
                      eventName = eventName,
                      Index = newIndexPanel.ToString() + columnOrder.ToString(),
                      PanelColumn = field.PanelColumn,
                      Style = field.Style,
                      PanelColumnSortOrder = field.PanelColumnSortOrder,
                      Key = field.Uid.ToString(),
                      ResponseTitle = field.FormFieldName,
                      IsTriggerField = isTrigger,
                      IsResponseField = isResponse,
                      IsRequeried = fieldIsRequeried,
                      PageStrategyUid = page.Uid,
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
                      UserRoles = userRoles
                    };

                    if (Convert.ToBoolean(field.IsMultipleSelect))
                    {
                      newSelectList.MultipleSelection = true;
                      newSelectList.Size = Convert.ToInt32(field.ListSize);
                      newSelectList.CommaDelimitedChoices = GetOptionsForSelector(field, requestId, false);
                    }
                    else
                    {
                      newSelectList.ShowEmptyOption = Convert.ToBoolean(field.IsEmptyOption);
                      newSelectList.EmptyOption = (Convert.ToBoolean(field.IsEmptyOption))
                                                                ? field.EmptyOption
                                                                : null;
                      if (value != string.Empty || _PageEventListenerHidden.Count > 0)
                      {
                        if (_PageEventListenerHidden.Count > 0)
                        {
                          //parentValue = GetValue(requestId, _PageEventListenerHidden[0].SourceField, _PageEventListenerHidden[0].FormPageUid, thisRequest.PageFlowId, formName);
                          //parentValue = dcParentValue.Where(e => e.Key == _PageEventListenerHidden[0].PageFieldUid).FirstOrDefault().;
                          if (dcParentValue != null)
                            if (dcParentValue.ContainsKey(_PageEventListenerHidden[0].PageFieldUid))
                            {
                              dcParentValue.TryGetValue(_PageEventListenerHidden[0].PageFieldUid, out parentValue);
                            }
                            else
                            {
                              PageField fieldParent = formFields.Where(e => e.Uid == _PageEventListenerHidden[0].PageFieldUid).FirstOrDefault();

                              if (fieldParent != null)
                              {
                                Panel _panel = _stpcForms.Panels.Expand("Page").Where(e => e.Uid == fieldParent.Panel.Uid).FirstOrDefault();
                                dataPageEvent = null;
                                parentValue = GetValue(requestId, _PageEventListenerHidden[0].SourceField, _panel.Page.Uid, thisRequest.PageFlowId, formName);
                                dataPageEvent = null;
                              }
                              else
                              {
                                parentValue = GetValue(requestId, _PageEventListenerHidden[0].SourceField, _PageEventListenerHidden[0].FormPageUid, thisRequest.PageFlowId, formName);
                              }
                            }
                          else
                            parentValue = GetValue(requestId, _PageEventListenerHidden[0].SourceField, _PageEventListenerHidden[0].FormPageUid, thisRequest.PageFlowId, formName);

                          if (parentValue != string.Empty)
                          {
                            string[] arrayGuid = parentValue.Split(',');
                            if (arrayGuid.Length > 1)
                              newSelectList.CommaDelimitedChoices = GetOptionsForCheckListSelector(field, parentValue);
                            else
                              newSelectList.CommaDelimitedChoices = GetOptionsForSelector(field, Convert.ToInt32(parentValue), true);
                          }
                          else
                            newSelectList.CommaDelimitedChoices = null;
                        }
                        else
                          newSelectList.CommaDelimitedChoices = GetOptionsForSelector(field, requestId, false);
                      }
                      else if (_PageEventListenerHidden.Count == 0)
                        newSelectList.CommaDelimitedChoices = GetOptionsForSelector(field, requestId, false);
                    }

                    foreach (var item in newSelectList.Choices)
                    {
                      bool re;
                      string[] arrayGuid = item.Value.Split('|');
                      //re = value.Contains(arrayGuid[0]);
                      re = (value == arrayGuid[0]);
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
                      ToolTip = field.ToolTip,
                      Style = field.Style,
                      IsReadOnly = IsReadOnly,
                      Checked = value == "True" ? true : false,
                      eventName = eventName,
                      IsTriguerEventChange = _PageEvent.Count() > 0 ? true : false,
                      ValueWhenHideControl = _PageEvent.Count() > 0 ? _PageEvent[0].FieldValue : string.Empty,
                      IdControlToHide = _PageEvent.Count() > 0 ? "STPC_DFi_" + _PageEvent[0].ListenerFieldId : string.Empty,
                      Index = newIndexPanel.ToString() + columnOrder.ToString(),
                      Key = field.Uid.ToString(),
                      PanelColumn = field.PanelColumn,
                      PanelColumnSortOrder = field.PanelColumnSortOrder,
                      IsTriggerField = isTrigger,
                      IsResponseField = isResponse,
                      PageStrategyUid = page.Uid,
                      ResponseTitle = field.FormFieldName,
                      Prompt =
                           (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                ? field.FormFieldPrompt
                                : null,
                      DisplayOrder = columnOrder,
                      // se cambia la propiedad de orden
                      //DisplayOrder = field.SortOrder
                      Required = Convert.ToBoolean(field.IsRequired),
                      IsRequeried = fieldIsRequeried,
                      RequiredMessage =
                           Convert.ToBoolean(field.IsRequired)
                                ? thisFieldType.ErrorMsgRequired.Replace(
                                      "%FormFieldName%", field.FormFieldName)
                                : string.Empty,
                      ViewRoles = field.ViewRoles,
                      EditRoles = field.EditRoles,
                      User = this.User.Identity.Name,
                      UserRoles = userRoles
                    };

                    dynamicFormFields.Add(newCheckBox);
                    break;
                  #endregion

                  #region CheckBoxList

                  case ("CheckBoxList"):
                    //Dictionary<string, string> lisControls = new Dictionary<string, string>();
                    string[] lisControls = new string[_PageEvent.Count];
                    int i = 0;

                    foreach (STPC.DynamicForms.Web.RT.Services.Entities.PageEvent item in _PageEvent)
                    {
                      //lisControls.Add(item.ListenerFieldId.ToString() + "|" + item.FieldValue, item.FieldValue);
                      lisControls[i] = item.ListenerFieldId.ToString();
                      i++;
                    }
                    var newCheckBoxList = new CheckBoxList
                    {
                      //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),
                      ToolTip = field.ToolTip,
                      IsReadOnly = IsReadOnly,
                      Style = field.Style,
                      listEvents = listEvent,
                      eventName = eventName,
                      IsTriguerEventChange = _PageEvent.Count() > 0 ? true : false,
                      ValueWhenHideControl = _PageEvent.Count() > 0 ? _PageEvent[0].FieldValue : string.Empty,
                      ListIdControlToHide = _PageEvent.Count() > 0 ? lisControls : null,
                      Index = newIndexPanel.ToString() + columnOrder.ToString(),
                      Key = field.Uid.ToString(),
                      ResponseTitle = field.FormFieldName,
                      PanelColumn = field.PanelColumn,
                      PanelColumnSortOrder = field.PanelColumnSortOrder,
                      IsTriggerField = isTrigger,
                      IsResponseField = isResponse,
                      PageStrategyUid = page.Uid,
                      Prompt =
                           (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                ? field.FormFieldPrompt
                                : null,
                      DisplayOrder = columnOrder,
                      // se cambia la propiedad de orden
                      //DisplayOrder = field.SortOrder
                      Required = Convert.ToBoolean(field.IsRequired),
                      IsRequeried = fieldIsRequeried,
                      RequiredMessage =
                           Convert.ToBoolean(field.IsRequired)
                                ? thisFieldType.ErrorMsgRequired.Replace(
                                      "%FormFieldName%", field.FormFieldName)
                                : string.Empty,
                      CommaDelimitedChoices = GetOptionsForSelector(field, requestId, false),
                      Orientation = (field.Orientation == "vertical") ? Orientation.Vertical : Orientation.Horizontal,
                      ViewRoles = field.ViewRoles,
                      EditRoles = field.EditRoles,
                      User = this.User.Identity.Name,
                      UserRoles = userRoles
                    };


                    newCheckBoxList.ShowEmptyOption = Convert.ToBoolean(field.IsEmptyOption);
                    newCheckBoxList.EmptyOption = (Convert.ToBoolean(field.IsEmptyOption))
                                                              ? field.EmptyOption
                                                              : null;
                    if (value != string.Empty || _PageEventListenerHidden.Count > 0)
                    {
                      if (_PageEventListenerHidden.Count > 0)
                      {
                        //parentValue = GetValue(requestId, _PageEventListenerHidden[0].SourceField, _PageEventListenerHidden[0].FormPageUid, thisRequest.PageFlowId, formName);
                        //parentValue = dcParentValue.Where(e => e.Key == _PageEventListenerHidden[0].PageFieldUid).FirstOrDefault().;
                        if (dcParentValue != null)
                          if (dcParentValue.ContainsKey(_PageEventListenerHidden[0].PageFieldUid))
                          {
                            dcParentValue.TryGetValue(_PageEventListenerHidden[0].PageFieldUid, out parentValue);
                          }
                          else
                          {
                            PageField fieldParent = _stpcForms.PageFields.Expand("Panel").Where(e => e.Uid == _PageEventListenerHidden[0].PageFieldUid).FirstOrDefault();
                            Panel _panel = _stpcForms.Panels.Expand("Page").Where(e => e.Uid == fieldParent.Panel.Uid).FirstOrDefault();

                            parentValue = GetValue(requestId, _PageEventListenerHidden[0].SourceField, _panel.Page.Uid, thisRequest.PageFlowId, formName);
                          }
                        else
                          parentValue = GetValue(requestId, _PageEventListenerHidden[0].SourceField, _PageEventListenerHidden[0].FormPageUid, thisRequest.PageFlowId, formName);


                        if (parentValue != string.Empty)
                        {
                          string[] arrayGuid = parentValue.Split(',');
                          if (arrayGuid.Length > 1)
                            newCheckBoxList.CommaDelimitedChoices = GetOptionsForCheckListSelector(field, parentValue);
                          else
                            newCheckBoxList.CommaDelimitedChoices = GetOptionsForSelector(field, Convert.ToInt32(parentValue), true);
                        }
                        else
                          newCheckBoxList.CommaDelimitedChoices = null;
                      }
                      else
                        newCheckBoxList.CommaDelimitedChoices = GetOptionsForSelector(field, requestId, false);
                    }
                    else if (_PageEventListenerHidden.Count == 0)
                      newCheckBoxList.CommaDelimitedChoices = GetOptionsForSelector(field, requestId, false);

                    foreach (var item in newCheckBoxList.Choices)
                    {
                      string[] arrayGuid = item.Value.Split('|');
                      //re = value.Contains(arrayGuid[0]);
                      if (value.Contains(arrayGuid[0]))
                        item.Selected = true;
                    }

                    dynamicFormFields.Add(newCheckBoxList);
                    break;
                  #endregion

                  #region RadioList

                  case ("RadioList"):


                    //Dictionary<string, string> lisControls = new Dictionary<string, string>();
                    string[] lisControlsRadio = new string[_PageEvent.Count];
                    int j = 0;

                    foreach (STPC.DynamicForms.Web.RT.Services.Entities.PageEvent item in _PageEvent)
                    {
                      //lisControls.Add(item.ListenerFieldId.ToString() + "|" + item.FieldValue, item.FieldValue);
                      lisControlsRadio[j] = item.ListenerFieldId.ToString();
                      j++;
                    }
                    var newRadioList = new RadioList
                    {
                      //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),
                      ToolTip = field.ToolTip,
                      IsReadOnly = IsReadOnly,
                      Style = field.Style,
                      listEvents = listEvent,
                      eventName = eventName,
                      IsTriguerEventChange = _PageEvent.Count() > 0 ? true : false,
                      ValueWhenHideControl = _PageEvent.Count() > 0 ? _PageEvent[0].EventType : string.Empty,
                      ListIdControlToHide = _PageEvent.Count() > 0 ? lisControlsRadio : null,
                      IdControlToHide = _PageEvent.Count() > 0 ? "STPC_DFi_" + _PageEvent[0].ListenerFieldId : string.Empty,
                      Index = newIndexPanel.ToString() + columnOrder.ToString(),
                      Key = field.Uid.ToString(),
                      ResponseTitle = field.FormFieldName,
                      PanelColumn = field.PanelColumn,
                      PanelColumnSortOrder = field.PanelColumnSortOrder,
                      IsTriggerField = isTrigger,
                      IsResponseField = isResponse,
                      PageStrategyUid = page.Uid,
                      Prompt =
                           (!string.IsNullOrEmpty(field.FormFieldPrompt))
                                ? field.FormFieldPrompt
                                : null,
                      DisplayOrder = columnOrder,
                      // se cambia la propiedad de orden
                      //DisplayOrder = field.SortOrder
                      Required = Convert.ToBoolean(field.IsRequired),
                      IsRequeried = fieldIsRequeried,
                      RequiredMessage =
                           Convert.ToBoolean(field.IsRequired)
                                ? thisFieldType.ErrorMsgRequired.Replace(
                                      "%FormFieldName%", field.FormFieldName)
                                : string.Empty,
                      CommaDelimitedChoices = GetOptionsForSelector(field, requestId, false),
                      Orientation = (field.Orientation == "vertical") ? Orientation.Vertical : Orientation.Horizontal,
                      ViewRoles = field.ViewRoles,
                      EditRoles = field.EditRoles,
                      User = this.User.Identity.Name,
                      UserRoles = userRoles
                    };

                    newRadioList.ShowEmptyOption = Convert.ToBoolean(field.IsEmptyOption);
                    newRadioList.EmptyOption = (Convert.ToBoolean(field.IsEmptyOption))
                                                              ? field.EmptyOption
                                                              : null;
                    if (value != string.Empty || _PageEventListenerHidden.Count > 0)
                    {
                      if (_PageEventListenerHidden.Count > 0)
                      {

                        //parentValue = GetValue(requestId, _PageEventListenerHidden[0].SourceField, _PageEventListenerHidden[0].FormPageUid, thisRequest.PageFlowId, formName);
                        //parentValue = dcParentValue.Where(e => e.Key == _PageEventListenerHidden[0].PageFieldUid).FirstOrDefault().;
                        if (dcParentValue != null)
                          if (dcParentValue.ContainsKey(_PageEventListenerHidden[0].PageFieldUid))
                          {
                            dcParentValue.TryGetValue(_PageEventListenerHidden[0].PageFieldUid, out parentValue);
                          }
                          else
                          {
                            PageField fieldParent = _stpcForms.PageFields.Expand("Panel").Where(e => e.Uid == _PageEventListenerHidden[0].PageFieldUid).FirstOrDefault();
                            Panel _panel = _stpcForms.Panels.Expand("Page").Where(e => e.Uid == fieldParent.Panel.Uid).FirstOrDefault();

                            parentValue = GetValue(requestId, _PageEventListenerHidden[0].SourceField, _panel.Page.Uid, thisRequest.PageFlowId, formName);
                          }
                        else
                          parentValue = GetValue(requestId, _PageEventListenerHidden[0].SourceField, _PageEventListenerHidden[0].FormPageUid, thisRequest.PageFlowId, formName);

                        if (parentValue != string.Empty)
                        {
                          string[] arrayGuid = parentValue.Split(',');
                          if (arrayGuid.Length > 1)
                            newRadioList.CommaDelimitedChoices = GetOptionsForCheckListSelector(field, parentValue);
                          else
                            newRadioList.CommaDelimitedChoices = GetOptionsForSelector(field, Convert.ToInt32(parentValue), true);
                        }
                        else
                          newRadioList.CommaDelimitedChoices = null;
                      }
                      else
                        newRadioList.CommaDelimitedChoices = GetOptionsForSelector(field, requestId, false);
                    }
                    else if (_PageEventListenerHidden.Count == 0)
                      newRadioList.CommaDelimitedChoices = GetOptionsForSelector(field, requestId, false);

                    foreach (var item in newRadioList.Choices)
                    {
                      string[] arrayGuid = item.Value.Split('|');
                      //re = value.Contains(arrayGuid[0]);
                      if (arrayGuid[0] == value)
                        item.Selected = true;
                    }
                    dynamicFormFields.Add(newRadioList);
                    break;
                  #endregion

                  #region FileUpload

                  case ("FileUpload"):
                    var newFileUpload = new FileUpload
                    {
                      ToolTip = field.ToolTip,
                      eventName = eventName,
                      //Index = newIndexPanel.ToString() + field.PanelColumnSortOrder.ToString(),
                      IsReadOnly = IsReadOnly,
                      FileId = value,
                      Index = newIndexPanel.ToString() + columnOrder.ToString(),
                      UriFilePath = (value != string.Empty ? "/FormPage/GetPrivateResource?resource=" + value : string.Empty),
                      MaxSize = !string.IsNullOrEmpty(field.MaxSize) ? field.MaxSize : field.MaxSizeBD,
                      IsTriggerField = isTrigger,
                      Required = Convert.ToBoolean(field.IsRequired),
                      IsRequeried = fieldIsRequeried,
                      IsResponseField = isResponse,
                      PageStrategyUid = page.Uid,
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
                      RequiredMessage =
                           Convert.ToBoolean(field.IsRequired)
                                ? thisFieldType.ErrorMsgRequired.Replace(
                                      "%FormFieldName%", field.FormFieldName)
                                : string.Empty,
                      DisplayOrder = columnOrder,
                      ViewRoles = field.ViewRoles,
                      EditRoles = field.EditRoles,
                      User = this.User.Identity.Name,
                      UserRoles = userRoles
                      // se cambia la propiedad de orden
                      //DisplayOrder = field.SortOrder
                    };

                    //if (!string.IsNullOrEmpty(field.ValidExtensions))
                    //	newFileUpload.ValidExtensions = "." +
                    //											  field.ValidExtensions.Replace(Environment.NewLine,
                    //																					  ",.");
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
                      IsReadOnly = IsReadOnly,
                      Index = newIndexPanel.ToString() + columnOrder.ToString(),
                      Key = field.Uid.ToString(),
                      PanelColumn = field.PanelColumn,
                      PanelColumnSortOrder = field.PanelColumnSortOrder,
                      IsTriggerField = isTrigger,
                      IsResponseField = isResponse,
                      PageStrategyUid = page.Uid,
                      //Template = String.Format("<a href={0}>{1}</a>", "\"http://" + field.LiteralText + "\"", field.FormFieldName),
                      DisplayOrder = columnOrder,
                      Text = field.FormFieldName,
                      Target = field.LiteralText,
                      Display = true,
                      ViewRoles = field.ViewRoles,
                      EditRoles = field.EditRoles,
                      User = this.User.Identity.Name,
                      UserRoles = userRoles
                    });
                    break;
                  #endregion

                  #region Blank

                  case ("Blank"):
                    dynamicFormFields.Add(new Blank()
                    {
                      IsReadOnly = IsReadOnly,
                      Index = newIndexPanel.ToString() + columnOrder.ToString(),
                      IsTriggerField = isTrigger,
                      IsResponseField = isResponse,
                      PageStrategyUid = page.Uid,
                      Key = field.Uid.ToString(),
                      PanelColumn = field.PanelColumn,
                      PanelColumnSortOrder = field.PanelColumnSortOrder,
                      DisplayOrder = columnOrder,
                      User = this.User.Identity.Name,
                    });
                    break;

                  #endregion

                  #region Image

                  case ("Image"):
                    dynamicFormFields.Add(new Image()
                    {
                      IsReadOnly = IsReadOnly,
                      Index = newIndexPanel.ToString() + columnOrder.ToString(),
                      IsTriggerField = isTrigger,
                      IsResponseField = isResponse,
                      Style = field.Style,
                      imageUrl = urlImageState,
                      PageStrategyUid = page.Uid,
                      Key = field.Uid.ToString(),
                      PanelColumn = field.PanelColumn,
                      PanelColumnSortOrder = field.PanelColumnSortOrder,
                      DisplayOrder = columnOrder,
                      User = this.User.Identity.Name,
                    });
                    break;

                  #endregion

                }
                #endregion tipos de control

                columnOrder++;
              }


              #endregion validar si contiene rol de visualizacion
            }

            #endregion navigate formFields

#if DEBUG
            System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "PC-Respond-forEachFormFields", DateTime.Now.Subtract(refTime).Milliseconds);
            refTime = DateTime.Now;
#endif

            var dynamicForm = new global::STPC.DynamicForms.Core.Form();
            dynamicForm.AddFields(dynamicFormFields.ToArray());
            dynamicForm.Serialize = true;
            dynamicForm.NumColumnsPanel = thisPanel.Columns;
            dynamicForm.PanelId = thisPanel.Uid;
            dynamicForm.panelCaption = thisPanel.Name;
            dynamicForm.DivCssStyle = thisPanel.DivCssStyle;
            dynamicForm.isDisabled = (eventNamePanel == "Disabled" ? true : false);
            dynamicForm.isHiden = (eventNamePanel == "Hide" ? true : false);

            /////////////////////////////////
            //Se adiciona parametro en el formulario para cargar si es un panel detalle.
            //Por: Jorge Alonso
            //Fecha:2016-09-17
            /////////////////////////////////
            dynamicForm.panelType = thisPanel.Type;
            dynamicForm.panelEditRol = (eventNamePanel == "Disabled" ? "true" : "false");
            /////////////////////////////////     

            listdynamicForm.Add(dynamicForm);
#if DEBUG
            System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "PC-Respond-BeforeReturn", DateTime.Now.Subtract(refTime).Milliseconds);
            refTime = DateTime.Now;
#endif

          }

          #endregion Validar si el panel contiene campos

          #region panel vacio

          else
          {
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
            if (listdynamicForm != null)
              listdynamicForm.Add(emptyForm);
            //return PartialView(emptyForm);
          }

          #endregion panel vacio

            /////////////////////////////////
          //Se Identifica si es un panel detalle y se cargan los datos del panel.
          //Por: Jorge Alonso
          //Fecha:2016-09-17
          /////////////////////////////////
          if (panel.Type == "Detalle")
          {
              ViewBag.Panel = panel;
              ViewBag.PrincipalData = data;
              ViewBag.FieldsList = formFields;
              ViewBag.Results = data;
              ViewBag.RequestId = requestId;
              ViewBag.TableName = panel.TableDetailName;
          }
            /////////////////////////////////

        }
        return PartialView(listdynamicForm);
      }
      catch (Exception ex)
      {
        #region Validacion de errores

        bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
        Guid correlationID = Guid.NewGuid();

        ViewBag.correlationID = correlationID;

        // enmascarar la excepcion
        #region Exception, No existe store procedure

        if (ex.Message.Contains(CustomMessages.EX0001))
        {
          ILogging eventWriter = LoggingFactory.GetInstance();
          string errorMessage = string.Format(CustomMessages.E0002, ex.Message.Split(' ')[ex.Message.Split(' ').Length - 1], "PanelController", "Respond", correlationID, ex.Message);
          eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

          if (ShowErrorDetail)
            return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(string.Format(CustomMessages.E0002, ex.Message.Split(' ')[ex.Message.Split(' ').Length - 1])), "Panel Controller", "Respond"));

          return PartialView("_ErrorGeneral", CustomMessages.E0001 + "\n" + "Código del error: " + correlationID.ToString());
        }

        #endregion Exception, No existe store procedure

        #region Exception, No existe tabla

        else if (ex.Message.Contains(CustomMessages.EX0002))
        {
          ILogging eventWriter = LoggingFactory.GetInstance();
          string errorMessage = string.Format(CustomMessages.E0002, ex.Message.Split(' ')[ex.Message.Split(' ').Length - 1], "PanelController", "Respond", correlationID, ex.Message);
          eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

          if (ShowErrorDetail)
            return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(string.Format(CustomMessages.E0002, ex.Message.Split(' ')[ex.Message.Split(' ').Length - 1])), "Panel Controller", "Respond"));

          return PartialView("_ErrorGeneral", CustomMessages.E0001 + "\n" + "Código del error: " + correlationID.ToString());
        }

        #endregion Exception, No existe tabla

        #region Excepcion no clasificada

        else
        {
          ILogging eventWriter = LoggingFactory.GetInstance();
          string errorMessage = string.Format(CustomMessages.E0007, "Panel Controller", "Respond", correlationID, ex.Message);
          eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));// crear registro de error en el visor de sucesos

          if (ShowErrorDetail)
            return PartialView("_ErrorDetail", new HandleErrorInfo(ex, "Panel Controller", "Respond"));

          return PartialView("_ErrorGeneral", CustomMessages.E0001 + "\n" + "Código del error: " + correlationID.ToString());
        }

        #endregion Excepcion no clasificada

        #endregion Validacion de errores
      }
    }


    public ActionResult Respond_dt(Guid id)
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

              if (String.IsNullOrEmpty(field.Style))
                field.Style = string.Empty;

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
                    Style = field.Style,
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
                      Style = field.Style,
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
                    Style = field.Style,
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
                    Style = field.Style,
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
                    Style = field.Style,
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
                    Style = field.Style,
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
                    Style = field.Style,
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
                    Style = field.Style,
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
                    Style = field.Style,
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

    private string GetOptionsForSelector(PageField field)
    {
      var TheCategory = _stpcForms.Categories.Where(n => n.Uid == Convert.ToInt32(field.OptionsCategoryName) && n.IsActive == true).FirstOrDefault();

      if (TheCategory != null)
      {
        var optionList = _stpcForms.Options.Where(c => c.Category_Uid == TheCategory.Uid).ToList();
        return GetOptionsForSelector(field, 200, false); //What is the Value? 200?
      }
      else
        return string.Empty;

    }

    private bool ValidatePageReadOnlyState(Guid? RequestWorkFlowState,
        Guid? FormPageReadOnlyState, Common.Services.Users.User user, Guid _formPageUid)
    {
      bool IsReadOnlyRole = false;
      string LockedRoleName = System.Configuration.ConfigurationManager.AppSettings["LockedRole"];

      if (!string.IsNullOrEmpty(LockedRoleName) && user != null)
      {
        foreach (var item in user.Roles)
        {
          if (item.Rolename.Equals(LockedRoleName))
          {
            IsReadOnlyRole = true;
            break;
          }
        }
      }

      bool IsReadOnlyState = false;

      if (FormPageReadOnlyState.HasValue && RequestWorkFlowState.HasValue)
        IsReadOnlyState = !RequestWorkFlowState.Equals(FormPageReadOnlyState);


      return IsReadOnlyRole || IsReadOnlyState || GetFormPageByStates(RequestWorkFlowState, _formPageUid) ? true : false;
    }

    private bool GetFormPageByStates(Guid? RequestWorkFlowState, Guid _formPageUid)
    {
      if (RequestWorkFlowState == null)
      {
        Guid newGuid = new Guid();
        RequestWorkFlowState = newGuid;
      }
      FormPageByStates _FormPageByStates = _stpcForms.FormPageByStates.Where(uid => uid.FormStatesUid ==
          RequestWorkFlowState && uid.FormPageUid == _formPageUid).FirstOrDefault();

      return _FormPageByStates == null ? true : false;
    }


    public ActionResult UpdateStateControlsByRole(FormCollection campos)
    {
      FormCollection par = new FormCollection();
      for (int i = 0; i < campos.Count - 1; i = i + 2)
      {
        par.Add(campos[i], campos[i + 1]);
      }

      Guid formPageId = Guid.Parse(par["FormPageUid"]);
      ViewData["RequestId"] = par["RequestId"];

      RequestServiceClient _RequestServiceClient = new RequestServiceClient();
      var thisRequest = _RequestServiceClient.GetRequestById(Convert.ToInt32(par["RequestId"]));
      var thisFormPage = _stpcForms.FormPages.Where(x => x.Uid == thisRequest.PageFlowId).FirstOrDefault();
      var formFields = new List<PageField>();
      var currentUser = provider.GetUser(User.Identity.Name);
      bool IsReadOnlyState = ValidatePageReadOnlyState(thisRequest.WorkFlowState, thisFormPage.ReadOnlyState, currentUser, thisRequest.PageFlowId);

      return Json(IsReadOnlyState, JsonRequestBehavior.AllowGet);
    }


    private string GetOptionsForSelector(PageField field, int value, bool isParent)
    {
      if (field == null) throw new ArgumentException("Objeto field nulo");
      if (field.OptionsMode == "local")
      {
        STPC.DynamicForms.Web.Common.Services.Users.User currentUser = provider.GetUser(User.Identity.Name);

        //var TheCategory = IsNewRequest == false ? _stpcForms.Categories.Where(n => n.Uid == Convert.ToInt32(field.OptionsCategoryName) && (n.AplicationNameId==currentUser.AplicationNameId || n.AplicationNameId==null)).FirstOrDefault() : _stpcForms.Categories.Where(n => n.Uid == Convert.ToInt32(field.OptionsCategoryName) && n.IsActive == true).FirstOrDefault();
        var TheCategory = IsNewRequest == false ? _stpcForms.Categories.Where(n => n.Uid == Convert.ToInt32(field.OptionsCategoryName)).FirstOrDefault() : _stpcForms.Categories.Where(n => n.Uid == Convert.ToInt32(field.OptionsCategoryName) && n.IsActive == true).FirstOrDefault();

        List<STPC.DynamicForms.Web.RT.Services.Entities.Option> TheOptions;

        if (TheCategory != null)
        {
          if (isParent) TheOptions = IsNewRequest == false ? _stpcForms.Options.Where(c => c.Category_Uid == TheCategory.Uid && c.Option_Uid_Parent == value).OrderBy(e => e.Value).ToList() : _stpcForms.Options.Where(c => c.Category_Uid == TheCategory.Uid && c.Option_Uid_Parent == value && c.IsActive == true).OrderBy(e => e.Value).ToList();
          else TheOptions = IsNewRequest == false ? _stpcForms.Options.Where(c => c.Category_Uid == TheCategory.Uid && c.IsActive == true).OrderBy(e => e.Value).ToList() : _stpcForms.Options.Where(c => c.Category_Uid == TheCategory.Uid && c.IsActive == true).OrderBy(e => e.Value).ToList();

          StringBuilder TheOptionsCommaSeparated = new StringBuilder();
          foreach (var option in TheOptions)
          {
            TheOptionsCommaSeparated.Append(option.Uid + "|" + option.Value + "|" + option.IsActive.ToString());
            TheOptionsCommaSeparated.Append(',');
          }
          if (TheOptionsCommaSeparated.Length > 0)
            TheOptionsCommaSeparated.Remove(TheOptionsCommaSeparated.Length - 1, 1);
          return TheOptionsCommaSeparated.ToString();
        }
        return string.Empty;
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

        List<string> valuesStrategie = LoadCacheStrategies(strategyId);
        IList<string> options;
        if (valuesStrategie == null)
        {
          options = _decisionEngine.GetList(strategyId, "requestId", value.ToString(), "lista", this.User.Identity.Name);
          AddValuesStrategiesCache(options.ToList(), strategyId);
        }
        else
        {
          options = valuesStrategie;
        }

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

    private void AddValuesStrategiesCache(List<String> values, int idStrategie)
    {
      if (_ListStrategiesCache == null)
      {
        _ListStrategiesCache = new List<ListStrategiesCache>();

      }

      ListStrategiesCache newListStrategiesCache = new ListStrategiesCache();
      newListStrategiesCache.StrategieID = idStrategie;
      newListStrategiesCache.ListValues = values;
      _ListStrategiesCache.Add(newListStrategiesCache);

    }
    private List<string> LoadCacheStrategies(int strategyId)
    {
      ListStrategiesCache _listStrategiesCache = _ListStrategiesCache.Where(e => e.StrategieID == strategyId).FirstOrDefault();

      if (_listStrategiesCache != null)
      {
        return _listStrategiesCache.ListValues;
      }
      else
      {

        return null;
      }


    }

    private string GetOptionsForCheckListSelector(PageField field, string Value)
    {
      string[] arrayGuid = Value.Split(',');

      if (field == null) throw new ArgumentException("Objeto field nulo");
      if (field.OptionsMode == "local")
      {
        var TheCategory = _stpcForms.Categories.Where(n => n.Uid == Convert.ToInt32(field.OptionsCategoryName) && n.IsActive == true).FirstOrDefault();
        if (TheCategory != null)
        {
          //var TheOptions = _stpcForms.Options.Where(c => c.Category_Uid == TheCategory.Uid && c.Option_Uid_Parent.ToString() == Value).ToList();

          var TheOptions = from x in _stpcForms.Options.ToList() where arrayGuid.Contains(x.Option_Uid_Parent.ToString()) && x.Category_Uid == TheCategory.Uid && x.IsActive == true select x;


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
        else return string.Empty;
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
        var options = _decisionEngine.GetList(strategyId, string.Empty, string.Empty, string.Empty, this.User.Identity.Name);
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
        var thePanel = _stpcForms.Panels.Expand("Page").Expand("PanelFields").Where(i => i.Uid == viewModel.panel.Uid).FirstOrDefault();
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
            //if (double.Parse(string.IsNullOrEmpty(newMaxSize) ? "0" : newMaxSize) > double.Parse(string.IsNullOrEmpty(newMaxSizeBD) ? "0" : newMaxSizeBD))
            //	return View("Error");

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
            _stpcForms.SaveChanges();
          }

          #endregion Campo nuevo

          currentField++;
          sortOrderDT++;

        }

        #endregion almacenar los campos del panel

        thePanel.Name = viewModel.panel.Name;
        thePanel.DivCssStyle = viewModel.panel.DivCssStyle;
        thePanel.Description = viewModel.panel.Description;
        _stpcForms.UpdateObject(thePanel);
        _stpcForms.SaveChanges();

        // se modifica esta linea para que regrese al listado de columnas
        //return RedirectToAction("GetColumns", "Panel", new { id = thePanel.Uid });

        return getListControls(thePanel.Uid, int.Parse(collection["colunmNumber"]));
        //return PartialView("Edit", viewModel);
      }
      catch (Exception ex)
      {

        throw;
      }
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
        return GetListPanels(thePanel.Page.Uid);
      }
      return Json(new { Success = false });
    }

    [HttpPost]
    public ActionResult Create(string name, string desc, int columns, string stylesheet, Guid formpageId)
    {

      _stpcForms.MergeOption = System.Data.Services.Client.MergeOption.PreserveChanges;
      var theFormPage = _stpcForms.FormPages.Expand("Panels").Where(i => i.Uid == formpageId).FirstOrDefault();
      int theOrder;
      if (theFormPage.Panels.Count == 0) theOrder = 0;
      else
        theOrder = theFormPage.Panels.Select(m => m.SortOrder).Max() + 1;
      var theNewPanel = new STPC.DynamicForms.Web.RT.Services.Entities.Panel
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
          STPC.DynamicForms.Web.RT.Services.Entities.Panel addedPanel = descriptor.Entity as STPC.DynamicForms.Web.RT.Services.Entities.Panel;

          if (addedPanel != null)
          {
            newPanelid = addedPanel.Uid;
          }
        }
      }
      return GetListPanels(theFormPage.Uid);
      //return Json(new { success = true, Name = name, Desc = desc, Columns = columns, CssStyle = stylesheet, uid = newPanelid });
    }

    protected List<SelectListItem> GetFormFieldTypes(bool includeEmptyChoice = false)
    {
      var formFieldTypeList = new List<SelectListItem>();
      if (includeEmptyChoice)
        formFieldTypeList.Add(new SelectListItem { Selected = true, Text = "--Select a Field Type--", Value = string.Empty });
      //Retain the foreach since a LINQ statement bombs on the .ToString for Uid
      foreach (var formFieldType in _stpcForms.PageFieldTypes.OrderBy(formfield => formfield.SortOrder))
      {
        formFieldTypeList.Add(new SelectListItem { Text = formFieldType.FieldTypeName, Value = formFieldType.Uid.ToString() });
      }
      return formFieldTypeList;
    }

    public string GetValue(int requestId, string formFieldName, Guid formPageId, Guid PageFlowId, string formName)
    {
      string result2 = string.Empty;
      string prefixFields = STPC.DynamicForms.Core.Constants.PREFIX_DYNAMIC_FIELD;
      Values result = new Values();


      if (listRequestRepository == null)
      {
        //if (dataPageEvent == null)

        dataPageEvent = requestProvider.GetPageFlowStepInstance(requestId, formPageId, PageFlowId, formName);
        setDataPageEvent(requestId, formFieldName, formPageId, PageFlowId, formName);

      }
      else
      {
        RequestRepository _RequestRepository = listRequestRepository.Where(e => e.requestId == requestId && e.formPageId == formPageId && e.PageFlowId == PageFlowId && e.formName == formName).FirstOrDefault();
        if (_RequestRepository != null)
        {
          dataPageEvent = _RequestRepository.dataPageEvent;
        }
        else
        {
          dataPageEvent = requestProvider.GetPageFlowStepInstance(requestId, formPageId, PageFlowId, formName);
          setDataPageEvent(requestId, formFieldName, formPageId, PageFlowId, formName);
        }

      }
      if (dataPageEvent.Any())
      {
        result = (from r in dataPageEvent
                  where r.key == prefixFields + formFieldName
                  select r).FirstOrDefault();
      }

      if (result != null)
        result2 = result.value;

      return string.IsNullOrEmpty(result2) ? string.Empty : result2;
    }

    private void setDataPageEvent(int requestId, string formFieldName, Guid formPageId, Guid PageFlowId, string formName)
    {
      if (listRequestRepository == null)
        listRequestRepository = new List<RequestRepository>();

      RequestRepository _RequestRepository = new RequestRepository();
      _RequestRepository.requestId = requestId;
      _RequestRepository.formFieldName = formFieldName;
      _RequestRepository.formPageId = formPageId;
      _RequestRepository.PageFlowId = PageFlowId;
      _RequestRepository.formName = formName;
      _RequestRepository.dataPageEvent = dataPageEvent;
      listRequestRepository.Add(_RequestRepository);
    }

    public string GetValue(List<Values> data, string formFieldName)
    {
      string result2 = string.Empty;
      string prefixFields = STPC.DynamicForms.Core.Constants.PREFIX_DYNAMIC_FIELD;
      Values result = new Values();


      if (data.Any())
      {
        result = (from r in data
                  where r.key == prefixFields + formFieldName
                  select r).FirstOrDefault();
      }

      if (result != null)
        result2 = result.value;

      return string.IsNullOrEmpty(result2) ? string.Empty : result2;
    }

    [HttpPost]
    [ValidateAntiForgeryTokenAttribute]
    public ActionResult GetColumns(Guid id)
    {
      return listColumn(id);
    }

    private ActionResult listColumn(Guid id)
    {
      var item = _stpcForms.Panels.Expand("PanelFields").Expand("Page").Where((x => x.Uid == id)).FirstOrDefault();

      ViewBag.CategoriesSelect = new List<SelectListItem>();
      foreach (var category in _stpcForms.Categories.Where(cat => cat.IsActive == true).OrderBy(e => e.Name))
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
      var availableRoles = Roles.GetAllRoles();
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
            AvailableRoles = availableRoles
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
      var editPanel = new FormFieldViewModel
      {
        AvailableRoles = availableRoles,
        SelectedEditRoles = new List<string>(),
        SelectedViewRoles = new List<string>(),
        ViewRoles = item.ViewRoles != null ? item.ViewRoles : string.Empty,
        EditRoles = item.EditRoles != null ? item.EditRoles : string.Empty,
      };

      ProcesarRoles(editPanel);
      viewModel.SelectedEditRoles = editPanel.SelectedEditRoles;
      viewModel.SelectedViewRoles = editPanel.SelectedViewRoles;
      return PartialView("GetColumns", viewModel);
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

        //Borra eventos relacionados con el control que se está borrando
        List<STPC.DynamicForms.Web.RT.Services.Entities.PageEvent> _PageEvent = _stpcForms.PageEvent.Where(pf => pf.PageFieldUid == formfieldUid || pf.ListenerFieldId == formfieldUid).ToList();

        foreach (var item in _PageEvent)
        {
          _stpcForms.DeleteObject(item);
          _stpcForms.SaveChanges();
        }
        return getListControls(panelUid, field.PanelColumn);


      }
      catch (Exception Ex)
      {
        string error = Ex.Message;

        return RedirectToAction("Edit", new { id = panelUid, colunmNumber = 0 });
      }

    }

    [HttpPost]
    public ActionResult UpdateRolesPanel(string EditRoles, string ViewRoles, Guid idPanel)
    {
      Panel panel = _stpcForms.Panels.Where(e => e.Uid == idPanel).FirstOrDefault();

      if (panel != null)
      {
        panel.ViewRoles = ViewRoles;
        panel.EditRoles = EditRoles;
        _stpcForms.UpdateObject(panel);
        _stpcForms.SaveChanges();
      }
      return Json(new { Success = true });
    }
  }
}