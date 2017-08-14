using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using STPC.DynamicForms.Core;
using STPC.DynamicForms.Web.RT.Services.Request;
using System.Xml;
using System.Data;

namespace STPC.DynamicForms.Web.RT.Helpers
{
	public class CustomRequestProvider
	{
		public IEnumerable<Request> GetRequest(Dictionary<string, string> values)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.GetRequest(values);
			}
		}
		public MyViewModel GetRequestDinamic(List<ReportParameters> values, Dictionary<string, string> ParametersRequestTable, string RequestType)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.GetRequestDynamic(values, ParametersRequestTable, RequestType);
			}
		}


		public MyViewModel GetRequestDynamicSearcher(List<ReportParameters> values, Dictionary<string, string> ParametersRequestTable, string RequestType)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.GetRequestDynamicSearcher(values, ParametersRequestTable, RequestType);
			}
		}

		public MyViewModel GetRequestByProcedure(string storeProcedureName)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.GetRequestByProcedure(storeProcedureName);
			}
		}

		public MyViewModel GetIndicatorByProcedure(string storeProcedureName, Dictionary<string, string> parameters)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.GetIndicatorByProcedure(storeProcedureName, parameters);
			}
		}


		public MyViewModel GetRequestsByParamProcedure(string storedProcedureName, string userName, int offSet, int fetch, string filter)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.GetRequestsByParamProcedure(storedProcedureName, userName, offSet, fetch, filter);
			}
		}

		public int CreateRequest(Request request)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.CreateRequest(request);
			}

		}

		public string CreatePageFlowStepInstance(Request request, Guid formPageId, string formName, string xml)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.CreatePageFlowStepInstance(request, formPageId, formName, xml);
			}
		}

		public string NewPageFlowStepInstance(Request request,  STPC.DynamicForms.Web.RT.Services.Request.FormData data)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.NewPageFlowStepInstance(request, data);
			}
		}

		public List<Values> GetPageFlowStepInstance(int requestId, Guid formPageId, Guid PageFlowId, string formName)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.GetPageFlowStepInstance(requestId, formPageId, PageFlowId, formName);
			}
		}
		public void UpdateRequest(Request request, Guid formPageId, string formName)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				client.UpdateRequest(request, formPageId, formName);
			}

		}
		
		public void InsertDecisionEngineResult(XmlElement xmlDecisionEngineScortoResult, int requestId, string updatedBy, string tableToInsert)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				
				client.InsertDecisionEngineResult(xmlDecisionEngineScortoResult, requestId, updatedBy, tableToInsert);
				
			}

		}

		public List<Common.Services.Users.PageField> getFieldByPage(Guid uid)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				List<Common.Services.Users.PageField> listPageFields = client.getFieldByPage(uid);				

				return client.getFieldByPage(uid);
			}
		}


		public List<Common.Services.Users.FormPageActions> GetUserFormPageActionsByState(string userName, Guid pageId, Guid formStateId)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.GetUserFormPageActionsByState(userName, pageId, formStateId);
			}
		}



		//DT
		public void InsertAtributesHierarchy(Dictionary<string, string> listFieldsValues, string table)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				client.InsertAtributesHierarchy(listFieldsValues, table);
			}
		}
		public int FindRecordIntoAtributeTable(string tableName, int nodeId)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.FindRecordIntoAtributeTable(tableName, nodeId);
			}
		}
		public void UpdateAtributesHierarchy(Dictionary<string, string> listFieldsValues, string table)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				client.UpdateAtributesHierarchy(listFieldsValues, table);
			}
		}
		public MyViewModel GetSchemaTable(string nombreTabla, int nodeId)
		{
			using (RequestServiceClient client = new RequestServiceClient())
			{
				return client.GetSchemaTable(nombreTabla, nodeId);
			}
		}
	}
}