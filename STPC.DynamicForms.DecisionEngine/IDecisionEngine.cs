using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.DecisionEngine
{
	public interface IDecisionEngine
	{
		//New Methods
		string CallStrategy(int stratId, string parameters, string userId);
		string CallStrategy(int stratId, string parameters);
		string CallPageStrategy(int stratId, Dictionary<string, string> dicDataPage);
		string CallStrategyParameters(int stratId, Dictionary<string, string> parameters);
		IList<string> GetList(int stratId, string paramname, string inputval, string listIdentifier, string userName);
		IDictionary<string, string> GetDictionary(int stratId, string paramname, string inputval, string listIdentifier);
		IDictionary<string, string> GetDictionaryAlt(int stratId, string procName, string inputval);
		bool ValidateData(int stratId, string name, int type, object value, out string message);
		//Existent
		IList<StrategyData> GetStrategyList();
		long CreateApplicationByStrategyId(int stratId, string parameters);
		long CreateApplicationByPublicNameId(int id, string parameters);
		string GetApplicationStatus(long appId);
		string GetApplicationResult(long appId);
		string GetPublicNameList();
		string GetStrategyExampleXML(int stratId);
		string GetStrategySchema(int stratId);
		string LoginUser(string user, string password);
	}

  public class StrategyData
  {
	 public int Id { get; set; }
	 public string Name { get; set; }
	 public DateTime? Termination_date { get; set; }
	 public string Description { get; set; }
  }
}