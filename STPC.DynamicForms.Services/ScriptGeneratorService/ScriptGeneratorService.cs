using STPC.DynamicForms.Web.Models;
using STPC.DynamicForms.Infraestructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace STPC.DynamicForms.Services.ScriptGeneratorService
{
	public class ScriptGeneratorService : IScriptGeneratorService
	{
		public string GenerateScriptString(Guid formId)
		{
			try
			{
				ScriptGenerator sc = new ScriptGenerator();
				return sc.GenerateScriptString(formId);
			}

			catch (Exception ex)
			{
				Guid correlationID = Guid.NewGuid();
				ILogging eventWriter = LoggingFactory.GetInstance();
				string errorMessage = string.Format("Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
				System.Diagnostics.Debug.WriteLine(errorMessage);
				eventWriter.WriteLog(errorMessage);
				throw ex;
			}
		}
	}
}