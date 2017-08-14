using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using STPC.DynamicForms.Core;
using STPC.DynamicForms.Web.Services.Request;
using System.Xml;

namespace STPC.DynamicForms.Web.Helpers
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


    public MyViewModel GetRequestProcedure(string storeProcedureName)
    {
      using (RequestServiceClient client = new RequestServiceClient())
      {
        return client.GetRequestByProcedure(storeProcedureName);
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


    public void UpdateRequest(Request request, Guid formPageId, string formName)
    {
      using (RequestServiceClient client = new RequestServiceClient())
      {
        client.UpdateRequest(request, formPageId, formName);
      }

    }

    public MyViewModel GetSchemaTable(string nombreTabla, int nodeId)
    {
      using (RequestServiceClient client = new RequestServiceClient())
      {
        return client.GetSchemaTable(nombreTabla, nodeId);
      }
    }

    public void InsertAtributesHierarchy(Dictionary<string, string> listFieldsValues, string table)
    {
      using (RequestServiceClient client = new RequestServiceClient())
      {
        client.InsertAtributesHierarchy(listFieldsValues, table);
      }
    }

    public void UpdateAtributesHierarchy(Dictionary<string, string> listFieldsValues, string table)
    {
      using (RequestServiceClient client = new RequestServiceClient())
      {
        client.UpdateAtributesHierarchy(listFieldsValues, table);
      }
    }

    public int FindRecordIntoAtributeTable(string tableName, int nodeId)
    {
      using (RequestServiceClient client = new RequestServiceClient())
      {
        return client.FindRecordIntoAtributeTable(tableName, nodeId);
      }
    }
  }
}