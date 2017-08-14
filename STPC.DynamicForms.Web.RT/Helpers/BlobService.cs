using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace STPC.DynamicForms.Web.RT.Helpers
{
  public class BlobService
  {
    private CloudStorageAccount account;
    private readonly CloudBlobClient _blobClient;


    //private const string ContainerName = "mypics";
    private string ContainerName = System.Configuration.ConfigurationManager.AppSettings["CloudContainerName"];
    private string CloudAccount = System.Configuration.ConfigurationManager.AppSettings["CloudAccount"];

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobService"/> class.
    /// </summary>
    public BlobService()
    {
      
      /*******************************************/

      switch (CloudAccount)
      {
        case "Development":
          #region DEVELOPMENT ACCOUNT

          account = CloudStorageAccount.DevelopmentStorageAccount;
          _blobClient = new CloudBlobClient(account.BlobEndpoint, account.Credentials);

          #endregion DEVELOPMENT ACCOUNT
          break;

        case "uat":
          #region UAT ACCOUNT

          CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
          configSetter(ConfigurationManager.ConnectionStrings[configName].ConnectionString));

          account = CloudStorageAccount.FromConfigurationSetting(ContainerName);
          _blobClient = new CloudBlobClient(account.BlobEndpoint, account.Credentials);

          #endregion UAT ACCOUNT
          break;

        default:
          #region DEVELOPMENT ACCOUNT

          account = CloudStorageAccount.DevelopmentStorageAccount;
          _blobClient = new CloudBlobClient(account.BlobEndpoint, account.Credentials);

          #endregion DEVELOPMENT ACCOUNT
          break;
      }

      /*******************************************/

    }

    /// <summary>
    /// Gets the container.
    /// </summary>
    /// <param name="containerName">Name of the container.</param>
    /// <returns></returns>
    private CloudBlobContainer GetContainer(string containerName)
    {
      var container = _blobClient.GetContainerReference(containerName);

      if (container.CreateIfNotExist())
      {
        var containerPermissions = container.GetPermissions();
        containerPermissions.PublicAccess = BlobContainerPublicAccessType.Container;
        container.SetPermissions(containerPermissions);
      }

      return container;
    }

    // -------------------------------------------------------------------------------

    /// <summary>
    /// Gets the cloud BLOB container.
    /// </summary>
    /// <returns></returns>
    public CloudBlobContainer GetCloudBlobContainer()
    {


      // Retrieve a reference to a container 
      CloudBlobContainer blobContainer = _blobClient.GetContainerReference(ContainerName);

      // Create the container if it doesn't already exist
      if (blobContainer.CreateIfNotExist())
      {

        blobContainer.SetPermissions(
           new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob }
        );
      }

      return blobContainer;
    }

    // -------------------------------------------------------------------------------

    /// <summary>
    /// Adds the BLOB.
    /// </summary>
    /// <param name="fileBytes">The file bytes.</param>
    /// <param name="fileName">Name of the file.</param>
    //public void AddBlob(byte[] fileBytes, string fileName)
    //{
    //  var blob = GetContainer(ContainerName).GetBlobReference(fileName);
    //  blob.UploadByteArray(fileBytes);
    //}

    /// <summary>
    /// Adds the BLOB.
    /// </summary>
    /// <param name="fileBytes">The file bytes.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public string AddBlob(byte[] fileBytes, string fileName, string mimeType)
    {
      //reemplazar el nombre del arhivo por un GUID para garantizar que sea unico
      var arrFileName = fileName.Split('.');
      string combinedFileName = Guid.NewGuid().ToString();
      if (!string.IsNullOrEmpty(arrFileName[1])) combinedFileName = combinedFileName + "." + arrFileName[1];

      //var blob = GetContainer(ContainerName).GetBlobReference(fileName);

      var blob = GetContainer(ContainerName).GetBlobReference(combinedFileName);
		blob.Properties.ContentType = mimeType;
      blob.UploadByteArray(fileBytes);
		 
		//return blob.Uri.LocalPath;
		return blob.Name;
    }

    /// <summary>
    /// Gets the BLOB.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public string GetBlob(string fileName)
    {
      var blob = GetContainer(ContainerName).GetBlobReference(fileName);
      return blob.Uri.ToString();

      //var blob = GetContainer("uniandes").GetBlobReference("Reporte.xls");

      ////https://lisim.blob.core.windows.net/uniandes/Reporte.xls

      //return blob.Uri.ToString();
    }

    /// <summary>
    /// Gets the blobs.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IListBlobItem> GetBlobs()
    {
      var container = GetContainer(ContainerName);
      return container.ListBlobs();
    }

    /// <summary>
    /// Deletes the BLOB.
    /// </summary>
    /// <param name="uri">The URI.</param>
    public void DeleteBlob(string uri)
    {
      var blob = GetContainer(ContainerName).GetBlobReference(uri);
      blob.DeleteIfExists();
    }

  }

}