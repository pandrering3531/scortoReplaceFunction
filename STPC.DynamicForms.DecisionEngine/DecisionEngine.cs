using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace STPC.DynamicForms.DecisionEngine
{
	public class DecisionEngine : IDecisionEngine
	{
		protected ScortoWSProxy.ScoringService service = null;
		protected bool loggedIn = false;
		protected string user;
		protected string password;
		protected int interval = 0;
		protected int timeOut = 0;

		public DecisionEngine(string user, string password, int pollInterval = 1000, int timeOut = 120000)
		{
			this.service = new ScortoWSProxy.ScoringService();
			this.service.Url = System.Configuration.ConfigurationManager.AppSettings["DecisionEngineUrl"];
			this.user = user;
			this.password = password;
			this.service.TicketValue = new ScortoWSProxy.Ticket();
			this.interval = pollInterval;
			this.timeOut = timeOut;
		}

		protected void Login()
		{
			if (this.loggedIn == false)
			{
				this.LoginUser(user, password);
				this.loggedIn = true;
			}
		}

		public string CallStrategy(int stratId, string parameters, string userId)
		{
			Login();

			//Consulta estructura de parametros para la estrategia.
			XmlDocument _xmlDocument = new XmlDocument();

			//Carga string con la estructura xml de los parametros de la estrategia.
			string stringParameters = this.GetStrategyExampleXML(stratId);

			if (!string.IsNullOrEmpty(stringParameters))
				//Construye xml con base en la estructura retronada por el ws.
				_xmlDocument.LoadXml(this.GetStrategyExampleXML(stratId));

			//string resultLastNode=_xmlDocument.DocumentElement.LastChild.Attributes.Item(0).Value;

			if (_xmlDocument.GetElementsByTagName("requestId").Count > 0)
				_xmlDocument.GetElementsByTagName("requestId").Item(0).InnerXml = parameters;

			if (_xmlDocument.GetElementsByTagName("userId").Count > 0)
				_xmlDocument.GetElementsByTagName("userId").Item(0).InnerXml = userId;
			//_xmlDocument.DocumentElement.LastChild.InnerText = userId;
			//_xmlDocument.DocumentElement.FirstChild.InnerText = parameters;

			long appId = CreateApplicationByStrategyId(stratId, _xmlDocument.InnerXml);
			int totalTime = 0;
			while (true)
			{
				string status = service.GetApplicationStatus(appId);
				if (status.Contains("processed") || status.Contains("processed_with_errors"))
				{
					break;
				}
				Thread.Sleep(this.interval);
				totalTime += this.interval;
				if (totalTime > this.timeOut)
				{
					throw new DecisionEngineTimeoutException(totalTime);
				}
			}
			string Rrespuesta = service.GetApplicationResult(appId);
			return Rrespuesta;
		}

		public string CallStrategy(int stratId, string parameters)
		{
			Login();

			//Consulta estructura de parametros para la estrategia.
			XmlDocument _xmlDocument = new XmlDocument();

			//Carga string con la estructura xml de los parametros de la estrategia.
			string stringParameters = this.GetStrategyExampleXML(stratId);

			if (!string.IsNullOrEmpty(stringParameters))
				//Construye xml con base en la estructura retronada por el ws.
				_xmlDocument.LoadXml(this.GetStrategyExampleXML(stratId));

			//string resultLastNode=_xmlDocument.DocumentElement.LastChild.Attributes.Item(0).Value;

			_xmlDocument.DocumentElement.LastChild.InnerText = parameters;


			long appId = CreateApplicationByStrategyId(stratId, _xmlDocument.InnerXml);
			int totalTime = 0;
			while (true)
			{
				string status = service.GetApplicationStatus(appId);
				if (status.Contains("processed") || status.Contains("processed_with_errors"))
				{
					break;
				}
				Thread.Sleep(this.interval);
				totalTime += this.interval;
				if (totalTime > this.timeOut)
				{
					throw new DecisionEngineTimeoutException(totalTime);
				}
			}
			string Rrespuesta = service.GetApplicationResult(appId);
			return Rrespuesta;
		}


		public string CallPageStrategy(int stratId, Dictionary<string, string> dicDataPage)
		{
			Login();

			//Consulta estructura de parametros para la estrategia.
			XmlDocument _xmlDocument = new XmlDocument();


			string valueParameter = string.Empty;
			string keyParameter = string.Empty;

			//Carga string con la estructura xml de los parametros de la estrategia.
			string stringParameters = this.GetStrategyExampleXML(stratId);

			if (!string.IsNullOrEmpty(stringParameters))
				_xmlDocument.LoadXml(stringParameters);

			//Carga la lista de nodos del XML
			XmlNodeList _XmlNodeList = _xmlDocument.SelectNodes("/application_data");

			//Recorre cada uno de los nodos
			foreach (XmlNode item in _XmlNodeList[0].ChildNodes)
			{

				var value = item.LastChild.InnerText;
				var fieldName = item.Name;

				//Busca el nodo en el xml enviado por el formulario
				var valueFieldPage = dicDataPage.Where(e => e.Key == fieldName).FirstOrDefault();

				if (!string.IsNullOrEmpty(valueFieldPage.Value))
					item.LastChild.InnerText = valueFieldPage.Value;
			}

			long appId = CreateApplicationByStrategyId(stratId, _xmlDocument.InnerXml);
			int totalTime = 0;
			while (true)
			{
				string status = service.GetApplicationStatus(appId);
				if (status.Contains("processed") || status.Contains("processed_with_errors"))
				{
					break;
				}
				Thread.Sleep(this.interval);
				totalTime += this.interval;
				if (totalTime > this.timeOut)
				{
					throw new DecisionEngineTimeoutException(totalTime);
				}
			}
			string resultDecisionEngine = string.Empty;
			resultDecisionEngine = service.GetApplicationResult(appId);


			return resultDecisionEngine;

		}


		//---------------------------------------------------------------------

		/// <summary>
		/// Calls the strategy parameters.
		/// </summary>
		/// <param name="stratId">The strat id.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		/// <exception cref="DecisionEngineTimeoutException"></exception>
		public string CallStrategyParameters(int stratId, Dictionary<string, string> parameters)
		{
			Login();

			//Consulta estructura de parametros para la estrategia.
			XmlDocument _xmlDocument = new XmlDocument();

			//Carga string con la estructura xml de los parametros de la estrategia.
			string stringParameters = this.GetStrategyExampleXML(stratId);

			if (!string.IsNullOrEmpty(stringParameters))
				//Construye xml con base en la estructura retornada por el ws.
				_xmlDocument.LoadXml(stringParameters);

			// se obtiene la lista de parametros para la estrategia
			XmlNodeList parameterList = _xmlDocument.GetElementsByTagName("application_data");

			#region procesa la lista de parametros

			KeyValuePair<string, string> pair;
			// recorrer la lista de parametros
			foreach (XmlElement parameterXML in parameterList)
			{
				for (int i = 0; i < parameterXML.ChildNodes.Count; i++)
				{
					pair = parameters.Where(e => e.Key == parameterXML.ChildNodes[i].Name).FirstOrDefault();
					if (pair.Value != null)
					{
						parameterXML.ChildNodes[i].InnerXml = pair.Value;
					}
					else
					{
						if (parameterXML.ChildNodes[i].Name.StartsWith("var"))
							parameterXML.ChildNodes[i].InnerXml = string.Empty;
						if (parameterXML.ChildNodes[i].Name.StartsWith("mon"))
							parameterXML.ChildNodes[i].InnerXml = "0";
						if (parameterXML.ChildNodes[i].Name.StartsWith("int"))
							parameterXML.ChildNodes[i].InnerXml = "0";
						if (parameterXML.ChildNodes[i].Name.StartsWith("dat"))
							parameterXML.ChildNodes[i].InnerXml = DateTime.MinValue.ToString();
					}
				}
			}

			#endregion procesa la lista de parametros

			long appId = CreateApplicationByStrategyId(stratId, _xmlDocument.InnerXml);
			int totalTime = 0;
			while (true)
			{
				string status = service.GetApplicationStatus(appId);
				if (status.Contains("processed") || status.Contains("processed_with_errors"))
				{
					break;
				}
				if (status.Contains("unknown"))
				{
					throw new Exception(service.GetApplicationResult(appId));
				}

				Thread.Sleep(this.interval);
				totalTime += this.interval;

				if (totalTime > this.timeOut)
				{
					throw new DecisionEngineTimeoutException(totalTime);
				}
			}
			return service.GetApplicationResult(appId);
		}

		public IList<string> GetList(int stratId, string paramname, string inputval, string listIdentifier, string userId)
		{
			XmlDocument doc = new XmlDocument();
			var list = new List<string>();

			string result = CallStrategy(stratId, inputval, userId);
			doc.LoadXml(result);
			var node = doc.SelectSingleNode("/application_result/" + listIdentifier + "[1]");
			if (node != null)
				foreach (XmlNode child in node)
				{
					list.Add(child.Attributes["uid"].Value + "|" + child.Attributes["value"].Value);
				}

			return list;
		}

		//        <?xml version="1.0" encoding="utf-8"?>
		//<application_data>
		//    <SPcategoria>Sexo</SPcategoria>
		//</application_data>

		//        <?xml version="1.0" encoding="utf-8"?>
		//<application_result>
		//    <SPcust><row Categoria="Sexo" Codigo="1" Valor="Masculino"/><row Categoria="Sexo" Codigo="2" Valor="Femenino"/><row Categoria="Sexo" Codigo="3" Valor="Sin Descripcion"/></SPcust>
		//</application_result>

		public IDictionary<string, string> GetDictionary(int stratId, string procName, string inputval, string listIdentifier)
		{
			XmlDocument doc = new XmlDocument();
			var a = doc.CreateXmlDeclaration("1.0", "utf-8", null);
			doc.AppendChild(a);
			var el = doc.CreateElement("application_data");
			var el1 = doc.CreateElement(procName);
			el1.InnerText = inputval;
			el.AppendChild(el1);
			doc.AppendChild(el);
			string result = CallStrategy(stratId, doc.OuterXml);
			doc.LoadXml(result);
			var nodelist = doc.SelectNodes(listIdentifier);
			var dictionary = new Dictionary<string, string>();
			foreach (XmlNode child in nodelist)
			{
				dictionary.Add(child.Attributes["Codigo"].Value, child.Attributes["Valor"].Value);
			}
			return dictionary;
		}

		public IDictionary<string, string> GetDictionaryAlt(int stratId, string procName, string inputval)
		{
			XmlDocument doc = new XmlDocument();
			var a = doc.CreateXmlDeclaration("1.0", "utf-8", null);
			doc.AppendChild(a);
			var el = doc.CreateElement("application_data");
			var el1 = doc.CreateElement(procName);
			el1.InnerText = inputval;
			el.AppendChild(el1);
			doc.AppendChild(el);
			string result = CallStrategy(stratId, doc.OuterXml);
			doc.LoadXml(result);
			var nodelist = doc.SelectNodes("//" + inputval);
			var dictionary = new Dictionary<string, string>();
			foreach (XmlNode child in nodelist)
			{
				dictionary.Add(child.Attributes["Codigo"].ToString(), child.Attributes["Valor"].ToString());
			}
			return dictionary;
		}

		//        <?xml version="1.0" encoding="utf-8"?>
		//<application_data>
		//    <strNombreVariable>prueba</strNombreVariable>
		//    <TipoCampo>1</TipoCampo>
		//    <ValorNumerico>10</ValorNumerico>
		//    <ValorCadena>Example string</ValorCadena>
		//    <ValorFecha>6/8/2012 5:11:42 PM</ValorFecha>
		//</application_data>

		//        <?xml version="1.0" encoding="utf-8"?>
		//<application_result>
		//    <intSalida>-1</intSalida>
		//    <strMensaje>Mensaje de Error Valor</strMensaje>
		//</application_result>
		public bool ValidateData(int stratId, string name, int type, object value, out string message)
		{
			bool retVal = false;
			message = "Error";
			XmlDocument doc = new XmlDocument();
			var a = doc.CreateXmlDeclaration("1.0", "utf-8", null);
			doc.AppendChild(a);
			var el = doc.CreateElement("application_data");
			var el1 = doc.CreateElement("strNombreVariable");
			el1.InnerText = name;
			var el2 = doc.CreateElement("TipoCampo");
			el2.InnerText = type.ToString();
			var el3 = doc.CreateElement("ValorNumerico");
			var el4 = doc.CreateElement("ValorCadena");
			var el5 = doc.CreateElement("ValorFecha");
			el5.InnerText = DateTime.Now.ToShortDateString();
			switch (type)
			{
				case 1:
					el3.InnerText = value.ToString();
					break;
				case 2:
					el4.InnerText = value.ToString();
					break;
				case 3:
					el5.InnerText = value.ToString();
					break;
			}
			el.AppendChild(el1);
			el.AppendChild(el2);
			el.AppendChild(el3);
			el.AppendChild(el4);
			el.AppendChild(el5);
			doc.AppendChild(el);
			string result = CallStrategy(stratId, doc.OuterXml);
			doc.LoadXml(result);
			var node = doc.SelectSingleNode("//strMensaje");
			if (node != null)
				message = node.InnerText;
			node = doc.SelectSingleNode("//intSalida");
			int x = 0;
			if (Int32.TryParse(node.InnerText, out x))
				if (x == 0)
					retVal = true;
			return retVal;
		}

		public IList<StrategyData> GetStrategyList()
		{
			Login();
			XmlDocument doc = new XmlDocument();
			string result = this.service.GetStrategyList();
			doc.LoadXml(result);
			var nodelist = doc.SelectNodes("//strategy_brief");
			var list = new List<StrategyData>();
			foreach (XmlNode node in nodelist)
			{
				StrategyData a = new StrategyData();
				foreach (XmlNode child in node.ChildNodes)
				{
					if (child.Name == "id")
						a.Id = Int32.Parse(child.InnerText);
					if (child.Name == "name")
						a.Name = child.InnerText;
					if (child.Name == "termination_date")
					{
						DateTime b;
						DateTime.TryParse(child.InnerText, out b);
						a.Termination_date = b;
					}
					if (child.Name == "description")
						a.Description = child.InnerText;
				}
				list.Add(a);
			}
			return list;

		}

		public long CreateApplicationByStrategyId(int stratId, string parameters)
		{
			Login();
			return this.service.CreateApplicationByStrategyId(stratId, parameters);
		}

		public long CreateApplicationByPublicNameId(int id, string parameters)
		{
			Login();
			return this.service.CreateApplicationByPublicNameId(id, parameters);
		}

		public string GetApplicationStatus(long appId)
		{
			Login();
			return this.service.GetApplicationStatus(appId);
		}

		public string GetApplicationResult(long appId)
		{
			Login();
			return this.service.GetApplicationResult(appId);
		}

		public string GetPublicNameList()
		{
			Login();
			return this.service.GetPublicNameList();
		}

		public string GetStrategyExampleXML(int stratId)
		{
			Login();
			return this.service.GetStrategyExampleXML(stratId);
		}

		public string GetStrategySchema(int stratId)
		{
			Login();
			return this.service.GetStrategySchema(stratId);
		}

		public string LoginUser(string user, string password)
		{
			try
			{
				string str = this.service.LoginUser(user, password);
				this.service.TicketValue.TicketId = str;
			}
			catch (System.Web.Services.Protocols.SoapException se)
			{
				throw new DecisionEngineLogonException(se);
			}
			return this.service.TicketValue.TicketId;
		}

	}
}