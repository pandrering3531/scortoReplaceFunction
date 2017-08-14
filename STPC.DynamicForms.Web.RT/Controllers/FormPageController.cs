using STPC.DynamicForms.DecisionEngine;
using STPC.DynamicForms.Infraestructure.Logging;
using STPC.DynamicForms.Web.Common;
using STPC.DynamicForms.Web.Common.Messages;
using STPC.DynamicForms.Web.RT.Helpers;
using STPC.DynamicForms.Web.RT.Services.Entities;
using STPC.DynamicForms.Web.RT.Services.Request;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using System.Net;
using System.Collections;
using System.Web.Helpers;
using Newtonsoft.Json;

namespace STPC.DynamicForms.Web.RT.Controllers
{
  [Authorize]


  public class FormPageController : Controller
  {
    CustomMembershipProvider provider = (CustomMembershipProvider)Membership.Provider;
    public AbcRedisCacheManager _AbcRedisCacheManager = new AbcRedisCacheManager();
    Services.Entities.STPC_FormsFormEntities _stpcForms;
    CustomRequestProvider requestProvider = new CustomRequestProvider();
    private IDecisionEngine _decisionEngine;


    public FormPageController(IDecisionEngine decisionEngine)
    {
      _decisionEngine = decisionEngine;
      _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));

    }

    public FormPageController()
    {
      string user = System.Configuration.ConfigurationManager.AppSettings["UserWs"];
      string pws = System.Configuration.ConfigurationManager.AppSettings["PwsWs"];
      string pollInterval = System.Configuration.ConfigurationManager.AppSettings["pollInterval"];
      string timeOut = System.Configuration.ConfigurationManager.AppSettings["timeOut"];
      this._decisionEngine = new STPC.DynamicForms.DecisionEngine.DecisionEngine(user, pws, Convert.ToInt32(pollInterval), Convert.ToInt32(timeOut));
      _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
      _stpcForms.IgnoreResourceNotFoundException = true;
      //throw new Exception();
    }


    private List<FormPageActions> ValidateButtonsPermissions(Guid pageId, int requestId)
    {
      List<FormPageActions> listActionsByState = new List<FormPageActions>();
      try
      {
        var request = _stpcForms.Request.Where(e => e.RequestId == requestId).FirstOrDefault();

        if (request.WorkFlowState == null)
          request.WorkFlowState = Guid.Empty;
        List<Common.Services.Users.FormPageActions> listActionsByStateResult = requestProvider.GetUserFormPageActionsByState(this.User.Identity.Name, pageId, (Guid)request.WorkFlowState);
        FormPageActions value;
        foreach (Common.Services.Users.FormPageActions item in listActionsByStateResult)
        {
          value = new FormPageActions()
          {
            Uid = item.Uid,
            Name = item.Name,
            Description = item.Description,
            Caption = item.Caption,
            IsAssociated = item.IsAssociated,
            IsExecuteStrategy = item.IsExecuteStrategy,
            PageId = item.PageId,
            DisplayOrder = item.DisplayOrder,
            Save = item.Save,
            GoToPageId = item.GoToPageId,
            FormStatesUid = item.FormStatesUid,
            ShowSuccessMessage = item.ShowSuccessMessage,
            ShowFailureMessage = item.ShowFailureMessage,
            SuccessMessage = item.SuccessMessage,
            FailureMessage = item.FailureMessage,
            StrategyID = item.StrategyID,
          };
          listActionsByState.Add(value);
        }
        return listActionsByState;
      }
      catch (Exception ex)
      {
        bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
        Guid correlationID = Guid.NewGuid();

        ILogging eventWriter = LoggingFactory.GetInstance();
        string errorMessage = string.Format(CustomMessages.E0007, "FormPageController", "ValidateButtonsPermissions", correlationID, ex.Message);
        System.Diagnostics.Debug.WriteLine("Excepción: " + errorMessage);
        if (ShowErrorDetail)
          eventWriter.WriteLog(string.Format("Excepción: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

        return listActionsByState;

      }

    }

    [HttpPost]
    public ActionResult GetReport(string parameters)
    {
      try
      {
        string userName = User.Identity.Name;
        ViewData["userName"] = userName;
        ViewData["parameters"] = parameters;
        return PartialView("ReportView");
      }
      catch (Exception ex)
      {

        return GenerateLogError(ex, "GetReport");
      }
    }


    [ActionName("GetReportByRequest")]
    public ActionResult GetReport(string parameters, int requestid)
    {

      try
      {
        string userName = User.Identity.Name;
        ViewData["userName"] = userName;
        ViewData["parameters"] = parameters;
        ViewData["requestId"] = requestid;
        return PartialView("ReportView");
      }
      catch (Exception ex)
      {

        return GenerateLogError(ex, "GetReportByRequest");
      }
    }

    [HttpPost]
    public string ReturnResource(Guid uidAction)
    {
      try
      {
        FormPageActions formPageAction = null;
        formPageAction = _stpcForms.FormPageActions.Where(e => e.Uid == uidAction).FirstOrDefault();
        return formPageAction.Resource;
      }
      catch (Exception ex)
      {
        GenerateLogError(ex, "ReturnResource");
        return string.Empty;
      }
    }

    public ActionResult GetPublicResource(Guid itemId, string parameters, int source)
    {
      WebClient client = new WebClient();
      try
      {
        if (bool.Parse(ConfigurationManager.AppSettings[itemId.ToString() + "-RequiresAuthentication"]))
        {
          string userId = ConfigurationManager.AppSettings[itemId.ToString() + "-User"];
          string password = ConfigurationManager.AppSettings[itemId.ToString() + "-Pwd"];
          string domain = ConfigurationManager.AppSettings[itemId.ToString() + "-UserDomain"];
          NetworkCredential nwc = new NetworkCredential(userId, password, domain);
          client.Credentials = nwc;
        }
        string documentType = ConfigurationManager.AppSettings[itemId.ToString() + "-DocType"];
        string fileName = ConfigurationManager.AppSettings[itemId.ToString() + "-FileName"];
        Byte[] pageData = client.DownloadData(parameters);
        //return File(pageData, "application/" + documentType);
        //return File(pageData, "application/" + "vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        return File(pageData, "application/" + documentType, fileName);
      }
      catch (Exception ex)
      {
        return DefaultErrorHandling(ex, "GetPublicResource");
      }
    }

    public ActionResult GetPublicResourcebyUser(Guid itemId, string parameters, int source)
    {
      WebClient client = new WebClient();
      try
      {
        if (bool.Parse(ConfigurationManager.AppSettings[itemId.ToString() + "-RequiresAuthentication"]))
        {
          string userId = ConfigurationManager.AppSettings[itemId.ToString() + "-User"];
          string password = ConfigurationManager.AppSettings[itemId.ToString() + "-Pwd"];
          string domain = ConfigurationManager.AppSettings[itemId.ToString() + "-UserDomain"];
          NetworkCredential nwc = new NetworkCredential(userId, password, domain);
          client.Credentials = nwc;
        }
        Byte[] pageData = client.DownloadData(parameters.Replace("usernamevalue", this.User.Identity.Name));
        return File(pageData, "application/pdf");
      }
      catch (Exception ex)
      {
        return DefaultErrorHandling(ex, "GetPublicResourcebyUser");
      }
    }

    public ActionResult GetPrivateResourceByParameters(Guid itemId, string parameters, int source)
    {
      BlobService blob = new BlobService();
      try
      {
        Microsoft.WindowsAzure.StorageClient.CloudBlobContainer container = blob.GetCloudBlobContainer();
        Response.AddHeader("Content-Disposition", "attachment; filename=" + parameters); // force download
        container.GetBlobReference(parameters).DownloadToStream(Response.OutputStream);
        return new EmptyResult();
      }
      catch (Exception ex)
      {
        return DefaultErrorHandling(ex, "GetPrivateResource");
      }
    }

    public ActionResult GetPrivateResource(string resource)
    {
      BlobService blob = new BlobService();
      try
      {
        Microsoft.WindowsAzure.StorageClient.CloudBlobContainer container = blob.GetCloudBlobContainer();
        Response.AddHeader("Content-Disposition", "attachment; filename=" + resource); // force download
        container.GetBlobReference(resource).DownloadToStream(Response.OutputStream);
        return new EmptyResult();
      }
      catch (Exception ex)
      {
        return DefaultErrorHandling(ex, "GetPrivateResource");
      }
    }

    private ActionResult DefaultErrorHandling(Exception ex, string triggerAction)
    {
      bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
      Guid correlationID = Guid.NewGuid();

      ILogging eventWriter = LoggingFactory.GetInstance();
      string errorMessage = string.Format(CustomMessages.E0007, "FormPageController", triggerAction, correlationID, ex.Message);
      System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
      eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
      if (ShowErrorDetail)
      {
        return PartialView("_ErrorDetail", new HandleErrorInfo(new Exception(errorMessage), "FormPageController", triggerAction));
      }
      else
      {
        return PartialView("_ErrorGeneral", string.Format(CustomMessages.E0001, correlationID.ToString()));
      }
    }

    private JsonResult DefaultJsonErrorHandling(Exception ex, string triggerAction)
    {
      bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
      Guid correlationID = Guid.NewGuid();

      ILogging eventWriter = LoggingFactory.GetInstance();
      string errorMessage = string.Format(CustomMessages.E0007, "FormPageController", triggerAction, correlationID, ex.Message);
      System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
      eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
      if (ShowErrorDetail)
      {
        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return Json(new HandleErrorInfo(new Exception(errorMessage), "FormPageController", triggerAction));
      }
      else
      {
        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return Json(ex.Message);
      }
    }

    public ActionResult RefreshPage(FormCollection formFields, Guid uidAction)
    {
      if (formFields.Count > 10)
      {
        FormCollection par = new FormCollection();
        for (int i = 0; i < 10; i = i + 2)
        {
          par.Add(formFields[i], formFields[i + 1]);
        }
        Guid formPageId = Guid.Parse(par["FormPageUid"]);
        var thisForm = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == formPageId)).FirstOrDefault();
        var thisFormNext = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Form.Uid == thisForm.Form.Uid && x.DisplayOrder < thisForm.DisplayOrder)).FirstOrDefault();

        if (thisForm == null) return View("Error");
        ViewData["FormUid"] = thisForm.Form.Uid;
        ViewData["FormName"] = thisForm.Form.Name;
        ViewData["FormPageUid"] = thisForm.Uid;
        ViewData["PagePanel"] = thisForm.Panels.FirstOrDefault().Uid;
        ViewData["Paramsvalues"] = "";
        ViewData["RequestId"] = par["RequestId"];

        #region Consultar Acciones por pagina
        Guid iPageId = thisForm.Uid;
        ViewBag.Actions = ValidateButtonsPermissions(thisForm.Uid, Convert.ToInt32(ViewData["RequestId"]));
        #endregion Consultar Acciones por pagina
        return PartialView("_Respond", thisForm.Panels);
      }
      else
      {
        return View("Error");
      }

    }

    public ActionResult GoToSpecificPage(FormCollection formFields, string ActionId, Guid uidAction)
    {
      if (formFields.Count > 10)
      {
        FormCollection par = new FormCollection();
        for (int i = 0; i < 10; i = i + 2)
        {
          par.Add(formFields[i], formFields[i + 1]);
        }

        Guid formId = Guid.Parse(par["FormUid"]);
        Guid formPageId = Guid.Parse(par["FormPageUid"]);
        var thisForm = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == formPageId)).FirstOrDefault();

        var thisFormNext = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == Guid.Parse(ActionId))).FirstOrDefault();

        if (thisFormNext != null)
        {
          foreach (var item in thisFormNext.Panels)
          {
            item.Page = thisFormNext;
          }

          if (thisFormNext == null) return View("Error");
          ViewData["FormUid"] = thisFormNext.Form.Uid;
          ViewData["FormName"] = thisFormNext.Form.Name;
          ViewData["FormPageUid"] = thisFormNext.Uid;
          ViewData["PagePanel"] = thisFormNext.Panels.FirstOrDefault().Uid;
          ViewData["Paramsvalues"] = "";
          ViewData["RequestId"] = par["RequestId"];

          #region Consultar Acciones por pagina

          ViewBag.Actions = ValidateButtonsPermissions(thisFormNext.Uid, Convert.ToInt32(ViewData["RequestId"]));

          #endregion Consultar Acciones por pagina

          //Guarda solicitud

          return PartialView("_Respond", thisFormNext.Panels.OrderBy(e => e.SortOrder));
        }
        else
        {
          if (thisForm == null) return View("Error");
          ViewData["FormUid"] = thisForm.Form.Uid;
          ViewData["FormName"] = thisForm.Form.Name;
          ViewData["FormPageUid"] = thisForm.Uid;
          ViewData["PagePanel"] = thisForm.Panels.FirstOrDefault().Uid;
          ViewData["Paramsvalues"] = "";
          ViewData["RequestId"] = par["RequestId"];

          #region Consultar Acciones por pagina

          Guid iPageId = thisForm.Uid;
          List<FormPageActions> lstFormPageActions = _stpcForms.FormPageActions.Where(a => a.PageId == thisForm.Uid && a.IsAssociated == true).ToList();

          #endregion Consultar Acciones por pagina

          ViewBag.Actions = lstFormPageActions;
          return PartialView("_Respond", thisForm.Panels.OrderBy(e => e.SortOrder));
        }
      }
      else
      {
        return View("Error");
      }
    }

    public ActionResult UpdateStateActions(FormCollection formFields, string uidAction)
    {
      FormCollection par = new FormCollection();
      string[] arrayGuid = uidAction.Split('/');
      for (int i = 0; i < formFields.Count - 1; i = i + 2)
      {
        par.Add(formFields[i], formFields[i + 1]);
      }
      Guid formPageId = Guid.Parse(par["FormPageUid"]);
      ViewData["RequestId"] = par["RequestId"];

      var thisForm = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == formPageId)).FirstOrDefault();

      List<FormPageActions> listAccionesRequest = ValidateButtonsPermissions(thisForm.Uid, Convert.ToInt32(ViewData["RequestId"]));

      if (listAccionesRequest.Where(e => e.Uid == Guid.Parse(arrayGuid[1])).FirstOrDefault() == null)
        return Json(false, JsonRequestBehavior.AllowGet);
      else
        return Json(true, JsonRequestBehavior.AllowGet);

    }

    [HttpPost]

    public ActionResult RespondAction(FormCollection formFields, Guid uidAction)
    {
#if DEBUG
      DateTime refTime = DateTime.Now;
#endif
      FormPageActions formPageAction = null;
      FormData formData = new FormData();
      string fieldNameUid = string.Empty;
      FormPage actualForm;
      FormPage nextForm;
      try
      {

        formData.FormId = Guid.Parse(formFields[1]);
        formData.FormPageId = Guid.Parse(formFields[3]);
        formData.FormName = formFields[5].ToString().Replace("+", "");
        formData.PanelId = Guid.Parse(formFields[7]);
        formData.RequestId = Convert.ToInt32(formFields[9]);

        formPageAction = _stpcForms.FormPageActions.Where(e => e.Uid == uidAction).FirstOrDefault();
#if DEBUG
        System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "FPC-RespondAction-formPageAction", DateTime.Now.Subtract(refTime).Milliseconds);
        refTime = DateTime.Now;
#endif

        _stpcForms.MergeOption = System.Data.Services.Client.MergeOption.PreserveChanges;


        if (formPageAction != null)
        {
          string uidPage = Guid.Empty.ToString();

          Services.Entities.Request request = null;
          CustomRequestProvider _CustomRequestProvider = new CustomRequestProvider();
          List<Common.Services.Users.PageField> listPageField = _CustomRequestProvider.getFieldByPage(formData.FormPageId);

          if (formPageAction.Save)
          {
            request = SaveRequest(formFields, ref uidPage, listPageField, ref formData);
          }
          string srNameField = string.Empty;
          Common.Services.Users.PageField _PageField = new Common.Services.Users.PageField();
          string ParentValue = string.Empty;

          STPC.DynamicForms.Web.RT.Services.Entities.PageEvent _pageEvent = null;
          List<STPC.DynamicForms.Web.RT.Services.Entities.PageEvent> listPageEvent = _stpcForms.PageEvent.Where(e => e.FormPageUid == formData.FormPageId).ToList();

          Dictionary<Guid, string> dcParentValue = new Dictionary<Guid, string>();




          if (formData.PageFields == null)
          {
            formData.PageFields = new Dictionary<string, string>();

          }

          for (int i = 10; i < formFields.Count - 1; i = i + 2)
          {
            fieldNameUid = formFields[i];

            if (fieldNameUid.Contains("STPC_DFi_"))
            {
              srNameField = fieldNameUid.Substring(9);

              _PageField = listPageField.Where(e => e.Uid == Guid.Parse(srNameField)).FirstOrDefault();

              if (!formPageAction.Save)
              {
                if (!formData.PageFields.ContainsKey(_PageField.FormFieldName))

                  formData.PageFields.Add(_PageField.FormFieldName, formFields[i + 1]);

              }


              if (_PageField != null)
              {
                _pageEvent = listPageEvent.Where(e => e.PageFieldUid == _PageField.Uid).FirstOrDefault();

                if (_pageEvent != null)
                {
                  if (!string.IsNullOrEmpty(formFields[i + 1]))

                    if (!dcParentValue.ContainsKey(_PageField.Uid))
                      dcParentValue.Add(_PageField.Uid, formFields[i + 1] + ",");
                    else
                      dcParentValue[_PageField.Uid] += formFields[i + 1] + ",";
                }

              }
            }
          }

          ViewData["DcParentValues"] = dcParentValue;

          if (formPageAction.FormStatesUid != null && !formPageAction.IsExecuteStrategy)
          {
            if (request == null)
              request = _stpcForms.Request.Where(e => e.RequestId == formData.RequestId).FirstOrDefault();

            request.WorkFlowState = formPageAction.FormStatesUid;
            request.UpdatedBy = User.Identity.Name;
            request.Updated = DateTime.Now;

            //Actualzia por ADO la instancia del request.
            UpdateRequest(formData, request);

            //Se comenta esto para evitar actualizar otros campos del request
            //_stpcForms.UpdateObject(request);
            //_stpcForms.SaveChanges();
          }

          if (formPageAction.StrategyID != 0 && formPageAction.IsExecuteStrategy)
          {
            string resultDecisionEngine = string.Empty;

            formData.PageFields.Add("requestId", formData.RequestId.ToString());
            formData.PageFields.Add("userId", User.Identity.Name);

            bool isXmlDocumentError = true;
            int countErrorParameters = 0;//Cuenta las veces que se repite el proceso 
            //Valida que el resultado no tenga parametros repetidos
            while (isXmlDocumentError)
            {
              validateXmlStrategieParameters(formPageAction, formData, ref resultDecisionEngine, ref isXmlDocumentError);
              if (countErrorParameters > 5 && isXmlDocumentError)
              {
                throw new Exception("Se cumplio el máximo de intentos para cargar los parametros de la estrategia: ");
              }
              else
                countErrorParameters++;
            }

            //List<FormPage> listFormPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where(page =>page.Uid== formData.FormPageId).ToList();
            FormPage formPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where(page => page.Uid == formData.FormPageId).FirstOrDefault();

            XmlDocument decisionEngineResult = new XmlDocument();
            decisionEngineResult.LoadXml(resultDecisionEngine);

            string errorCode = decisionEngineResult.SelectSingleNode("/application_result/ErrorCode[1]").InnerText;
            string responseTableName = "TBL_" + formPage.Name.Trim() + "_" + formPageAction.Caption.Trim().Replace(' ', '_') + "_Response";
            requestProvider.InsertDecisionEngineResult(decisionEngineResult.DocumentElement, formData.RequestId, User.Identity.Name, responseTableName);

            if (errorCode == "0")
            {
              string workFlowState = decisionEngineResult.SelectSingleNode("/application_result/Workflowstate[1]").InnerText;
              uidPage = decisionEngineResult.SelectSingleNode("/application_result/PageToShow[1]").InnerText;
              string messageToDisplay = decisionEngineResult.SelectSingleNode("/application_result/MessageToDisplay[1]").InnerText;
              string formToShow = decisionEngineResult.SelectSingleNode("/application_result/FormToShow[1]").InnerText;


              ViewBag.messageStrategy = messageToDisplay;

              if (request == null)
                request = _stpcForms.Request.Where(e => e.RequestId == formData.RequestId).FirstOrDefault();

              if (workFlowState != Guid.Empty.ToString())
              {
                request.WorkFlowState = Guid.Parse(workFlowState);

              }
              else
              {
                if (formPageAction.FormStatesUid != null)
                  request.WorkFlowState = formPageAction.FormStatesUid;
              }

              if (uidPage != Guid.Empty.ToString())
              {

                if (formPageAction.Save)
                {
                  request.UpdatedBy = User.Identity.Name;
                  request.Updated = DateTime.Now;

                  UpdateRequest(formData, request);
                }
                //_stpcForms.UpdateObject(request);
                //_stpcForms.SaveChanges();

                actualForm = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == formData.FormPageId)).FirstOrDefault();
                nextForm = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == Guid.Parse(uidPage))).FirstOrDefault();

                request.PageFlowId = Guid.Parse(uidPage);
                ViewData["FormUid"] = formPage.Form.Uid;
                ViewData["FormName"] = formPage.Form.Name;
                ViewData["FormPageUid"] = uidPage;
                ViewData["PagePanel"] = nextForm.Panels.FirstOrDefault().Uid;
                ViewData["Paramsvalues"] = "";
                ViewData["RequestId"] = formData.RequestId;

                #region Consultar Acciones por pagina

                ViewBag.Actions = ValidateButtonsPermissions(nextForm.Uid, Convert.ToInt32(formData.RequestId));

                #endregion Consultar Acciones por pagina

                //Guarda solicitud
                return PartialView("_Respond", nextForm.Panels.OrderBy(e => e.SortOrder));
              }

              if (formToShow != Guid.Empty.ToString())
              {
                throw new NotImplementedException();
              }
              if (formPageAction.Save)
              {
                UpdateRequest(formData, request);
              }
              if (!formPageAction.Save && workFlowState != Guid.Empty.ToString())
              {
                UpdateRequest(formData, request);
              }

            }
            else
            {
              string errorMessage = decisionEngineResult.SelectSingleNode("/application_result/ErrorMessage[1]").InnerText;
              throw new Exception(string.Format("***{0}***", errorMessage));
            }
          }

          int displayOrder;

          if (ConfigurationManager.AppSettings["ActionsUpdateCache"] != null)
          {
            string actionsUpdateCache = System.Configuration.ConfigurationManager.AppSettings["ActionsUpdateCache"].ToString();

            string[] words = actionsUpdateCache.Split(' ');
            foreach (string word in words)
            {
              if (!string.IsNullOrEmpty(word))
                if (Guid.Parse(word) == formPageAction.Uid)
                {
                  _AbcRedisCacheManager.updateCacheOptioneByInstance();
                }
            }
          }
          switch (formPageAction.Name)
          {
            #region Next
            case "Siguiente":
              actualForm = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == formData.FormPageId)).FirstOrDefault();
              displayOrder = actualForm.DisplayOrder + 1;
              nextForm = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Form.Uid == formData.FormId && x.DisplayOrder == displayOrder)).FirstOrDefault();

              if (nextForm != null)
              {
                foreach (var item in nextForm.Panels)
                {
                  item.Page = nextForm;
                }

                if (nextForm == null) return View("Error");

                ViewData["FormUid"] = nextForm.Form.Uid;
                ViewData["FormName"] = nextForm.Form.Name;
                ViewData["FormPageUid"] = nextForm.Uid;
                ViewData["PagePanel"] = nextForm.Panels.FirstOrDefault().Uid;
                ViewData["Paramsvalues"] = "";
                ViewData["RequestId"] = formData.RequestId;
                ViewBag.Actions = ValidateButtonsPermissions(nextForm.Uid, Convert.ToInt32(ViewData["RequestId"]));
                return PartialView("_Respond", nextForm.Panels.OrderBy(e => e.SortOrder));
              }
              else
              {
                if (actualForm == null) return View("Error");
                ViewData["FormUid"] = actualForm.Form.Uid;
                ViewData["FormName"] = actualForm.Form.Name;
                ViewData["FormPageUid"] = actualForm.Uid;
                ViewData["PagePanel"] = actualForm.Panels.FirstOrDefault().Uid;
                ViewData["Paramsvalues"] = "";
                ViewData["RequestId"] = formData.RequestId;

                #region Consultar Acciones por pagina
                List<FormPageActions> lstFormPageActions = _stpcForms.FormPageActions.Where(a => a.PageId == actualForm.Uid).ToList();
                #endregion Consultar Acciones por pagina

                ViewBag.Actions = lstFormPageActions;
                return PartialView("_Respond", actualForm.Panels.OrderBy(e => e.SortOrder));
              }
            #endregion
            #region Previous
            case "Anterior":
              actualForm = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == formData.FormPageId)).FirstOrDefault();
              displayOrder = actualForm.DisplayOrder - 1;
              nextForm = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Form.Uid == formData.FormId && x.DisplayOrder == displayOrder)).FirstOrDefault();


              List<STPC.DynamicForms.Web.RT.Services.Entities.PageEvent> _PageEventListener = null;



              if (nextForm != null)
              {
                foreach (var item in nextForm.Panels)
                {
                  item.Page = nextForm;

                  _PageEventListener = _stpcForms.PageEvent.Where(e => e.ListenerFieldId == item.Uid && e.EventType != "Cascade").ToList();

                  ViewData[item.Uid.ToString()] = _PageEventListener.Count > 0 ? true : false;
                }

                if (nextForm == null) return View("Error");
                ViewData["FormUid"] = nextForm.Form.Uid;
                ViewData["FormName"] = nextForm.Form.Name;
                ViewData["FormPageUid"] = nextForm.Uid;
                ViewData["PagePanel"] = nextForm.Panels.FirstOrDefault().Uid;
                ViewData["Paramsvalues"] = "";
                ViewData["RequestId"] = formData.RequestId;

                #region Consultar Acciones por pagina

                ViewBag.Actions = ValidateButtonsPermissions(nextForm.Uid, Convert.ToInt32(formData.RequestId));

                #endregion Consultar Acciones por pagina

                //Guarda solicitud
                return PartialView("_Respond", nextForm.Panels.OrderBy(e => e.SortOrder));
              }
              else
              {
                if (actualForm == null) return View("Error");
                ViewData["FormUid"] = actualForm.Form.Uid;
                ViewData["FormName"] = actualForm.Form.Name;
                ViewData["FormPageUid"] = actualForm.Uid;
                ViewData["PagePanel"] = actualForm.Panels.FirstOrDefault().Uid;
                ViewData["Paramsvalues"] = "";
                ViewData["RequestId"] = formData.RequestId;

                #region Consultar Acciones por pagina

                Guid iPageId = actualForm.Uid;

                ViewBag.Actions = ValidateButtonsPermissions(actualForm.Uid, Convert.ToInt32(formData.RequestId));
                #endregion Consultar Acciones por pagina
                return PartialView("_Respond", actualForm.Panels.OrderBy(e => e.SortOrder));
              }
            #endregion
            #region GotoPage
            case "IrPaginaEspecifica":
              actualForm = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == formData.FormPageId)).FirstOrDefault();
              displayOrder = actualForm.DisplayOrder - 1;
              nextForm = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == formPageAction.GoToPageId)).FirstOrDefault();

              if (nextForm != null)
              {
                foreach (var item in nextForm.Panels)
                {
                  item.Page = nextForm;
                }

                if (nextForm == null) return View("Error");
                ViewData["FormUid"] = nextForm.Form.Uid;
                ViewData["FormName"] = nextForm.Form.Name;
                ViewData["FormPageUid"] = nextForm.Uid;
                ViewData["PagePanel"] = nextForm.Panels.FirstOrDefault().Uid;
                ViewData["Paramsvalues"] = "";
                ViewData["RequestId"] = formData.RequestId;

                #region Consultar Acciones por pagina

                ViewBag.Actions = ValidateButtonsPermissions(nextForm.Uid, Convert.ToInt32(formData.RequestId));

                #endregion Consultar Acciones por pagina

                //Guarda solicitud
                return PartialView("_Respond", nextForm.Panels.OrderBy(e => e.SortOrder));
              }
              else
              {
                if (actualForm == null) return View("Error");
                ViewData["FormUid"] = actualForm.Form.Uid;
                ViewData["FormName"] = actualForm.Form.Name;
                ViewData["FormPageUid"] = actualForm.Uid;
                ViewData["PagePanel"] = actualForm.Panels.FirstOrDefault().Uid;
                ViewData["Paramsvalues"] = "";
                ViewData["RequestId"] = formData.RequestId;

                #region Consultar Acciones por pagina

                Guid iPageId = actualForm.Uid;

                ViewBag.Actions = ValidateButtonsPermissions(actualForm.Uid, Convert.ToInt32(formData.RequestId));
                #endregion Consultar Acciones por pagina
                return PartialView("_Respond", actualForm.Panels.OrderBy(e => e.SortOrder));
              }
            #endregion
            #region Save
            case "Guardar":
              if (formPageAction.ShowSuccessMessage)
              {
                return Json(new
                {
                  Success = true,
                  UidPage = uidPage,
                  Message = formPageAction.SuccessMessage
                });
              }
              else
              {
                return Json(new
                {
                  UidPage = uidPage,
                  Success = true,
                  Message = CustomMessages.G0001
                });
              }
            case "GuardarSinValidar":
              if (formPageAction.ShowSuccessMessage)
              {
                return Json(new
                {
                  Success = true,
                  UidPage = uidPage,
                  Message = formPageAction.SuccessMessage
                });
              }
              else
              {
                return Json(new
                {
                  UidPage = uidPage,
                  Success = true,
                  Message = CustomMessages.G0001
                });
              }
            #endregion

            default:
              return Json("Ok");
          }

          //Valida si se actualiza el cache de categorias



        }
        return View("Error");

      }
      catch (Exception ex)
      {
        string errorMessage;

        if (ex.Message.Contains(CustomMessages.EX0001) || ex.Message.Contains(CustomMessages.EX0002) || ex.Message.Contains(CustomMessages.EX0003) || ex.Message.Contains(CustomMessages.E0000))
        {
          errorMessage = string.Format(CustomMessages.E0002, ex.Message.Split(' ')[ex.Message.Split(' ').Length - 1]);
        }

        else
        {
          ILogging eventWriter = LoggingFactory.GetInstance();
          bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
          Guid correlationID = Guid.NewGuid();
          errorMessage = string.Format(CustomMessages.E0007, "FormPageController", "RespondAction", correlationID, ex.Message);
          System.Diagnostics.Debug.WriteLine("Exception: " + errorMessage);
          eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

          if (!ShowErrorDetail)
          {
            errorMessage = string.Format(CustomMessages.E0001, correlationID.ToString());
          }
        }

        if (formPageAction != null)
        {
          if (formPageAction.ShowFailureMessage)
          {
            errorMessage = formPageAction.FailureMessage;
          }
        }

        Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return Json(ex.Message);
        //return Json(new
        //{
        //	Success = false,
        //	Message = ex.Message
        //});
      }
    }

    private void validateXmlStrategieParameters(FormPageActions formPageAction, FormData formData, ref string resultDecisionEngine, ref bool isXmlDocumentError)
    {
      resultDecisionEngine = _decisionEngine.CallPageStrategy(formPageAction.StrategyID, formData.PageFields);


      XmlDocument _xmlDocument2 = new XmlDocument();
      _xmlDocument2.LoadXml(resultDecisionEngine);

      XmlNodeList _XmlNodeListValidation = _xmlDocument2.SelectNodes("/application_result");

      //Variable que va concatenando los nombres de los nodos;
      string listValues = string.Empty;


      foreach (XmlNode item in _XmlNodeListValidation[0].ChildNodes)
      {
        var value = item.LocalName;
        if (listValues.Contains(value))
        {
          writeError();
          isXmlDocumentError = true;
          break;
        }
        else
          isXmlDocumentError = false;
        listValues += value;
      }
    }

    private void writeError()
    {
      ILogging eventWriter = LoggingFactory.GetInstance();
      System.Diagnostics.Debug.WriteLine("Exception: Parametros repetidos, ejecutando estrategia de nuevo...");
      eventWriter.WriteLog(string.Format("Exception: {0}", "Exception: Parametros repetidos, ejecutando estrategia de nuevo"));
    }

    private Services.Entities.Request SaveRequest(FormCollection formFields, ref string uidPage, List<Common.Services.Users.PageField> listFields, ref FormData data)
    {
#if DEBUG
      DateTime refTime = DateTime.Now;
#endif
      try
      {
        string fieldNameUid = string.Empty;

        string fieldValue = string.Empty;
        StringBuilder requiredFields = new StringBuilder();
        int requestId = data.RequestId;
        Guid formPageId = data.FormPageId;
        Guid formId = data.FormId;
        string srNameField = string.Empty;
        Common.Services.Users.PageField _PageField = new Common.Services.Users.PageField();

        Common.Services.Users.PageFieldType _PageFieldType = new Common.Services.Users.PageFieldType();
        data.PageFields = new Dictionary<string, string>();
        Guid panelId = data.PanelId;



        string FieldTypeEmailName = System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameEmail"].ToString();
#if DEBUG
        System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "FPC-SaveRequest-pageFieldsandPageFieldTypes", DateTime.Now.Subtract(refTime).Milliseconds);
        refTime = DateTime.Now;
#endif
        //List<PageField> listFields = _stpcForms.PageFields.Expand("FormFieldType").ToList();

        for (int i = 10; i < formFields.Count - 1; i = i + 2)
        {
          fieldNameUid = formFields[i];

          if (fieldNameUid.Contains("STPC_DFi_"))
          {
            srNameField = fieldNameUid.Substring(9);
            _PageField = listFields.Where(e => e.Uid == Guid.Parse(srNameField)).FirstOrDefault();
            _PageFieldType = _PageField.FormFieldType;

            #region valida estrategia

            if (_PageField.ValidationStrategyID != null && _PageField.ValidationStrategyID != 0)
            {
              if (!string.IsNullOrEmpty(formFields[i + 1]))
              {
                string resultXml = _decisionEngine.CallStrategy((int)_PageField.ValidationStrategyID, formFields[i + 1]);
                XmlDocument _xmlDocument = new XmlDocument();
                _xmlDocument.LoadXml(resultXml);
                string result = _xmlDocument.LastChild.ToString();
              }
            }
            #endregion

            #region validar tipo moneda

            // validar tipo moneda o tipo numerico (MAX SIZE)
            if (_PageFieldType.FieldTypeName.Equals("Moneda") && !string.IsNullOrEmpty(formFields[i + 1]))
            {
              var max_size_convert = !string.IsNullOrEmpty(_PageField.MaxSize) ? Int64.Parse(_PageField.MaxSize) : Int64.Parse(_PageField.MaxSizeBD);
              var min_size_convert = !string.IsNullOrEmpty(_PageField.MinSize) ? _PageField.MinSize : "0";

              var converted = formFields[i + 1].Trim().Replace("$", "").Replace(".", "").Replace(",", ".");
              //var converted = par[i].Trim().Replace("$", "").Replace(".", "").Replace(",", "");
              var inputValue = !string.IsNullOrEmpty(converted) ? decimal.Parse(converted) : 0;

              if (converted != "0")
              {
                //Se pone entre comentarios, ya que no es necesaria la validación de este lado. está pendiente si es posible acceder a los atributos del input desde aquí
                if (inputValue > max_size_convert)
                {
                  //requiredFields.AppendFormat("El campo {0} tiene un valor superior al valor permitido. \n", _PageField.FormFieldName);
                  //break;
                }

                if (inputValue < Int64.Parse(min_size_convert))
                {
                  //requiredFields.AppendFormat("El campo {0} tiene un valor inferior al valor permitido. \n", _PageField.FormFieldName);
                  //break;
                }
              }
            }

            #endregion validar tipo moneda

            #region validar tipo numero

            // validar tipo moneda o tipo numerico (MAX SIZE)
            if (_PageFieldType.FieldTypeName.Equals("Número") && !string.IsNullOrEmpty(formFields[i + 1]))
            {
              var max_size_convert = !string.IsNullOrEmpty(_PageField.MaxSize) ? double.Parse(_PageField.MaxSize) : double.Parse(_PageField.MaxSizeBD);

              var min_size_convert = !string.IsNullOrEmpty(_PageField.MinSize) ? _PageField.MinSize : "0";

              var converted = formFields[i + 1].Trim().Replace("$", "").Replace(".", ",").Replace(",", ".");
              var inputValue = !string.IsNullOrEmpty(converted) ? double.Parse(converted) : 0;

              if (converted != "0")
              {
                if (inputValue > max_size_convert)
                {
                  requiredFields.AppendFormat("El campo {0} tiene un valor superior al valor permitido. \n", _PageField.FormFieldName);
                  break;
                }

                if (inputValue < double.Parse(min_size_convert))
                {
                  requiredFields.AppendFormat("El campo {0} tiene un valor inferior al valor permitido. \n", _PageField.FormFieldName);
                  break;
                }
              }
            }

            #endregion validar tipo moneda

            #region validar email

            // validar email
            if (!string.IsNullOrEmpty(formFields[i + 1]) && _PageFieldType.FieldTypeName == FieldTypeEmailName)
            {
              string strRegex = _PageFieldType.RegExDefault;
              formFields[i + 1].Replace(formFields[i + 1], formFields[i + 1].Trim());
              Regex re = new Regex(strRegex);
              if (!re.IsMatch(formFields[i + 1].Trim()))
              {
                requiredFields.AppendFormat("El campo {0} tiene un valor no permitido. \n", _PageField.FormFieldName);
                break;
              }
            }

            #endregion validar email

            #region validar tamaño de archivos

            if (_PageFieldType.FieldTypeName.Equals("Subir Archivo") && !string.IsNullOrEmpty(formFields[i + 1]))
            {

              //string mimeType = Request.Files[inputTagName].ContentType;
              //Stream fileStream = Request.Files[inputTagName].InputStream;
              //string fileName = Path.GetFileName(Request.Files[inputTagName].FileName);
              //int fileLength = Request.Files[inputTagName].ContentLength;
              //byte[] fileData = new byte[fileLength];
              //fileStream.Read(fileData, 0, fileLength);

              // --------------- este codigo guarda el archivo  ------------
              //var savedFileName = Path.Combine("E:\\", Path.GetFileName(fileName));
              //file.SaveAs(savedFileName);

            }

            #endregion validar tamaño de archivos

            // obtener y procesar el campo
            fieldValue = formFields[i + 1];

            // procesar tipo moneda
            if (_PageFieldType.FieldTypeName.Equals("Moneda") && !string.IsNullOrEmpty(formFields[i + 1]))
              fieldValue = fieldValue.Trim().Replace("$", "").Replace(".", "").Replace(",", ".");

            if (data.PageFields.ContainsKey(_PageField.FormFieldName))
            {
              if (!string.IsNullOrEmpty(fieldValue))
              {
                //TODO: find a better way
                string newFieldValue;
                data.PageFields.TryGetValue(_PageField.FormFieldName, out newFieldValue);
                data.PageFields.Remove(_PageField.FormFieldName);
                data.PageFields.Add(_PageField.FormFieldName, newFieldValue + "," + fieldValue);
              }

            }
            else
            {
              data.PageFields.Add(_PageField.FormFieldName, fieldValue);
            }
          }
        }

#if DEBUG
        System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "FPC-RespondAction-for", DateTime.Now.Subtract(refTime).Milliseconds);
        refTime = DateTime.Now;
#endif
        Services.Entities.Request _request = _stpcForms.Request.Where(e => e.RequestId == requestId).FirstOrDefault();

        string requeriedFieldsMessage = requiredFields.ToString();
        if (!string.IsNullOrEmpty(requeriedFieldsMessage))
        {
          throw new Exception(requeriedFieldsMessage);
        }

        Services.Request.Request request = new Services.Request.Request();

        #region carga request actual

        request = UpdateRequest(data.RequestId, data.FormPageId, _request.WorkFlowState);
        #endregion
#if DEBUG
        System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "FPC-RespondAction-updateRequest", DateTime.Now.Subtract(refTime).Milliseconds);
        refTime = DateTime.Now;
#endif
        #region crea la nueva instancia del formulario
        string _PageFlowState;

        //_PageFlowState = requestProvider.CreatePageFlowStepInstance(request, uidFormPage, formName, srXmlData);
        _PageFlowState = requestProvider.NewPageFlowStepInstance(request, data);
#if DEBUG
        System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "FPC-RespondAction-NewPageFlowStepInstance", DateTime.Now.Subtract(refTime).Milliseconds);
        refTime = DateTime.Now;
#endif
        _request = _stpcForms.Request.Where(e => e.RequestId == requestId).FirstOrDefault();
        _request.PageFlowState = _PageFlowState;
        _request.PageFlowId = data.FormPageId;
        _request.Updated = DateTime.Now;
        _request.UpdatedBy = User.Identity.Name;
        _stpcForms.UpdateObject(_request);
        _stpcForms.SaveChanges();
#if DEBUG
        System.Diagnostics.Debug.WriteLine("{0}-{1}:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, "FPC-RespondAction-SaveChanges", DateTime.Now.Subtract(refTime).Milliseconds);
        refTime = DateTime.Now;
#endif
        #endregion

        return _request;
      }
      catch (Exception ex)
      {

        Guid correlationID = Guid.NewGuid();
        ILogging eventWriter = LoggingFactory.GetInstance();
        string errorMessage = string.Format(CustomMessages.E0007, "FormPageController", "SaveRequest", correlationID, ex.Message);
        eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
        throw ex;
      }
    }


    private void UpdateRequest(FormData formData, Services.Entities.Request request)
    {
      Services.Request.Request request2 = new Services.Request.Request();

      request2.WorkFlowState = request.WorkFlowState;
      request2.RequestId = request.RequestId;
      request2.PageFlowState = request.PageFlowState;
      request2.PageFlowId = request.PageFlowId;
      request2.UpdatedBy = request.UpdatedBy;
      requestProvider.UpdateRequest(request2, formData.FormPageId, formData.FormName);
    }

    [HttpPost]
    public ActionResult getDataReport(FormCollection formFields, Guid uidAction)
    {
      try
      {

        FormData formData = new FormData();

        formData.RequestId = Convert.ToInt32(formFields[9]);


        return Json(new
        {
          RequestId = formData.RequestId,
        });

      }
      catch (Exception ex)
      {

        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return Json(ex.Message);
      }
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


    public FileContentResult DownloadPdf(Guid uidAction, int requestId)
    {
      try
      {
        FormPageActions formPageAction = null;

        string userid = ConfigurationManager.AppSettings["SsrsUser"];
        string password = ConfigurationManager.AppSettings["SsrsPwd"];
        string domain = ConfigurationManager.AppSettings["SsrsUserDomain"];
        formPageAction = _stpcForms.FormPageActions.Where(e => e.Uid == uidAction).FirstOrDefault();


        string reportURL = ConfigurationManager.AppSettings["SsrsServerPath"] + formPageAction.Resource + requestId; //"ServidorInformes/?%2fReportes%2fSufi%2fReporteSufiVFinal&rs:Command=Render&rs:Format=PDF&Idform=12";
        //formPageAction.
        //"http://ServerName/ReportServer?/ReportsFolder/ReportName&Parameter=UserName&rs:Command=Render&rs:Format=PDF";

        NetworkCredential nwc = new NetworkCredential(userid, password, domain);

        WebClient client = new WebClient();
        client.Credentials = nwc;
        Byte[] pageData = null;

        pageData = client.DownloadData(reportURL);
        //Response.ContentType = "application/pdf";
        //Response.AddHeader("Content-Disposition", "attachment; filename=" + DateTime.Now);
        //Response.BinaryWrite(pageData);
        //Response.Flush();
        //Response.End();
        return File(pageData, "application/pdf");
      }
      catch (Exception ex)
      {
        bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
        Guid correlationID = Guid.NewGuid();

        ILogging eventWriter = LoggingFactory.GetInstance();
        string errorMessage = string.Format(CustomMessages.E0007, "FormPageController", "ValidateButtonsPermissions", correlationID, ex.Message);
        System.Diagnostics.Debug.WriteLine("Excepción: " + errorMessage);
        if (ShowErrorDetail)
          eventWriter.WriteLog(string.Format("Excepción: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

        Byte[] pageData = null;
        return File(pageData, "application/pdf"); ;
      }
    }

    private void SaveRequest(FormCollection formFields, Guid uidAction, ref string uidPage)
    {
      try
      {
        FormCollection par = new FormCollection();
        for (int i = 0; i < formFields.Count - 1; i = i + 2)
        {
          par.Add(formFields[i], formFields[i + 1]);
        }

        string formUid = par["FormUid"];
        string formName = par["FormName"].ToString().Replace("+", "");
        string fieldNameUid = string.Empty; ;
        string srNameField = string.Empty; ;
        string fieldValue = string.Empty;
        StringBuilder requiredFields = new StringBuilder();
        string resultDecisionEngine = string.Empty;
        int requestId = Convert.ToInt32(par["RequestId"]);
        Guid uidPanel = Guid.Parse(par["PagePanel"]);
        Guid uidFormPage = Guid.Parse(par["FormPageUid"]);

        PageField _PageField = new PageField();
        PageFieldType _PageFieldType = new PageFieldType();

        if (!string.IsNullOrEmpty(formUid))
        {
          STPC.DynamicForms.Web.RT.Models.FormDataToXml objectForm = new Models.FormDataToXml(); //Objeto que almacena la data del formulario
          objectForm.IdForm = Guid.Parse(formUid);
          objectForm.Idpanel = uidPanel;
          objectForm.IdFormPage = uidFormPage;
          objectForm.FieldsByPage = new Dictionary<string, string>();

          //TODO: Filter the pageField from the current Page
          List<PageField> pageFields = _stpcForms.PageFields.ToList();
          List<PageFieldType> pageFieldTypes = _stpcForms.PageFieldTypes.ToList();

          for (int i = 0; i < par.Count; i++)
          {
            fieldNameUid = ((System.Collections.Specialized.NameValueCollection)(par)).AllKeys[i];

            if (fieldNameUid.Contains("STPC_DFi_"))
            {
              srNameField = fieldNameUid.Substring(9);
              _PageField = pageFields.Where(e => e.Uid == Guid.Parse(srNameField)).FirstOrDefault();
              _PageFieldType = pageFieldTypes.Where(e => e.Uid == _PageField.FormFieldType_Uid).FirstOrDefault();

              #region valida estrategia

              if (_PageField.ValidationStrategyID != null && _PageField.ValidationStrategyID != 0)
              {
                string resultXml = _decisionEngine.CallStrategy((int)_PageField.ValidationStrategyID, par[i]);
                XmlDocument _xmlDocument = new XmlDocument();
                _xmlDocument.LoadXml(resultXml);
                string result = _xmlDocument.LastChild.ToString();
              }
              #endregion

              #region validar requerido

              // se comenta esta seccion para que no dispare
              // la validacion en el post
              // validar requerido
              //if (_PageField.IsRequired && string.IsNullOrEmpty(par[i]))
              //{
              //  validationRequeridFields = validationRequeridFields + "El campo " + _PageField.FormFieldName + " es requerido";
              //  validationRequeridFields = validationRequeridFields + "\n";
              //  break;
              //}

              #endregion validar requerido

              #region validar tipo moneda

              // validar tipo moneda o tipo numerico (MAX SIZE)
              if (_PageFieldType.FieldTypeName.Equals("Moneda") && !string.IsNullOrEmpty(par[i]))
              {
                var max_size_convert = !string.IsNullOrEmpty(_PageField.MaxSize) ? int.Parse(_PageField.MaxSize) : int.Parse(_PageField.MaxSizeBD);
                var converted = par[i].Trim().Replace("$", "").Replace(".", "").Replace(",", ".");
                //var converted = par[i].Trim().Replace("$", "").Replace(".", "").Replace(",", "");
                var inputValue = !string.IsNullOrEmpty(converted) ? decimal.Parse(converted) : 0;

                if (inputValue > max_size_convert)
                {
                  requiredFields.AppendFormat("El campo {0} tiene un valor superior al valor permitido. \n", _PageField.FormFieldName);
                  break;
                }
              }

              #endregion validar tipo moneda

              #region validar tipo numero

              // validar tipo moneda o tipo numerico (MAX SIZE)
              if (_PageFieldType.FieldTypeName.Equals("Número") && !string.IsNullOrEmpty(par[i]))
              {
                //var max_size_convert = !string.IsNullOrEmpty(_PageField.MaxSize) ? int.Parse(_PageField.MaxSize) : int.Parse(_PageField.MaxSizeBD);
                var max_size_convert = !string.IsNullOrEmpty(_PageField.MaxSize) ? double.Parse(_PageField.MaxSize) : double.Parse(_PageField.MaxSizeBD);
                //var converted = par[i].Trim().Replace("$", "").Replace(".", "").Replace(",", "");
                var converted = par[i].Trim().Replace("$", "").Replace(".", "").Replace(",", ".");
                var inputValue = !string.IsNullOrEmpty(converted) ? double.Parse(converted) : 0;

                if (inputValue > max_size_convert)
                {
                  requiredFields.AppendFormat("El campo {0} tiene un valor superior al valor permitido. \n", _PageField.FormFieldName);
                  break;
                }
              }

              #endregion validar tipo moneda

              #region validar email

              string FieldTypeEmailName = System.Configuration.ConfigurationManager.AppSettings["FieldTypeNameEmail"].ToString();

              // validar email
              if (!string.IsNullOrEmpty(par[i]) && _PageFieldType.FieldTypeName == FieldTypeEmailName)
              {
                string strRegex = _PageFieldType.RegExDefault;
                Regex re = new Regex(strRegex);
                if (!re.IsMatch(par[i]))
                {
                  requiredFields.AppendFormat("El campo {0} tiene un valor superior al valor permitido. \n", _PageField.FormFieldName);
                  break;
                }
              }

              #endregion validar email

              #region validar tamaño de archivos

              if (_PageFieldType.FieldTypeName.Equals("Subir Archivo") && !string.IsNullOrEmpty(par[i]))
              {

                //string mimeType = Request.Files[inputTagName].ContentType;
                //Stream fileStream = Request.Files[inputTagName].InputStream;
                //string fileName = Path.GetFileName(Request.Files[inputTagName].FileName);
                //int fileLength = Request.Files[inputTagName].ContentLength;
                //byte[] fileData = new byte[fileLength];
                //fileStream.Read(fileData, 0, fileLength);

                // --------------- este codigo guarda el archivo  ------------
                //var savedFileName = Path.Combine("E:\\", Path.GetFileName(fileName));
                //file.SaveAs(savedFileName);

              }

              #endregion validar tamaño de archivos

              // obtener y procesar el campo
              fieldValue = par[i];

              // procesar tipo moneda
              if (_PageFieldType.FieldTypeName.Equals("Moneda") && !string.IsNullOrEmpty(par[i]))
                fieldValue = fieldValue.Trim().Replace("$", "").Replace(".", "").Replace(",", ".");
              //fieldValue = fieldValue.Trim().Replace("$", "").Replace(".", "").Replace(",", "");

              string lastChar = string.Empty;

              if (!string.IsNullOrEmpty(fieldValue))
                lastChar = fieldValue.Substring(fieldValue.Length - 1, 1);

              if (lastChar == ",")
                objectForm.FieldsByPage.Add(_PageField.FormFieldName, fieldValue.Substring(0, fieldValue.Length - 1));
              else
                objectForm.FieldsByPage.Add(_PageField.FormFieldName, fieldValue);

            }
          }

          string requeriedFieldsMessage = requiredFields.ToString();
          if (!string.IsNullOrEmpty(requeriedFieldsMessage))
          {
            throw new Exception(requeriedFieldsMessage);
          }

          Services.Request.Request request = new Services.Request.Request();
          FormPageActions _FormPageActions;

          #region Serializa la data del formulario

          string srXmlData = convertToXmlFormData(objectForm);

          #endregion

          #region Serializa la data del formulario
          Dictionary<string, string> dicDataPage = serializeFormPage(objectForm);
          #endregion

          #region carga request actual
          Services.Entities.Request _request = _stpcForms.Request.Where(e => e.RequestId == int.Parse(par["RequestId"])).FirstOrDefault();
          request = UpdateRequest(requestId, uidFormPage, _request.WorkFlowState);
          #endregion

          #region crea la nueva instancia del formulario
          string _PageFlowState;

          _PageFlowState = requestProvider.CreatePageFlowStepInstance(request, uidFormPage, formName, srXmlData);
          _request = _stpcForms.Request.Where(e => e.RequestId == requestId).FirstOrDefault();
          _request.PageFlowState = _PageFlowState;
          _request.PageFlowId = uidFormPage;
          _request.Updated = DateTime.Now;
          _request.UpdatedBy = User.Identity.Name;
          _stpcForms.UpdateObject(_request);
          _stpcForms.SaveChanges();

          var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == Guid.Parse(formUid)).FirstOrDefault();
          var pag = thisForm.Pages.Where(pages => pages.Uid == uidFormPage).FirstOrDefault();
          var orderList = thisForm.Pages.OrderBy(o => o.DisplayOrder).ToList();
          var indexCurrentPage = orderList.IndexOf(pag);

          #endregion

          #region valida si la acción modifica el estado de la solicitud

          _FormPageActions = _stpcForms.FormPageActions.Where(e => e.Uid == uidAction).FirstOrDefault();
          if (_FormPageActions != null)
          {
            if (_FormPageActions.FormStatesUid != null)
            {
              _request = _stpcForms.Request.Where(e => e.RequestId == requestId).FirstOrDefault();
              _request.WorkFlowState = _FormPageActions.FormStatesUid;
              _stpcForms.UpdateObject(_request);
              _stpcForms.SaveChanges();
            }
          }

          #endregion

          #region ejecuta estrategia de página
          //valida si se debe ejecutar o no la estrategia


          if (_FormPageActions != null)
          {
            if (_FormPageActions.StrategyID != 0 && _FormPageActions.IsExecuteStrategy)
            {
              dicDataPage.Add("requestId", requestId.ToString());
              dicDataPage.Add("userId", User.Identity.Name);
              resultDecisionEngine = _decisionEngine.CallPageStrategy(_FormPageActions.StrategyID, dicDataPage);

              FormPage _FormPage = _stpcForms.FormPages.Where(e => e.Uid == uidFormPage).FirstOrDefault();

              #region Busca en el xml de respuesta el workFlowState

              XmlDocument decisionEngineResult = new XmlDocument();
              decisionEngineResult.LoadXml(resultDecisionEngine);

              string workFlowState = decisionEngineResult.SelectSingleNode("/application_result/Workflowstate[1]").InnerText;
              string errorCode = decisionEngineResult.SelectSingleNode("/application_result/ErrorCode[1]").InnerText;
              uidPage = decisionEngineResult.SelectSingleNode("/application_result/PageToShow").InnerText;

              string responseTableName = "TBL_" + _FormPage.Name.Trim() + "_" + _FormPageActions.Caption.Trim().Replace(' ', '_') + "_Response";
              requestProvider.InsertDecisionEngineResult(decisionEngineResult.DocumentElement, requestId, User.Identity.Name, responseTableName);

              if (workFlowState != "-1")
              {
                request = UpdateRequest(requestId, uidFormPage, Guid.Parse(workFlowState));
                _request = _stpcForms.Request.Where(e => e.RequestId == requestId).FirstOrDefault();
                _request.WorkFlowState = Guid.Parse(workFlowState);
                _stpcForms.UpdateObject(_request);
                _stpcForms.SaveChanges();
              }
              #endregion
            }

          }
          #endregion

        }
      }
      catch (Exception ex)
      {
        Guid correlationID = Guid.NewGuid();
        ILogging eventWriter = LoggingFactory.GetInstance();
        string errorMessage = string.Format(CustomMessages.E0007, "FormPageController", "SaveRequest", correlationID, ex.Message);
        eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));
        throw ex;
      }
    }



    [HttpPost]
    [ValidateAntiForgeryTokenAttribute]
    public ActionResult RespondNew(Guid pageId, int requestId)
    {
      try
      {
        var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where(x => x.Uid == pageId).FirstOrDefault();

        if (thisPage == null) return View("Error");

        foreach (var item in thisPage.Panels)
        {
          item.Page = thisPage;
        }

        ViewData["FormUid"] = thisPage.Form.Uid;
        ViewData["FormName"] = thisPage.Form.Name;
        ViewData["FormPageUid"] = thisPage.Uid;
        ViewData["PagePanel"] = pageId;
        ViewData["Paramsvalues"] = "";
        ViewData["RequestId"] = requestId;

        #region Consulta estados de la página

        #endregion
        #region Consultar Acciones por pagina

        ViewBag.Actions = ValidateButtonsPermissions(pageId, requestId);

        #endregion Consultar Acciones por pagina



        return PartialView("_RespondNew", thisPage.Panels.OrderBy(o => o.SortOrder));

      }
      catch (Exception ex)
      {
        return DefaultErrorHandling(ex, "RespondNew");
      }
    }

    public ActionResult Respond_dt(Guid id)
    {
      try
      {


        var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == id)).FirstOrDefault();

        if (thisPage == null) return View("Error");
        ViewData["FormUid"] = thisPage.Form.Uid;
        ViewData["PageName"] = thisPage.Name;
        ViewData["FormPageUid"] = thisPage.Uid;
        ViewData["PagePanel"] = id;
        ViewData["Paramsvalues"] = "";
        return PartialView("_Respond_dt", thisPage.Panels.OrderBy(o => o.SortOrder));
      }
      catch (Exception ex)
      {
        return DefaultErrorHandling(ex, "Respond_dt");
      }
    }

    [HttpPost]
    [ValidateAntiForgeryTokenAttribute]
    public ActionResult Respond(Guid pageId, int requestId)
    {
      try
      {
        var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Expand("PageEvents").Where(x => x.Uid == pageId).FirstOrDefault();

        if (thisPage == null) return View("Error");


        List<STPC.DynamicForms.Web.RT.Services.Entities.PageEvent> _PageEventListener = null;


        foreach (var item in thisPage.Panels)
        {
          item.Page = thisPage;

          _PageEventListener = thisPage.PageEvents.Where(e => e.ListenerFieldId == item.Uid && e.EventType != "Cascade").ToList();

          ViewData[item.Uid.ToString()] = _PageEventListener.Count > 0 ? true : false;
        }

        ViewData["FormUid"] = thisPage.Form.Uid;
        ViewData["FormName"] = thisPage.Form.Name;
        ViewData["FormPageUid"] = thisPage.Uid;
        ViewData["PagePanel"] = pageId;
        ViewData["Paramsvalues"] = "";
        ViewData["RequestId"] = requestId;

        #region Consulta estados de la página

        #endregion

        #region Consultar Acciones por pagina

        ViewBag.Actions = ValidateButtonsPermissions(pageId, requestId);

        #endregion Consultar Acciones por pagina



        return PartialView("_Respond", thisPage.Panels.OrderBy(o => o.SortOrder));

      }
      catch (Exception ex)
      {
        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return Json(ex.Message);
      }
    }

    [HttpPost]
    public ActionResult RespondPost(FormPage thisPage, FormCollection campos, Panel panel)
    {
      string srNameForm = campos["FormUid"];
      Guid uidPanel = Guid.Parse(campos["PagePanel"]);
      Guid uidFormPage = Guid.Parse(campos["FormPageUid"]);
      string formName = campos["FormName"].ToString();
      string fieldNameUid;
      Guid idForm = new Guid();
      string srNameField;
      PageField _PageField = new PageField();
      string srValCampo;
      int requestId = Convert.ToInt32(campos["RequestId"]);
      Guid nextPageId = new Guid();

      if (!string.IsNullOrEmpty(srNameForm))
      {
        STPC.DynamicForms.Web.RT.Models.FormDataToXml objectForm = new Models.FormDataToXml(); //Objeto que almacena la data del formulario

        objectForm.IdForm = Guid.Parse(srNameForm);
        objectForm.Idpanel = uidPanel;
        objectForm.IdFormPage = uidFormPage;

        idForm = Guid.Parse(srNameForm);

        STPC.DynamicForms.Web.RT.Models.FormDataToXml _formDataToXml = new Models.FormDataToXml();
        objectForm.FieldsByPage = new Dictionary<string, string>();

        List<PageField> pageFields = _stpcForms.PageFields.Where(e => e.PanelUid == panel.Uid).ToList();

        for (int i = 0; i < campos.Count; i++)
        {
          fieldNameUid = ((System.Collections.Specialized.NameValueCollection)(campos)).AllKeys[i];

          if (fieldNameUid.Contains("STPC_DFi_"))
          {
            srNameField = fieldNameUid.Substring(9);
            _PageField = pageFields.Where(e => e.Uid == Guid.Parse(srNameField)).FirstOrDefault();
            srValCampo = campos[i];
            objectForm.FieldsByPage.Add(_PageField.FormFieldName, srValCampo);

          }
        }

        Services.Request.Request request = new Services.Request.Request()
        {
          RequestId = requestId,
          FormId = uidFormPage,
          UpdatedBy = User.Identity.Name

        };

        requestProvider.CreatePageFlowStepInstance(request, uidFormPage, formName, convertToXmlFormData(objectForm));
        var thisForm = _stpcForms.Forms.Expand("Pages").Where(form => form.Uid == Guid.Parse(srNameForm)).FirstOrDefault();
        var pag = thisForm.Pages.Where(pages => pages.Uid == uidFormPage).FirstOrDefault();
        var orderList = thisForm.Pages.OrderBy(o => o.DisplayOrder).ToList();
        var indexCurrentPage = orderList.IndexOf(pag);

        nextPageId = indexCurrentPage + 1 >= orderList.Count() ? orderList[indexCurrentPage].Uid : orderList[indexCurrentPage + 1].Uid;
      }
      return RedirectToAction("FormPageRespond", "Form", new
      {
        FormId = Guid.Parse(srNameForm),
        PageId = nextPageId,
        requestId = requestId
      });
    }

    [HttpPost]
    public ActionResult SaveFile()
    {
      string uriResult = null;
      int BytesConverter = 1024;

      try
      {
        #region validar archivo en el request

        foreach (string inputTagName in Request.Files)
        {
          HttpPostedFileBase file = Request.Files[inputTagName];

          if (file.ContentLength > 0)
          {

            #region obtener la informacion del file

            string mimeType = Request.Files[inputTagName].ContentType;
            Stream fileStream = Request.Files[inputTagName].InputStream;
            string fileName = Path.GetFileName(Request.Files[inputTagName].FileName);
            int fileLength = Request.Files[inputTagName].ContentLength;/// BytesConverter; // convertir bytes a KBytes
            byte[] fileData = new byte[fileLength];
            fileStream.Read(fileData, 0, fileLength);

            #endregion obtener la informacion del file

            // almacenar en el blob
            #region BLOB STORAGE

            // consultar
            //var listablobs = new BlobService().GetBlobs();

            uriResult = new BlobService().AddBlob(fileData, fileName, mimeType);

            //-------------------------------------------------------------------------------
            //-------------------------------------------------------------------------------
            /*******************************************
            BlobService _myBlobStorageService = new BlobService();

            //reemplazar el nombre del arhivo por un GUID para garantizar que sea unico
            var arrFileName = fileName.Split('.');
            string combinedFileName = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(arrFileName[1])) combinedFileName = combinedFileName + "." + arrFileName[1];

            // Retrieve a reference to a container 
            CloudBlobContainer blobContainer = _myBlobStorageService.GetCloudBlobContainer();

            CloudBlob blob = blobContainer.GetBlobReference(combinedFileName);

            // Create or overwrite the "myblob" blob with contents from a local file
            blob.UploadFromStream(fileStream);

            uriResult = blob.Uri.ToString();

              *******************************************/
            //-------------------------------------------------------------------------------
            //------------------------------------------------------------------------------

            #endregion BLOB STORAGE
          }

          //System.Web.HttpContext.Current.Session[GuidControlHelper.GetGuid() + typeof(HttpPostedFileBase).Name] = null;

        }

        #endregion validar archivo en el request
      }
      catch (Exception ex)
      {
        bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
        Guid correlationID = Guid.NewGuid();

        ILogging eventWriter = LoggingFactory.GetInstance();
        string errorMessage = string.Format(ex.Message, "FormPageController", "SaveFile", correlationID, ex.Message);
        System.Diagnostics.Debug.WriteLine("Excepcion: " + errorMessage);
        eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

        if (ShowErrorDetail)
        {
          return PartialView("_ErrorDetail_FileUpload", new HandleErrorInfo(new Exception(errorMessage), "FormPageController", "SaveFile"));
        }
        else
        {
          return PartialView("_ErrorDetail_FileUpload", string.Format(CustomMessages.E0001, correlationID.ToString()));
        }
      }
      InfoFile _InfoFile = new InfoFile(uriResult, new Guid());

      JsonResult result = Json(_InfoFile);
      result.ContentType = "text/plain";
      return result;
      //return Content(uriResult);
      //return Content(Request.Files[0].ContentType + ";" + Session["ContentLength"]);
    }

    /// <summary>
    /// Actualiza la tabla request
    /// </summary>
    /// <param name="requestId"></param>
    /// <param name="uidFormPage"></param>
    /// <returns></returns>
    private Services.Request.Request UpdateRequest(int requestId, Guid uidFormPage, Guid? workFlowState)
    {
      Services.Request.Request request = new Services.Request.Request()
      {
        RequestId = requestId,
        FormId = uidFormPage,
        UpdatedBy = User.Identity.Name,
        Updated = DateTime.Now,
        WorkFlowState = workFlowState
      };
      return request;
    }

    private Services.Request.Request UpdateRequest(int requestId, Guid uidFormPage)
    {
      Services.Request.Request request = new Services.Request.Request()
      {
        RequestId = requestId,
        FormId = uidFormPage,
        UpdatedBy = User.Identity.Name
      };
      return request;
    }

    [AcceptVerbs(HttpVerbs.Get)]
    public JsonResult GetStrategyResult(int id, string value)
    {
      string resultXml = _decisionEngine.CallStrategy((int)id, value);
      XmlDocument _xmlDocument = new XmlDocument();
      _xmlDocument.LoadXml(resultXml);
      string result = _xmlDocument.LastChild.InnerText;
      return Json(result, JsonRequestBehavior.AllowGet);

    }


    [AcceptVerbs(HttpVerbs.Post)]
    public JsonResult GetMathExpresion(Guid FormPageUid)
    {

      try
      {

        //List<Panel>lisPanels=_stpcForms.Panels.Where(e=>e.Page.Uid==FormPageUid).ToList();
        List<PageMathOperation> _ListpageMathOperation = new List<PageMathOperation>();

        _ListpageMathOperation = JsonConvert.DeserializeObject<List<PageMathOperation>>(_AbcRedisCacheManager["listPageMathOperation"]).ToList();

        //_ListpageMathOperation = _stpcForms.PageMathOperation.ToList();


        string[] result = new string[2];
        string[,] operations = new string[_ListpageMathOperation.Count, 2];
        int countOperations = 0;

        foreach (var _pageMathOperation in _ListpageMathOperation)
        {
          result[0] = _pageMathOperation.Expression;
          result[1] = _pageMathOperation.ResultField.ToString();
          operations[countOperations, 0] = result[0];
          operations[countOperations, 1] = result[1];
          countOperations++;
        }

        return Json(operations, JsonRequestBehavior.AllowGet);
      }
      catch (Exception ex)
      {
        return DefaultJsonErrorHandling(ex, "GetMathExpresion");
      }

    }

    private string convertToXmlFormData(Models.FormDataToXml form)
    {
      DataContractSerializer serializer = new DataContractSerializer(form.GetType());

      using (StringWriter sw = new StringWriter())
      {
        using (XmlTextWriter writer = new XmlTextWriter(sw))
        {
          // add formatting so the XML is easy to read in the log
          writer.Formatting = System.Xml.Formatting.Indented;

          serializer.WriteObject(writer, form);

          writer.Flush();
        }
        return sw.ToString();
      }
    }

    private Dictionary<string, string> serializeFormPage(Models.FormDataToXml form)
    {
      Dictionary<string, string> dicDataPage = new Dictionary<string, string>();

      foreach (var item in form.FieldsByPage)
      {
        dicDataPage.Add(item.Key, item.Value);
      }
      return dicDataPage;
    }

    public ActionResult CreateFromTemplate()
    {
      STPC.DynamicForms.Web.RT.Models.FormPageViewModel fpvm = new Models.FormPageViewModel();
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
        return RedirectToAction("Edit", "Form", new
        {
          id = theFormOfThePage
        });
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
        return getListPages(thePage.Form.Uid);
      }
      return Json(new { Success = false });
    }

    [HttpPost]
    [ValidateAntiForgeryTokenAttribute]
    public ActionResult LisPagePost(Guid id)
    {
      try
      {
        return getListPages(id);
      }
      catch (Exception ex)
      {
        return GenerateLogError(ex, "LisPagePost");

      }
      //return View(item.Pages.OrderBy(o => o.SortOrder));
    }

    private ActionResult GenerateLogError(Exception ex, string action)
    {
      string errorMessage;

      if (ex.Message.Contains(CustomMessages.EX0001) || ex.Message.Contains(CustomMessages.EX0002) || ex.Message.Contains(CustomMessages.EX0003) || ex.Message.Contains(CustomMessages.E0000))
      {
        errorMessage = string.Format(CustomMessages.E0002, ex.Message.Split(' ')[ex.Message.Split(' ').Length - 1]);
      }

      else
      {
        ILogging eventWriter = LoggingFactory.GetInstance();
        bool ShowErrorDetail = bool.Parse(ConfigurationManager.AppSettings["ShowErrorDetail"]);
        Guid correlationID = Guid.NewGuid();
        errorMessage = string.Format(CustomMessages.E0007, "FormPageController", action, correlationID, ex.Message);
        System.Diagnostics.Debug.WriteLine("Exception: " + errorMessage);
        eventWriter.WriteLog(string.Format("Exception: {0}, Stack Trace: {1}", errorMessage, ex.StackTrace));

        if (!ShowErrorDetail)
        {
          errorMessage = string.Format(CustomMessages.E0001, correlationID.ToString());
        }
      }

      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Json(ex.Message);
    }

    private ActionResult getListPages(Guid id)
    {
      var item = _stpcForms.Forms.Expand("Pages").Where((x => x.Uid == id)).FirstOrDefault();
      //TODO: Refactor código repetido en otro controller
      ViewBag.StrategiesSelect = new List<SelectListItem>();
      ViewBag.StrategiesSelect.Add(new SelectListItem { Text = "Seleccione una estrategia", Value = "0" });
      foreach (var strategy in _decisionEngine.GetStrategyList())
        ViewBag.StrategiesSelect.Add(new SelectListItem { Text = strategy.Name, Value = strategy.Id.ToString() });
      ViewBag.FormName = item.Name;
      ViewBag.FormId = item.Uid;
      return PartialView("List", item.Pages.OrderBy(o => o.DisplayOrder));
    }

    [HttpPost]
    public ActionResult Create(string name, string desc, int strategy, Guid formId)
    {
      _stpcForms.MergeOption = System.Data.Services.Client.MergeOption.PreserveChanges;

      var theForm = _stpcForms.Forms.Expand("Pages").Where(i => i.Uid == formId).FirstOrDefault();
      int theOrder;
      if (theForm.Pages.Count == 0) theOrder = 0;
      else
        theOrder = theForm.Pages.Select(m => m.DisplayOrder).Max() + 1;

      //theOrder = theForm.Pages.Select(m => m.SortOrder).Max() + 1;

      var theNewPage = new STPC.DynamicForms.Web.RT.Services.Entities.FormPage
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
      _stpcForms.SaveChanges();
      //System.Data.Services.Client.DataServiceResponse response = _stpcForms.SaveChanges();
      //Guid? newPageFormid = null;

      //foreach (System.Data.Services.Client.ChangeOperationResponse change in response)
      //{
      //	 System.Data.Services.Client.EntityDescriptor descriptor = change.Descriptor as System.Data.Services.Client.EntityDescriptor;
      //	 if (descriptor != null)
      //	 {
      //		  STPC.DynamicForms.Web.RT.Services.Entities.FormPage addedFormPage = descriptor.Entity as STPC.DynamicForms.Web.RT.Services.Entities.FormPage;

      //		  if (addedFormPage != null)
      //		  {
      //				newPageFormid = addedFormPage.Uid;

      //				List<FormPageActions> listFormPageActions = GetListFormPageActions(addedFormPage.Uid);
      //				foreach (var item in listFormPageActions)
      //				{

      //					 var theNewPageAction = new STPC.DynamicForms.Web.RT.Services.Entities.FormPageActions
      //					 {
      //						  Caption = item.Caption,
      //						  Description = item.Description,
      //						  IsAssociated = false,
      //						  Name = item.Name,
      //						  PageId = item.PageId
      //					 };
      //					 _stpcForms.AddObject("FormPageActions", theNewPageAction);

      //					 //_stpcForms.AddRelatedObject(TheFormPage, "FormPageActions", theNewPageAction);

      //					 System.Data.Services.Client.DataServiceResponse response2 = _stpcForms.SaveChanges();
      //				}
      //		  }
      //	 }
      //}
      return getListPages(formId);
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
        new FormPageActions{ Uid = new Guid(), Name="GuardarSinValidar", Description="Guardar Sin Validar", Caption="Guardar", IsAssociated=true, PageId = pageId,Save=true},
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
    [HttpPost]
    [ValidateAntiForgeryTokenAttribute]
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

      var item = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == id)).FirstOrDefault();

      if (Session["listStrategies"] == null)
      {
        ViewBag.StrategiesSelect = new List<SelectListItem>();
        foreach (var strategy in _decisionEngine.GetStrategyList().OrderBy(str => str.Name))
          ViewBag.StrategiesSelect.Add(new SelectListItem { Text = strategy.Name, Value = strategy.Id.ToString(), Selected = (item.StrategyID == strategy.Id ? true : false) });
        Session["listStrategies"] = ViewBag.StrategiesSelect;
      }
      else
      {
        ViewBag.StrategiesSelect = Session["listStrategies"];
      }


      //Recorre la lista de acciones para buscar la pagina asignada en la acción GoToPage

      List<FormPageActionsExtend> listFormPageActionsExtend = new List<FormPageActionsExtend>();
      foreach (FormPageActions _action in listActions)
      {
        FormPageActionsExtend _FormPageActionsExtend = new FormPageActionsExtend();
        _FormPageActionsExtend.Uid = _action.Uid;
        _FormPageActionsExtend.Caption = _action.Caption;
        _FormPageActionsExtend.Name = _action.Name;
        _FormPageActionsExtend.Description = _action.Description;
        _FormPageActionsExtend.IsAssociated = _action.IsAssociated;
        _FormPageActionsExtend.IsExecuteStrategy = _action.IsExecuteStrategy;
        _FormPageActionsExtend.IsPrivateResource = _action.IsPrivateResource;
        _FormPageActionsExtend.PageId = _action.PageId;
        _FormPageActionsExtend.DisplayOrder = _action.DisplayOrder;
        _FormPageActionsExtend.Save = _action.Save;

        _FormPageActionsExtend.GoToPageId = _action.GoToPageId;
        _FormPageActionsExtend.FormStatesUid = _action.FormStatesUid;
        _FormPageActionsExtend.ShowSuccessMessage = _action.ShowSuccessMessage;
        _FormPageActionsExtend.ShowFailureMessage = _action.ShowFailureMessage;
        _FormPageActionsExtend.SuccessMessage = _action.SuccessMessage;
        _FormPageActionsExtend.FailureMessage = _action.FailureMessage;

        _FormPageActionsExtend.StrategyID = _action.StrategyID;
        _FormPageActionsExtend.Resource = _action.Resource;
        _FormPageActionsExtend.SendUserParam = _action.SendUserParam;
        _FormPageActionsExtend.SendRequestIdParam = _action.SendRequestIdParam;
        _FormPageActionsExtend.SendHierarchyIdParam = _action.SendHierarchyIdParam;
        _FormPageActionsExtend.FormStates = _action.FormStates;
        _FormPageActionsExtend.FormPageActionsByStatesList = _action.FormPageActionsByStatesList;
        _FormPageActionsExtend.FormPageActionsRolesList = _action.FormPageActionsRolesList;


        if (_action.GoToPageId != null)
        {

          _FormPageActionsExtend.NameGoToPage = _stpcForms.FormPages.Where(c => c.Uid == _action.GoToPageId).FirstOrDefault().Name;
        }
        else
          _FormPageActionsExtend.NameGoToPage = "";

        if (_action.StrategyID != null && _action.IsExecuteStrategy)
        {
          _FormPageActionsExtend.NameStrategie = getStrategieName(ViewBag.StrategiesSelect, _action.StrategyID);
        }
        else
          _FormPageActionsExtend.NameStrategie = "";



        listFormPageActionsExtend.Add(_FormPageActionsExtend);
      }
      List<FormPageActions> lstFormPageActions = listActions;

      #endregion Consultar Acciones por pagina



      ViewBag.FormPageName = item.Name;
      ViewBag.FormPageId = item.Uid;
      ViewBag.FormId = item.Form.Uid;


      ViewBag.StrategiesSelect.Add(new SelectListItem { Text = "---Seleccione estrategia---", Value = "0", Selected = (item.StrategyID == 0 ? true : false) });



      ViewBag.PageStrategy = item.StrategyID.ToString();
      List<string> roles = Roles.GetAllRoles().ToList();
      ViewBag.roles = roles.OrderBy(q => q);

      ViewBag.states = _stpcForms.FormStates.OrderBy(e => e.StateName).ToList();
      //ViewBag.FormStates = _stpcForms2.FormPageActionsByStates.Where().Expand("FormStates").ToList();
      //var aaaa = from e in _stpcForms2.FormPageActionsByStates
      //			  where idActions.Contains(e.FormPageActionsUid)
      //			  select e;

      return PartialView(listFormPageActionsExtend);


    }

    [HttpPost]
    public ActionResult RefresFormPageAction(Guid id)
    {
      #region Consultar Acciones por pagina
      _stpcForms.MergeOption = System.Data.Services.Client.MergeOption.PreserveChanges;
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

      var item = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == id)).FirstOrDefault();

      if (Session["listStrategies"] == null)
      {
        ViewBag.StrategiesSelect = new List<SelectListItem>();
        foreach (var strategy in _decisionEngine.GetStrategyList().OrderBy(str => str.Name))
          ViewBag.StrategiesSelect.Add(new SelectListItem { Text = strategy.Name, Value = strategy.Id.ToString(), Selected = (item.StrategyID == strategy.Id ? true : false) });
        Session["listStrategies"] = ViewBag.StrategiesSelect;
      }
      else
      {
        ViewBag.StrategiesSelect = Session["listStrategies"];
      }


      //Recorre la lista de acciones para buscar la pagina asignada en la acción GoToPage

      List<FormPageActionsExtend> listFormPageActionsExtend = new List<FormPageActionsExtend>();
      foreach (FormPageActions _action in listActions)
      {
        FormPageActionsExtend _FormPageActionsExtend = new FormPageActionsExtend();
        _FormPageActionsExtend.Uid = _action.Uid;
        _FormPageActionsExtend.Caption = _action.Caption;
        _FormPageActionsExtend.Name = _action.Name;
        _FormPageActionsExtend.Description = _action.Description;
        _FormPageActionsExtend.IsAssociated = _action.IsAssociated;
        _FormPageActionsExtend.IsExecuteStrategy = _action.IsExecuteStrategy;
        _FormPageActionsExtend.IsPrivateResource = _action.IsPrivateResource;
        _FormPageActionsExtend.PageId = _action.PageId;
        _FormPageActionsExtend.DisplayOrder = _action.DisplayOrder;
        _FormPageActionsExtend.Save = _action.Save;

        _FormPageActionsExtend.GoToPageId = _action.GoToPageId;
        _FormPageActionsExtend.FormStatesUid = _action.FormStatesUid;
        _FormPageActionsExtend.ShowSuccessMessage = _action.ShowSuccessMessage;
        _FormPageActionsExtend.ShowFailureMessage = _action.ShowFailureMessage;
        _FormPageActionsExtend.SuccessMessage = _action.SuccessMessage;
        _FormPageActionsExtend.FailureMessage = _action.FailureMessage;

        _FormPageActionsExtend.StrategyID = _action.StrategyID;
        _FormPageActionsExtend.Resource = _action.Resource;
        _FormPageActionsExtend.SendUserParam = _action.SendUserParam;
        _FormPageActionsExtend.SendRequestIdParam = _action.SendRequestIdParam;
        _FormPageActionsExtend.SendHierarchyIdParam = _action.SendHierarchyIdParam;
        _FormPageActionsExtend.FormStates = _action.FormStates;
        _FormPageActionsExtend.FormPageActionsByStatesList = _action.FormPageActionsByStatesList;
        _FormPageActionsExtend.FormPageActionsRolesList = _action.FormPageActionsRolesList;


        if (_action.GoToPageId != null)
        {

          _FormPageActionsExtend.NameGoToPage = _stpcForms.FormPages.Where(c => c.Uid == _action.GoToPageId).FirstOrDefault().Name;
        }
        else
          _FormPageActionsExtend.NameGoToPage = "";

        if (_action.StrategyID != null && _action.IsExecuteStrategy)
        {
          _FormPageActionsExtend.NameStrategie = getStrategieName(ViewBag.StrategiesSelect, _action.StrategyID);
        }
        else
          _FormPageActionsExtend.NameStrategie = "";



        listFormPageActionsExtend.Add(_FormPageActionsExtend);
      }
      List<FormPageActions> lstFormPageActions = listActions;

      #endregion Consultar Acciones por pagina



      ViewBag.FormPageName = item.Name;
      ViewBag.FormPageId = item.Uid;
      ViewBag.FormId = item.Form.Uid;


      ViewBag.StrategiesSelect.Add(new SelectListItem { Text = "---Seleccione estrategia---", Value = "0", Selected = (item.StrategyID == 0 ? true : false) });



      ViewBag.PageStrategy = item.StrategyID.ToString();
      List<string> roles = Roles.GetAllRoles().ToList();
      ViewBag.roles = roles.OrderBy(q => q);

      ViewBag.states = _stpcForms.FormStates.OrderBy(e => e.StateName).ToList();
      //ViewBag.FormStates = _stpcForms2.FormPageActionsByStates.Where().Expand("FormStates").ToList();
      //var aaaa = from e in _stpcForms2.FormPageActionsByStates
      //			  where idActions.Contains(e.FormPageActionsUid)
      //			  select e;

      return PartialView("FormPageAction", listFormPageActionsExtend);


    }


    private string getStrategieName(List<SelectListItem> lisStrategies, int strategieId)
    {
      foreach (SelectListItem item in lisStrategies)
      {
        if (Convert.ToInt32(item.Value) == strategieId)
          return item.Text;
      }
      return string.Empty;

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

      _stpcForms.MergeOption = System.Data.Services.Client.MergeOption.PreserveChanges;

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

          _FormPageAction = _stpcForms.FormPageActions.Expand("FormPageActionsByStatesList/FormStates").Where(e => e.PageId == Guid.Parse(par["PageId"]) && e.Uid == Guid.Parse(Acciones[i])).FirstOrDefault();

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
              _stpcForms.DeleteObject(_FormPageAction);

              //foreach (var item in _FormPageAction.FormPageActionsByStatesList)
              //{
              //  _stpcForms.DeleteObject(item);
              //}
              _stpcForms.SaveChanges();
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
          _stpcForms.UpdateObject(_FormPageAction);
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

      return Json(new { Success = true });
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

    public ActionResult GetPagesStrategies(Guid FormPageId, Guid FormId, Guid FormaActionId)
    {
      try
      {
        FormPageActions _formPageAction = null;
        _formPageAction = _stpcForms.FormPageActions.Where(e => e.Uid == FormaActionId).FirstOrDefault();

        List<FormStates> _FormStates = new List<FormStates>();
        ViewBag.ActionId = FormaActionId;
        ViewBag.formState = _formPageAction.FormStatesUid;
        ViewBag.StrategiesSelect = new List<SelectListItem>();

        if (Session["listStrategies"] == null)
          foreach (var strategy in _decisionEngine.GetStrategyList().OrderBy(str => str.Name))
            ViewBag.StrategiesSelect.Add(new SelectListItem { Text = strategy.Name, Value = strategy.Id.ToString(), Selected = false });
        else
          ViewBag.StrategiesSelect = (List<SelectListItem>)Session["listStrategies"];

        return PartialView();
      }
      catch (Exception ex)
      {

        throw;
      }
    }

    [HttpPost]
    public ActionResult GetPagesStates(Guid id, Guid actionId, string stateName, bool isChecked)
    {
      FormPageActions _formPageAction = null;
      _formPageAction = _stpcForms.FormPageActions.Where(e => e.Uid == actionId).FirstOrDefault();

      if (_formPageAction != null)
      {
        if (isChecked)
          _formPageAction.FormStatesUid = id;
        else
          _formPageAction.FormStatesUid = null;

        _stpcForms.UpdateObject(_formPageAction);
        _stpcForms.SaveChanges();
        ViewBag.IdAction = actionId;
      }
      return Json(new { Success = true, name = id.ToString(), ActionId = actionId, NameEstado = stateName });
    }

    [HttpPost]
    public ActionResult GetPagesStrategies(int id, Guid actionId, string stateName, bool ischecked)
    {
      FormPageActions _formPageAction = null;
      _formPageAction = _stpcForms.FormPageActions.Where(e => e.Uid == actionId).FirstOrDefault();

      if (_formPageAction != null)
      {

        if (ischecked)
          _formPageAction.StrategyID = id;
        else
          _formPageAction.StrategyID = -1;

        _formPageAction.IsExecuteStrategy = ischecked;
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

      var theNewPageAction = new STPC.DynamicForms.Web.RT.Services.Entities.FormPageActions
      {
        Caption = _FormPageActions.Caption,
        Description = _FormPageActions.Description,
        IsAssociated = true,
        Save = _FormPageActions.Save,
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

          var _FormPageActionsRolesNew = new STPC.DynamicForms.Web.RT.Services.Entities.FormPageActionsRoles
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

          var _FormPageActionsByStatesNew = new STPC.DynamicForms.Web.RT.Services.Entities.FormPageActionsByStates
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

  public class GuidControlHelper
  {
    //Constructor
    public GuidControlHelper()
    {

    }
    public static Object GetElemetFromGuid(string nombre)
    {
      return (Object)HttpContext.Current.Session[GetGuid() + nombre];
    }
    public static string GetGuid()
    {
      if (HttpContext.Current.Session["guid"] == null)
        HttpContext.Current.Session["guid"] = Guid.NewGuid().ToString();
      return HttpContext.Current.Session["guid"].ToString();
    }
    public static void SessionToFree(string keyToFree)
    {
      HttpContext.Current.Session.Remove(keyToFree);
    }
  }
}