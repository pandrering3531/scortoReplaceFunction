using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace STPC.DynamicForms.Services.ScriptGeneratorService
{
  [ServiceContract]
  public interface IScriptGeneratorService
  {
    [OperationContract]
    string GenerateScriptString(Guid formId);
  }
}
