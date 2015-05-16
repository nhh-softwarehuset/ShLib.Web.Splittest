using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SHLib.Web.SplitTest.FileProvider
{
    public class AzureBlobProvider : IPathProvider
    {
        private CloudBlobContainer container { set; get; }
        private string ContainerName { get; set; }
        private string ConnectionString { get; set; }
        private string FileName { get; set; }

        public string MapPath(string path)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileName"></param>
        /// <param name="connectionString">If not provided StorageConnectionString is used</param>
        public AzureBlobProvider(string containerName, string fileName, string connectionString = null)
        {
            ContainerName = containerName;
            FileName = fileName;
            ConnectionString = connectionString;
            container = GetContainer(ContainerName, ConnectionString);
        }
        /// <summary>
        /// Get Container for provider
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private CloudBlobContainer GetContainer(string containerName, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "StorageConnectionString";
            }
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                //CloudConfigurationManager.GetSetting());

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference("mycontainer");
            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();
            return container;
        }

        public void SaveContent(string xml)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(FileName);
            
            blob.UploadText(xml);
        }

        public string LoadContent()
        {

            var blob = container.GetBlockBlobReference(FileName);
            var xml = blob.DownloadText(); // transaction 1
     
            return xml;
        }

    }
}
