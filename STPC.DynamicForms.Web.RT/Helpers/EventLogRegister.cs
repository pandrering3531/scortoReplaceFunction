using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

namespace STPC.DynamicForms.Web.RT.Helpers
{
  public class EventLogRegister
  {
    #region Constants and Attributes

    // Proyecto Origen del Log de eventos
    private static string _source = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EventLogSource"]) ? System.Configuration.ConfigurationManager.AppSettings["EventLogSource"] : "LiSim ABC";

    // Nombre del Log de la Aplicacion
    private static string _name = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EventLogName"]) ? System.Configuration.ConfigurationManager.AppSettings["EventLogName"] : "LiSim ABC - EventLog";

    // Encabezado con la fecha para registrar un evento
    private static string _header = "[ " + DateTime.Now + "] : ";

    #endregion Constants and Attributes

    #region Public Properties

    #endregion Public Properties

    #region Contructor

    #endregion Contructor

    #region Private Methods
    #endregion Private Methods

    #region Public Methods

    /// <summary>
    /// Writes the event log.
    /// </summary>
    /// <param name="iMessage">The i message.</param>
    /// <param name="iEventType">Type of the i event.</param>
    public static void WriteEventLog(string iMessage, EventLogEntryType iEventType)
    {
      try
      {
        // validar si ya existe un registo en el Event Log
        if (!EventLog.SourceExists(_source))
          EventLog.CreateEventSource(_source, _name);

        EventLog.WriteEntry(_name, iMessage, iEventType);
        WriteTrace(iMessage);
      }
      catch (Exception Ex)
      {
        WriteTrace(Ex.Message);
      }

    }

    /// <summary>
    /// Writes the event log.
    /// </summary>
    /// <param name="iMessage">The i message.</param>
    public static void WriteEventLog(string iMessage)
    {
      try
      {
        // validar si ya existe un registo en el Event Log
        if (!EventLog.SourceExists(_source))
          EventLog.CreateEventSource(_source, _name);

        EventLog.WriteEntry(_name, iMessage, EventLogEntryType.Error);
        WriteTrace(iMessage);
      }
      catch (Exception Ex)
      {
        WriteTrace(Ex.Message);
      }

    }


    /// <summary>
    /// Writes the event log.
    /// </summary>
    /// <param name="iMessage">The i message.</param>
    /// <param name="iController">The i controller.</param>
    /// <param name="iAction">The i action.</param>
    /// <param name="iEventType">Type of the i event.</param>
    public static void WriteEventLog(string iMessage, string iController, string iAction, EventLogEntryType iEventType)
    {
      try
      {
        // validar si ya existe un registo en el Event Log
        if (!EventLog.SourceExists(_source))
          EventLog.CreateEventSource(_source, _name);

        StringBuilder CustomMessage = new StringBuilder();
        CustomMessage.AppendLine("Ha ocurrido un error procesando su solicitud.");
        CustomMessage.AppendLine("El origen de este comportamiento corresponde al controlador " + iController
          + " en la ejecucion de la accion " + iAction);
        CustomMessage.AppendLine("Detalle del error: " + iMessage);

        EventLog.WriteEntry(_name, CustomMessage.ToString(), iEventType);
        WriteTrace(CustomMessage.ToString());
      }
      catch (Exception Ex)
      {
        WriteTrace(Ex.Message);
      }

    }

    /// <summary>
    /// Writes the event log.
    /// </summary>
    /// <param name="iMessage">The i message.</param>
    /// <param name="iController">The i controller.</param>
    /// <param name="iAction">The i action.</param>
    public static void WriteEventLog(string iMessage, string iController, string iAction)
    {
      try
      {
        // validar si ya existe un registo en el Event Log
        if (!EventLog.SourceExists(_source))
          EventLog.CreateEventSource(_source, _name);

        StringBuilder CustomMessage = new StringBuilder();
        CustomMessage.AppendLine("Ha ocurrido un error procesando su solicitud.");
        CustomMessage.AppendLine("El origen de este comportamiento corresponde al controlador " + iController
          + " en la ejecucion de la accion " + iAction);
        CustomMessage.AppendLine("Detalle del error: " + iMessage);

        EventLog.WriteEntry(_name, CustomMessage.ToString(), EventLogEntryType.Error);
        WriteTrace(CustomMessage.ToString());
      }
      catch (Exception Ex)
      {
        WriteTrace(Ex.Message);
      }

    }


    /// <summary>
    /// Writes the event log.
    /// </summary>
    /// <param name="iMessage">The i message.</param>
    /// <param name="iController">The i controller.</param>
    /// <param name="iAction">The i action.</param>
    /// <param name="iCorrelationID">The i correlation ID.</param>
    /// <param name="iEventType">Type of the i event.</param>
    public static void WriteEventLog(string iMessage, string iController, string iAction, Guid iCorrelationID, EventLogEntryType iEventType)
    {
      try
      {
        // validar si ya existe un registo en el Event Log
        if (!EventLog.SourceExists(_source))
          EventLog.CreateEventSource(_source, _name);

        StringBuilder CustomMessage = new StringBuilder();
        CustomMessage.AppendLine("Ha ocurrido un error procesando su solicitud.");
        CustomMessage.AppendLine("El origen de este comportamiento corresponde al controlador " + iController
          + " en la ejecucion de la accion " + iAction);
        CustomMessage.AppendLine("Detalle del error: " + iMessage);
        CustomMessage.AppendLine("Codigo de Error: " + iCorrelationID.ToString());

        EventLog.WriteEntry(_name, CustomMessage.ToString(), iEventType);
        WriteTrace(CustomMessage.ToString());
      }
      catch (Exception Ex)
      {
        WriteTrace(Ex.Message);
      }

    }

    /// <summary>
    /// Writes the event log.
    /// </summary>
    /// <param name="iMessage">The i message.</param>
    /// <param name="iController">The i controller.</param>
    /// <param name="iAction">The i action.</param>
    /// <param name="iCorrelationID">The i correlation ID.</param>
    public static void WriteEventLog(string iMessage, string iController, string iAction, Guid iCorrelationID)
    {
      try
      {
        // validar si ya existe un registo en el Event Log
        if (!EventLog.SourceExists(_source))
          EventLog.CreateEventSource(_source, _name);

        StringBuilder CustomMessage = new StringBuilder();
        CustomMessage.AppendLine("Ha ocurrido un error procesando su solicitud.");
        CustomMessage.AppendLine("El origen de este comportamiento corresponde al controlador " + iController
          + " en la ejecucion de la accion " + iAction);
        CustomMessage.AppendLine("Detalle del error: " + iMessage);
        CustomMessage.AppendLine("Codigo de Error: " + iCorrelationID.ToString());

        EventLog.WriteEntry(_name, CustomMessage.ToString(), EventLogEntryType.Error);
        WriteTrace(CustomMessage.ToString());
      }
      catch (Exception Ex)
      {
        WriteTrace(Ex.Message);
      }

    }

    /// <summary>
    /// Writes the trace.
    /// </summary>
    /// <param name="iMessage">The i message.</param>
    public static void WriteTrace(string iMessage)
    {
      // Escribir en el registro de eventos Trace
      Trace.WriteLine(_header + iMessage, _source);

      // Escribir en el registro de eventos Debug
      Debug.WriteLine(_header + iMessage, _source);
    }

    #endregion Public Methods
  }
}