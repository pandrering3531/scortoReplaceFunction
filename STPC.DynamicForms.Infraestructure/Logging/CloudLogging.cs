using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.Infraestructure.Logging
{
  public class CloudLogging : ILogging
  {
    #region Atributos y Constantes

    private TableServiceContext serviceContext;

    #endregion

    #region Propiedades

    /// <summary>
    /// Gets the LOG SOURCE.
    /// </summary>
    /// <value>
    /// The LOG SOURCE.
    /// </value>
    static string CLOUD_STORAGE_ACCOUNT_NAME
    {
      get { return ConfigurationManager.AppSettings["StorageConnectionString"]; }
    }

    /// <summary>
    /// Gets the LOG NAME.
    /// </summary>
    /// <value>
    /// The LOG NAME.
    /// </value>
    static string LOG_NAME
    {
      get { return ConfigurationManager.AppSettings["LogName"]; }
    }

    /// <summary>
    /// Gets the LOG HEADER.
    /// </summary>
    /// <value>
    /// The LOG HEADER.
    /// </value>
    static string LOG_HEADER
    {
      get { return "[ " + DateTime.Now + "] : "; }
    }

    /// <summary>
    /// Gets the SERVIC e_ CONTEX t_ CLASS.
    /// </summary>
    /// <value>
    /// The SERVIC e_ CONTEX t_ CLASS.
    /// </value>
    static string SERVICE_CONTEXT_CLASS
    {
      get { return ConfigurationManager.AppSettings["ServiceContextClass"]; }
    }


    // ---------------------------------------

    public string Name { get; set; }
    public string Description { get; set; }


    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudLogging" /> class.
    /// </summary>
    public CloudLogging()
    {
      CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CLOUD_STORAGE_ACCOUNT_NAME);
      CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
      tableClient.CreateTableIfNotExist(LOG_NAME);
      serviceContext = tableClient.GetDataServiceContext();
    }

    #endregion Constructor

    #region Implementacion interface ILogging

    /// <summary>
    /// Writes the log.
    /// </summary>
    /// <param name="iMessage">The i message.</param>
    public void WriteLog(string iMessage)
    {
      var newEntry = new LogEntry(iMessage);
      serviceContext.AddObject(SERVICE_CONTEXT_CLASS, newEntry);
      serviceContext.SaveChangesWithRetries();
    }

    #endregion Implementacion interface ILogging
  }

  #region clase auxiliar

  public class LogEntry : TableServiceEntity
  {
    #region Atributos y Constantes

    #endregion

    #region Propiedades

    public string Name { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// Gets the LOG NAME.
    /// </summary>
    /// <value>
    /// The LOG NAME.
    /// </value>
    static string LOG_NAME
    {
      get { return ConfigurationManager.AppSettings["LogName"]; }
    }

    /// <summary>
    /// Gets the LOG HEADER.
    /// </summary>
    /// <value>
    /// The LOG HEADER.
    /// </value>
    static string LOG_HEADER
    {
      get { return "[ " + DateTime.Now + "] : "; }
    }


    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry" /> class.
    /// </summary>
    public LogEntry(string iMessage)
    {
      //PartitionKey = string.Format("{0}_{1}", "Warning", DateTime.UtcNow.ToString("yyyyMMdd"));
      //RowKey = string.Format("{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());

      PartitionKey = LOG_NAME;
      RowKey = string.Format("{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());

      this.Name = LOG_NAME;
      this.Description = iMessage;
    }

    #endregion Constructor

    #region Metodos Privados
    #endregion

    #region Metodos Publicos

    #endregion
  }


  #endregion clase auxiliar
}
