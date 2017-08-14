using STPC.DynamicForms.Core;
using STPC.DynamicForms.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using System.Xml;

namespace STPC.DynamicForms.Services.RequestService
{

	[ServiceContract]
	public interface IRequestService
	{
		[OperationContract]
		IEnumerable<STPC.DynamicForms.Web.Models.Request> GetRequest(Dictionary<string, string> values);

		[OperationContract]
		MyViewModel GetRequestDynamic(List<ReportParameters> values, Dictionary<string, string> ParametersRequestTable, string RequestType);

		[OperationContract]
		MyViewModel GetRequestDynamicSearcher(List<ReportParameters> values, Dictionary<string, string> ParametersRequestTable, string RequestType);

		[OperationContract]
		MyViewModel GetRequestByProcedure(string iStoredProcedureName);

		[OperationContract]
		MyViewModel GetIndicatorByProcedure(string storedProcedure, Dictionary<string, string> parameters);

		[OperationContract]
		MyViewModel GetRequestsByParamProcedure(string iStoredProcedureName, string userName, int offSet, int fetch, string filter);

		[OperationContract]
		MyViewModel GetSchemaTable(string tableName, int nodeId);

		[OperationContract]
		void InsertAtributesHierarchy(Dictionary<string, string> listFieldsValues, string table);

		[OperationContract]
		void UpdateAtributesHierarchy(Dictionary<string, string> listFieldsValues, string table);

		[OperationContract]
		int FindRecordIntoAtributeTable(string tableName, int nodeId);

		[OperationContract]
		int CreateRequest(STPC.DynamicForms.Web.Models.Request request);

		

		[OperationContract]
		string CreatePageFlowStepInstance(STPC.DynamicForms.Web.Models.Request request, Guid formPageId, string formName, string xml);

		[OperationContract]
		string NewPageFlowStepInstance(STPC.DynamicForms.Web.Models.Request request, FormData pageData);

		[OperationContract]
		List<Values> GetPageFlowStepInstance(int requestId, Guid formPageId, Guid PageFlowId, string formName);

		[OperationContract]
		STPC.DynamicForms.Web.Models.Request GetRequestById(int iRequestId);

		[OperationContract]
		void UpdateRequest(STPC.DynamicForms.Web.Models.Request request, Guid formPageId, string formName);

		[OperationContract]
		void InsertDecisionEngineResult(System.Xml.XmlElement xmlDecisionEngineResult, int requestId, string updatedBy, string tableToInsert);

		[OperationContract]
		List<FormPageActions> GetUserFormPageActionsByState(string roleName, Guid pageId, Guid formStateId);

		[OperationContract]
		List<PageField> getFieldByPage(Guid uid);
	}
}