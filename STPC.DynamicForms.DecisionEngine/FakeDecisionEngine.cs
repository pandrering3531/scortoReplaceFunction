using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.DecisionEngine
{
  public class FakeDecisionEngine : IDecisionEngine
  {
	  public string CallStrategy(int stratId, string parameters, string userId)
	 {
		throw new NotImplementedException();
	 }
	  public string CallStrategy(int stratId, string parameters)
	  {
		  throw new NotImplementedException();
	  }
	 public IList<string> GetList(int stratId, string paramname, string inputval, string listIdentifier, string userName)
	 {
		if (stratId == 3)
		  return new List<string> { "Credito de consumo", "Credito de vehiculo", "Credito Hipotecario" };
		if (stratId == 4)
		  return new List<string> { "hasta 1M", "2-3M", "3-4M", "Mas de 4M" };
		return new List<string>();
	 }

	 public IDictionary<string, string> GetDictionary(int stratId, string paramname, string inputval, string listIdentifier)
	 {
		throw new NotImplementedException();
	 }

	 public IDictionary<string, string> GetDictionaryAlt(int stratId, string procName, string inputval)
	 {
		throw new NotImplementedException();
	 }

	 public bool ValidateData(int stratId, string name, int type, object value, out string message)
	 {
		throw new NotImplementedException();
	 }

	 public IList<StrategyData> GetStrategyList()
	 {
		return new List<StrategyData> {
					 new StrategyData { Id=1, Name="Validar mayor de edad"}, 
					 new StrategyData {Id=2,Name="Validad capacidad endeudamiento"},
					 new StrategyData {Id=3,Name="Lista de productos"},
					 new StrategyData {Id=4,Name="Lista de escalas salariales"},
					 new StrategyData {Id=5,Name="Persistir datos del credito"},
					 new StrategyData {Id=6,Name="Persistir info solicitante"},
					 new StrategyData {Id=7,Name="Persistir info ingresos"}
				};
	 }

	 public long CreateApplicationByStrategyId(int stratId, string parameters)
	 {
		throw new NotImplementedException();
	 }

	 public long CreateApplicationByPublicNameId(int id, string parameters)
	 {
		throw new NotImplementedException();
	 }

	 public string GetApplicationStatus(long appId)
	 {
		throw new NotImplementedException();
	 }

	 public string GetApplicationResult(long appId)
	 {
		throw new NotImplementedException();
	 }

	 public string GetPublicNameList()
	 {
		throw new NotImplementedException();
	 }

	 public string GetStrategyExampleXML(int stratId)
	 {
		throw new NotImplementedException();
	 }

	 public string GetStrategySchema(int stratId)
	 {
		throw new NotImplementedException();
	 }

	 public string LoginUser(string user, string password)
	 {
		throw new NotImplementedException();
	 }

	 public string CallStrategyParameters(int stratId, Dictionary<string, string> parameters)
	 {
		throw new NotImplementedException();
	 }

	 public string CallPageStrategy(int stratId, Dictionary<string, string> dicDataPage)
	 {
		 throw new NotImplementedException();
	 }

  }
}