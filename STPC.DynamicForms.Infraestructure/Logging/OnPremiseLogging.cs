using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.Infraestructure.Logging
{
	 class OnPremiseLogging : ILogging 
	 {
		 #region Constants and Attributes

		 // Proyecto Origen del Log de eventos
		 private static string _source = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EventLogSource"]) ? System.Configuration.ConfigurationManager.AppSettings["EventLogSource"] : "LiSim ABC";

		 // Nombre del Log de la Aplicacion
		 private static string _name = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EventLogName"]) ? System.Configuration.ConfigurationManager.AppSettings["EventLogName"] : "LiSim ABC - EventLog";

		 // Encabezado con la fecha para registrar un evento
		 private static string _header = "[ " + DateTime.Now + "] : ";

		 #endregion Constants and Attributes

		  #region Metodos Privados
		  #endregion

		  #region Metodos Publicos

		  /// <summary>
		  /// Writes the line in the event log.
		  /// </summary>
		  /// <param name="iMessage">The message.</param>
		  void WriteEventLog(string iMessage, EventLogEntryType iEventType)
		  {
				try
				{
					 // validar si ya existe un registo en el Event Log
					 if (!EventLog.SourceExists(_source))
						 EventLog.CreateEventSource(_source, _name);

					 EventLog.WriteEntry(_name, _header + iMessage, iEventType);
				}
				catch (Exception Ex)
				{
					 WriteTrace(Ex.Message);
				}

		  }

		  /// <summary>
		  /// Writes the event log.
		  /// Default Event Type: Warning
		  /// </summary>
		  /// <param name="iMessage">The i message.</param>    
		  void WriteEventLog(string iMessage)
		  {
				try
				{
					 // validar si ya existe un registo en el Event Log
					if (!EventLog.SourceExists(_source))
						EventLog.CreateEventSource(_source, _name);

					EventLog.WriteEntry(_name, _header + iMessage, EventLogEntryType.Warning);
				}
				catch (Exception Ex)
				{
					 WriteTrace(Ex.Message);
				}

		  }

		  //---------------------------------------------------------------------------------------
		  // escribir en el trace de la aplicacion
		  //---------------------------------------------------------------------------------------

		  /// <summary>
		  /// Writes the trace.
		  /// </summary>
		  /// <param name="iMessage">The i message.</param>
		  void WriteTrace(string iMessage)
		  {
				// Escribir en el registro de eventos Trace
			  Trace.WriteLine("[ " + DateTime.Now + " ] " + iMessage, _source);

				// Escribir en el registro de eventos Debug
			  Debug.WriteLine("[ " + DateTime.Now + " ] " + iMessage, _name);
		  }

		  //---------------------------------------------------------------------------------------


		  public void WriteLog(string iMessage, EventLogEntryType iEventType)
		  {
				WriteEventLog(iMessage, iEventType);
				WriteTrace(iMessage);
		  }

		  /// <summary>
		  /// Writes the log.
		  /// </summary>
		  /// <param name="iMessage">The i message.</param>
		  public void WriteLog(string iMessage)
		  {
				WriteEventLog(iMessage);
				WriteTrace(iMessage);
		  }

		  #endregion
	 }
}