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
  public class StrategyController : Controller
  {
    Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
    private IDecisionEngine _decisionEngine;
    CustomMembershipProvider UsersProvider;
    ScriptGeneratorServiceClient _ScriptGenerator;

    public StrategyController()
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

    public StrategyController(IDecisionEngine iEngine)
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

      #region lista de estrategias asociadas

      // lista de estrategias asociadas
      var StrategyList = _stpcForms.PageStrategy.Expand("StrategyParametersList").Where(ps => ps.PageId == thisPage.Uid);
      ViewBag.StrategyList = StrategyList;

      #endregion lista de estrategias asociadas

      #region cargar la lista de estrategias disponibles

      List<SelectListItem> StrategiesSelect = new List<SelectListItem>();

      StrategiesSelect.Add(
        new SelectListItem
        {
          Selected = true,
          Text = "---Seleccione estrategia---",
          Value = "0"
        }
        );

      ViewBag.StrategiesSelect = new List<SelectListItem>();
      foreach (var strategy in _decisionEngine.GetStrategyList().OrderBy(str => str.Name))
        StrategiesSelect.Add(new SelectListItem { Text = strategy.Name, Value = strategy.Id.ToString() });

      ViewBag.StrategiesSelect = StrategiesSelect;

      #endregion cargar la lista de estrategias disponibles

      return View(StrategyList);
    }

    //
    // GET: /Strategy/Details/5

    public ActionResult Details(int StrategyId, Guid id)
    {
      var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((x => x.Uid == id)).FirstOrDefault();
      ViewBag.Data_FormPageName = thisPage.Name;
      ViewBag.Data_FormPageId = thisPage.Uid;
      ViewBag.Data_FormId = thisPage.Form.Uid;
      ViewBag.Data_StrategyId = StrategyId;

      //#region lista de tipos de campo

      //// lista de tipos de campo
      //ViewBag.listaTypes = _stpcForms.PageFieldTypes.ToList();

      //#endregion lista de tipos de campo

      #region lista de tipos de parametro

      // lista de tipos de campo
      ViewBag.listaTypes = this.GetParameterTypeList();

      #endregion lista de tipos de parametro

      #region cargar la lista de estrategias disponibles

      List<SelectListItem> StrategiesSelect = new List<SelectListItem>();

      StrategiesSelect.Add(
        new SelectListItem
        {
          Selected = true,
          Text = "---Seleccione estrategia---",
          Value = "0"
        }
        );

      ViewBag.StrategiesSelect = new List<SelectListItem>();
      foreach (var strategy in _decisionEngine.GetStrategyList())
        StrategiesSelect.Add(new SelectListItem { Text = strategy.Name, Value = strategy.Id.ToString() });

      ViewBag.StrategiesSelect = StrategiesSelect;

      #endregion cargar la lista de estrategias disponibles

      #region cargar la lista de campos del formulario

      var fieldList = new List<STPC.DynamicForms.Web.Services.Entities.PageField>();

      // recorrer los paneles para obtener los campos
      foreach (var panel in thisPage.Panels)
      {
        fieldList.AddRange(_stpcForms.PageFields.Where(pf => pf.PanelUid == panel.Uid && pf.FormFieldName != null && pf.FormFieldName != "").OrderBy(pf => pf.FormFieldName).ToList());
      }

      ViewBag.fieldList = fieldList;

      #endregion cargar la lista de campos del formulario

      #region obtener estructura de la estrategia

      // lista de parametros procesados
      List<ParameterViewModel> parameters = GetParameters(StrategyId);

      #endregion obtener estructura de la estrategia

      return PartialView(parameters);
    }

    //
    // GET: /Strategy/Create

    public ActionResult Create()
    {
      return View();
    }

    //
    // POST: /Strategy/Create

    [HttpPost]
    public JsonResult Create(FormCollection campos)
    {
      try
      {
        // obtener los parametros de pagina que vienen en el formcollection
        string pageName = null;
        Guid FormPageId = new Guid();
        Guid FormId = new Guid();
        int StrategyId = 0;

        Guid? TriggerUid = null;
        bool HasTrigger = false;
        Guid? ResponseUid = null;
        bool HasResponse = false;

        #region obtener los parametros de pagina que vienen en el formcollection

        for (int i = 0; i < campos.Count; i = i + 2)
        {
          if (campos[i].Contains("Data_"))
          {
            #region extraer valores globales

            switch (campos[i])
            {
              case "Data_FormPageName":
                pageName = campos[i + 1];
                break;

              case "Data_FormPageId":
                FormPageId = Guid.Parse(campos[i + 1]);
                break;

              case "Data_FormId":
                FormId = Guid.Parse(campos[i + 1]);
                break;

              case "Data_StrategyId":
                StrategyId = int.Parse(campos[i + 1]);
                break;
            }

            #endregion extraer valores globales
          }

          #region Extraer valores de trigger y response

          else
          {
            #region Trigger

            if (campos[i].Contains("Trigger"))
            {
              // i = trigger
              // i+1 = on/off
              HasTrigger = campos[i + 1].Equals("on") ? true : false;
              // i +2 = ddlTrigger
              // i+3 = uid
              TriggerUid = Guid.Parse(campos[i + 3]);
              i = i + 2;
            }

            #endregion Trigger

            #region Response

            else if (campos[i].Contains("Response"))
            {
              // i = response
              // i+1 = on/off
              HasResponse = campos[i + 1].Equals("on") ? true : false;
              // i +2 = ddlResponse
              // i+3 = uid
              ResponseUid = Guid.Parse(campos[i + 3]);
              i = i + 2;
            }
            #endregion Response
          }

          #endregion Extraer valores de trigger y response

        }

        #endregion obtener los parametros de pagina que vienen en el formcollection

        PageStrategy newStrategy = null;

        #region crear nueva estrategia

        if (campos.Count > 0)
        {
          newStrategy = new PageStrategy();
          newStrategy.Uid = Guid.NewGuid();
          newStrategy.PageId = FormPageId;
          newStrategy.StrategyId = StrategyId;
          newStrategy.HasTrigger = HasTrigger;
          newStrategy.TriggerFieldUid = TriggerUid.HasValue ? TriggerUid : null;
          newStrategy.HasResponse = HasResponse;
          newStrategy.ResponseFieldUid = ResponseUid.HasValue ? ResponseUid : null;

          _stpcForms.AddObject("PageStrategy", newStrategy);
          // guardar cambios
          _stpcForms.SaveChanges();

        }

        #endregion crear nueva estrategia

        // insertar parametros asociados
        SaveParameters(campos, newStrategy.Uid, StrategyId);

        return Json(new { success = true });
      }
      catch
      {
        return Json(new { success = false });
      }
    }

    //
    // GET: /Strategy/Edit/5

    public ActionResult Edit(Guid Uid)
    {
      ViewBag.EditKey = Uid;

      // consultar la estragia
      var theStrategy = _stpcForms.PageStrategy.Where(pstr => pstr.Uid == Uid).FirstOrDefault();

      List<ParameterViewModel> parameters = null;

      if (theStrategy != null)
      {
        var thisPage = _stpcForms.FormPages.Expand("Panels").Expand("Form").Where((page => page.Uid == theStrategy.PageId)).FirstOrDefault();
        ViewBag.Data_FormPageName = thisPage.Name;
        ViewBag.Data_FormPageId = thisPage.Uid;
        ViewBag.Data_FormId = thisPage.Form.Uid;
        ViewBag.Data_StrategyId = theStrategy.StrategyId;

        ViewBag.HasTrigger = theStrategy.HasTrigger;
        ViewBag.TriggerFieldUid = theStrategy.TriggerFieldUid;
        ViewBag.HasResponse = theStrategy.HasResponse;
        ViewBag.ResponseFieldUid = theStrategy.ResponseFieldUid;

        #region lista de tipos de campo

        // lista de tipos de campo
        //ViewBag.listaTypes = _stpcForms.PageFieldTypes.ToList();

        ViewBag.listaTypes = this.GetParameterTypeList();

        #endregion lista de tipos de campo

        #region cargar la lista de campos del formulario

        var fieldList = new List<STPC.DynamicForms.Web.Services.Entities.PageField>();


        // recorrer los paneles para obtener los campos
        foreach (var panel in thisPage.Panels)
        {
          fieldList.AddRange(_stpcForms.PageFields.Where(pf => pf.PanelUid == panel.Uid).OrderBy(pf => pf.FormFieldName).ToList());
        }

        ViewBag.fieldList = fieldList;

        #endregion cargar la lista de campos del formulario

        #region obtener estructura de la estrategia

        // lista de parametros procesados
        parameters = GetParameters(theStrategy.StrategyId);

        #endregion obtener estructura de la estrategia

        #region parametros de la estrategia

        // consultar los parametros de esa estrategia
        var parametersList = _stpcForms.StrategyParameter.Where(par => par.PageStrategyUid == theStrategy.Uid);

        #endregion parametros de la estrategia

        #region Asociar valores seleccionados

        // recorrer los parametros para asociar el valor seleccionado
        if (parametersList != null && parametersList.Count() > 0 && parameters != null && parameters.Count > 0)
        {
          foreach (var item in parameters)
          {
            foreach (var par in parametersList)
            {
              if (item.Name.Equals(par.ParameterName))
              {
                item.SelectedValue = par.FieldType != null ? par.FieldType.ToString() : par.PageFieldId.ToString();
                item.ParameterType = par.ParameterType;
                break;
              }
            }
          }
        }

        #endregion Asociar valores seleccionados

      }
      return PartialView(parameters);
    }

    //
    // POST: /Strategy/Edit/5

    [HttpPost]
    public JsonResult Edit(Guid EditKey, FormCollection campos)
    {
      try
      {
        PageStrategy theStrategy = _stpcForms.PageStrategy.Where(page => page.Uid == EditKey).FirstOrDefault();

        if (theStrategy == null)
          throw new Exception();

        // actualizar parametros asociados
        UpdateParameters(campos, theStrategy.Uid, theStrategy.StrategyId);

        return Json(new { success = true });
      }
      catch
      {
        return Json(new { success = false });
      }
    }

    //
    // GET: /Strategy/Delete/5

    public ActionResult Delete(int id)
    {
      return View();
    }

    //
    // POST: /Strategy/Delete/5

    [HttpPost]
    public ActionResult Delete(Guid pageStrategyUid)
    {
      if (pageStrategyUid == null) return View("Error");

      #region parametros de la estrategia

      // consultar los parametros de esa estrategia
      var parametersList = _stpcForms.StrategyParameter.Where(par => par.PageStrategyUid == pageStrategyUid);

      // eliminar los parametros
      if (parametersList != null && parametersList.Count() > 0)
      {
        foreach (var parameter in parametersList)
        {
          _stpcForms.DeleteObject(parameter);
        }
      }

      #endregion parametros de la estrategia

      #region estrategia

      // consultar la estrategia
      var theStrategy = _stpcForms.PageStrategy.Where(ps => ps.Uid == pageStrategyUid).FirstOrDefault();

      // elminar la estrategia
      if (theStrategy != null)
      {
        _stpcForms.DeleteObject(theStrategy);

        // guardar el set de cambios en el contexto
        _stpcForms.SaveChanges();

        return Json(new { Success = true });
      }

      #endregion estrategia

      return Json(new { Success = false });
    }

    /// <summary>
    /// Saves the parameters.
    /// </summary>
    /// <param name="campos">The campos.</param>
    /// <param name="newPageStrategyUid">The new page strategy uid.</param>
    /// <param name="pageName">Name of the page.</param>
    /// <param name="FormPageId">The form page id.</param>
    /// <param name="FormId">The form id.</param>
    /// <param name="StrategyId">The strategy id.</param>
    private void SaveParameters(FormCollection campos, Guid newPageStrategyUid, int StrategyId)
    {
      FormCollection par = new FormCollection();
      StrategyParameter newParameter;

      #region prepara las llaves-valor

      // prepara las llaves-valor
      int j = 0;
      for (int i = 0; i < campos.Count - 1; i = i + 2)
      {
        j = i + 1;
        if (!campos[i].Contains("Data_") && !campos[i].Contains("Trigger")
          && !campos[i].Contains("Response") && !campos[i].StartsWith("ddlParameterType_"))
          par.Add(campos[i] + "/" + i, campos[j]);
      }

      #endregion prepara las llaves-valor

      if (par.Count > 0)
      {
        #region obtiene los valores de los campos

        // obtiene los valores de los campos
        for (int i = 0; i < par.Count; i++)
        {
          newParameter = new StrategyParameter();
          newParameter.Uid = Guid.NewGuid();
          newParameter.PageStrategyUid = newPageStrategyUid;

          var fieldKey = ((System.Collections.Specialized.NameValueCollection)(par)).AllKeys[i];
          var fieldKeyProcessed = fieldKey.Split('/')[0];
          var fieldValue = par[fieldKey];

          // validar que el parametro contenga un valor 
          if (!string.IsNullOrEmpty(fieldValue))
          {
            bool isReadyToSave = false;

            #region validar si el key seleccionado es un campo

            // validar si el key seleccionado es un campo [fieldList] o un tipo [FieldTypeList]
            if (fieldKeyProcessed.StartsWith("fieldList"))
            {
              // obtener el nombre del parametro 
              newParameter.ParameterName = fieldKeyProcessed.Split(':')[1];

              // obtener el valor del parametro
              newParameter.PageFieldId = Guid.Parse(fieldValue);

              // almacenar el tipo de parametro
              newParameter.ParameterType = "Campo Formulario";

              // marcar el registro para guardar
              isReadyToSave = true;
            }

            #endregion validar si el key seleccionado es un campo

            #region el campo seleccionado es de ingreso manual

            else if (fieldKeyProcessed.StartsWith("txtManualValue"))
            {
              // obtener el nombre del parametro 
              newParameter.ParameterName = fieldKeyProcessed.Split(':')[1];

              // obtener el valor del parametro
              newParameter.FieldType = fieldValue;

              // almacenar el tipo de parametro
              newParameter.ParameterType = "Valor Manual";

              // marcar el registro para guardar
              isReadyToSave = true;
            }

            #endregion el campo seleccionado es de ingreso manual

            if (isReadyToSave)
              _stpcForms.AddObject("StrategyParameter", newParameter);
          }
        }

        #endregion obtiene los valores de los campos

        // guardar cambios
        _stpcForms.SaveChanges();

      }

    }

    /// <summary>
    /// Gets the parameters.
    /// </summary>
    /// <param name="StrategyId">The strategy id.</param>
    /// <returns></returns>
    private List<ParameterViewModel> GetParameters(int StrategyId)
    {
      // lista de parametros procesados
      List<ParameterViewModel> strategyParameters = new List<ParameterViewModel>();
      ParameterViewModel newParameter = null;

      // consulta la estrategia por el id
      var result = _decisionEngine.GetStrategySchema(StrategyId);

      // se convierte a XML
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.LoadXml(result);

      // se obtiene la lista de parametros para la estrategia
      XmlNodeList parameterList = xmlDoc.GetElementsByTagName("VariableDescriptors");

      // recorrer la lista de parametros  
      foreach (XmlElement parameter in parameterList)
      {
        // se obtiene un parametro
        XmlNodeList lista = parameter.GetElementsByTagName("VariableDescriptor");

        #region procesar nodo XML

        foreach (XmlElement nodo in lista)
        {
          newParameter = new ParameterViewModel();

          // obtener la estructura del parametro
          XmlNodeList parameterName = nodo.GetElementsByTagName("Name");
          newParameter.Name = parameterName[0].InnerText;

          XmlNodeList parameterTypeName = nodo.GetElementsByTagName("TypeName");
          #region procesar el tipo de dato
          switch (parameterTypeName[0].InnerText)
          {
            case "String":
              newParameter.Type = "Texto";
              break;

            case "Numeric":
              newParameter.Type = "Número";
              break;

            case "Date":
              newParameter.Type = "Fecha";
              break;

            case "Boolean":
              newParameter.Type = "Booleano";
              break;

            default:
              newParameter.Type = parameterTypeName[0].InnerText;
              break;
          }
          #endregion procesar el tipo de dato

          XmlNodeList parameterInitialValue = nodo.GetElementsByTagName("InitialValue");
          newParameter.InitialValue = parameterInitialValue[0].InnerText;

          XmlNodeList parameterDescription = nodo.GetElementsByTagName("Description");
          newParameter.Description = parameterDescription[0].InnerText;

          XmlNodeList parameterIsInput = nodo.GetElementsByTagName("IsInput");
          newParameter.IsInput = (!string.IsNullOrEmpty(parameterIsInput[0].InnerText) && parameterIsInput[0].InnerText.Equals("True")) ? true : false;

          XmlNodeList parameterIsOutput = nodo.GetElementsByTagName("IsOutput");
          newParameter.IsOutput = (!string.IsNullOrEmpty(parameterIsOutput[0].InnerText) && parameterIsOutput[0].InnerText.Equals("True")) ? true : false;

          XmlNodeList parameterConstraints = nodo.GetElementsByTagName("Constraints");
          newParameter.Constraints = parameterConstraints[0].InnerText;

          newParameter.SelectedValue = string.Empty;

          strategyParameters.Add(newParameter);
        }

        #endregion procesar nodo XML
      }

      return strategyParameters;
    }

    /// <summary>
    /// Gets the parameter type list.
    /// </summary>
    /// <returns></returns>
    private List<string> GetParameterTypeList()
    {
      List<string> resultList = null;

      resultList = new List<string>();
      resultList.Add("Campo Formulario");
      resultList.Add("Valor Manual");

      return resultList;
    }


    /// <summary>
    /// Updates the parameters.
    /// </summary>
    /// <param name="campos">The campos.</param>
    /// <param name="PageStrategyUid">The page strategy uid.</param>
    /// <param name="StrategyId">The strategy id.</param>
    private void UpdateParameters(FormCollection campos, Guid PageStrategyUid, int StrategyId)
    {
      FormCollection newParList = new FormCollection();

      #region prepara las llaves-valor

      // prepara las llaves-valor
      int j = 1;
      for (int i = 1; i < campos.Count - 1; i = i + 2)
      {
        j = i + 1;
        if (!campos[i].Contains("Data_") && !campos[i].Equals("EditKey"))
          newParList.Add(campos[i] + "/" + i, campos[j]);
      }

      #endregion prepara las llaves-valor

      #region parametros de la estrategia

      // consultar los parametros de esa estrategia
      var parametersList = _stpcForms.StrategyParameter.Where(param => param.PageStrategyUid == PageStrategyUid);

      #endregion parametros de la estrategia

      #region actualizar valores de los parametros

      // recorrer la lista de parametros asociados
      if (parametersList != null && parametersList.Count() > 0)
      {
        foreach (var parameter in parametersList)
        {
          // recorrer la lista de nuevos valores
          if (newParList.Count > 0)
          {
            #region obtiene los valores de los campos

            // obtiene los valores de los campos
            for (int i = 0; i < newParList.Count; i++)
            {
              var fieldKey = ((System.Collections.Specialized.NameValueCollection)(newParList)).AllKeys[i];
              var fieldKeyProcessed = fieldKey.Split('/')[0];
              var fieldValue = newParList[fieldKey];

              // validar que el parametro contenga un valor 
              if (!string.IsNullOrEmpty(fieldValue))
              {
                #region [fieldList]

                // validar si el key seleccionado es un campo [fieldList] o un tipo [FieldTypeList]
                if (fieldKeyProcessed.StartsWith("fieldList"))
                {
                  // obtener el nombre del parametro 
                  if (parameter.ParameterName.Equals(fieldKeyProcessed.Split(':')[1]))
                  {
                    // actualiza el valor del parametro
                    parameter.PageFieldId = Guid.Parse(fieldValue);
                    parameter.FieldType = null;
                    _stpcForms.UpdateObject(parameter);
                    break;
                  }
                }

                #endregion [fieldList]

                #region [FieldTypeList]

                else if (fieldKeyProcessed.StartsWith("FieldTypeList"))
                {

                  // obtener el nombre del parametro 
                  if (parameter.ParameterName.Equals(fieldKeyProcessed.Split(':')[1]))
                  {
                    // actualiza el valor del parametro
                    parameter.PageFieldId = null;
                    parameter.FieldType = fieldValue;
                    _stpcForms.UpdateObject(parameter);
                    break;
                  }
                }

                #endregion [FieldTypeList]
              }
            }

            #endregion obtiene los valores de los campos
          }
        }

        // guardar cambios
        _stpcForms.SaveChanges();

      }

      #endregion actualizar valores de los parametros

    }
  }
}
