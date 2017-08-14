using STPC.DynamicForms.DecisionEngine;
using STPC.DynamicForms.Web.RT.Helpers;
using STPC.DynamicForms.Web.RT.Models;
using STPC.DynamicForms.Web.RT.Services.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace STPC.DynamicForms.Web.RT.Controllers
{
    
    
	public class StrategySettingsController : Controller
	{
		//
		// GET: /StrategySettings/
		Services.Entities.STPC_FormsFormEntities _stpcForms = new Services.Entities.STPC_FormsFormEntities(new Uri(ConfigurationManager.AppSettings["DataServicesURI"].ToString()));
		private IDecisionEngine _decisionEngine;

		public StrategySettingsController()
		{
			string user = System.Configuration.ConfigurationManager.AppSettings["UserWs"];
			string pws = System.Configuration.ConfigurationManager.AppSettings["PwsWs"];
			string pollInterval = System.Configuration.ConfigurationManager.AppSettings["pollInterval"];
			string timeOut = System.Configuration.ConfigurationManager.AppSettings["timeOut"];
			STPC.DynamicForms.DecisionEngine.DecisionEngine iEngine = new STPC.DynamicForms.DecisionEngine.DecisionEngine(user, pws, Convert.ToInt32(pollInterval), Convert.ToInt32(timeOut));
			this._decisionEngine = iEngine;

		}
		public ActionResult Index()
		{
			#region cargar la lista de estrategias disponibles

			try
			{
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
			}
			catch (Exception ex)
			{

				throw;
			}

			#endregion cargar la lista de estrategias disponibles

			return View();
		}

		[HttpPost]
		public ActionResult GetIndex(StrategySettings model, FormCollection par)
		{

			List<StrategySettings> thisCampaign = _stpcForms.StrategySettings.ToList();

			return PartialView("_Index", thisCampaign);
		}



		/// <summary>
		/// Gets the parameters.
		/// </summary>
		/// <param name="StrategyId">The strategy id.</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult GetParametersStrategy(int StrategyId)
		{
			// lista de parametros procesados

			List<StrategySettings> list = _stpcForms.StrategySettings.Where(st => st.StrategyID == StrategyId).ToList();
			//List<ParameterViewModel> parameters = GetParameters(Convert.ToInt32(StrategyId));

			return PartialView("_Index", list);
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


		[HttpPost]
		public ActionResult UpdateAttributeValue(Guid uid, string value, int strategyId)
		{

			StrategySettings _StrategySettings = _stpcForms.StrategySettings.Where(e => e.Uid == uid).FirstOrDefault();

			if (_StrategySettings != null)
			{
				string resultValidation = TypeValueValidator(_StrategySettings.ValueType, value);
				if (resultValidation!=string.Empty)
				{
					ViewBag.error = resultValidation;
				}
				else
				{
					_StrategySettings.Value = value;

					_stpcForms.UpdateObject(_StrategySettings);
					_stpcForms.SaveChanges();
					ViewBag.error = string.Empty;
				}
			}

			List<StrategySettings> list = _stpcForms.StrategySettings.Where(st => st.StrategyID == strategyId).ToList();
			//List<ParameterViewModel> parameters = GetParameters(Convert.ToInt32(StrategyId));

			return PartialView("_Index", list);


		}

		private string TypeValueValidator(string ValueType, string value)
		{


			if (ValueType.Equals("Numero"))
			{
				Int32 result;
				Int32.TryParse(value.ToString(), out result);

				return result == 0 ? "Error, ingrese un valor numérico válido" : string.Empty;

			}
			if (ValueType.Equals("Fecha"))
			{
				CultureInfo enUS = new CultureInfo("en-US"); 
				DateTime dateTime;
				string dateFormat = System.Configuration.ConfigurationManager.AppSettings["FormatoFecha"].ToString();

				if (!DateTime.TryParseExact(value.ToString(), dateFormat, enUS,
									DateTimeStyles.None, out dateTime))
				{
					return "Error, ingrese una fecha válida";
				}
				else
				{
					return string.Empty;
				}
				

			}
			return string.Empty; ;

		}

	}
}
