using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Service.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Service.Concrete
{
    public class BlobHandler : IBlobHandler
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobHandler()
        {
            // Retrieve connection string from web.config
            string connectionString = ConfigurationData.BlobConnectionString;
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> GetBlobAsString(string blobUri)
        {
            string base64String = string.Empty;
            //     string containerName = "your-container-name";

            // Create a blob service client
            // BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Get a reference to a container
            // BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(blobUri));
            BlobClient blobClient = containerClient.GetBlobClient(GetBlobName(blobUri));
            Response<BlobDownloadInfo> response = await blobClient.DownloadAsync();
            // Read the stream into a byte array
            using (MemoryStream ms = new MemoryStream())
            {
                await response.Value.Content.CopyToAsync(ms);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to base64 string
                base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
           // return await new StreamReader(response.Value.Content).ReadToEndAsync();
        }

        public string GetBlobNameforUri(string blobUri)
        {
            return GetBlobName(blobUri);
        }

        public async Task<Stream> GetBlobAsStream(string blobUri, string containerName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            // BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(blobUri));
            BlobClient blobClient = containerClient.GetBlobClient(GetBlobName(blobUri));
            Response<BlobDownloadInfo> response = await blobClient.DownloadAsync();
            return response.Value.Content;
           
        }

        public async Task<string> MoveBlob(string blobUri, string fromFolder, string toFolder)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(blobUri));
            BlobClient sourceBlob = containerClient.GetBlobClient(GetBlobName(blobUri));
            string destinationBlobName = blobUri.Replace(fromFolder, toFolder);
            BlobClient destinationBlob = containerClient.GetBlobClient(GetBlobName(destinationBlobName));
            await destinationBlob.StartCopyFromUriAsync(sourceBlob.Uri);
            await sourceBlob.DeleteIfExistsAsync();
            return destinationBlobName;
        }

        public async Task<string> CreateBlobFromString(string blobUri, string content)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(blobUri));
            BlobClient blobClient = containerClient.GetBlobClient(GetBlobName(blobUri));
            await blobClient.UploadAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)), true);
            return blobClient.Uri.ToString(); // Return the URI of the uploaded blob
        }

        public async Task<string> CreateBlobFromStream(string blobUri, Stream content)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(blobUri));
            BlobClient blobClient = containerClient.GetBlobClient(GetBlobName(blobUri));
            await blobClient.UploadAsync(content, true);
            return blobClient.Uri.ToString(); // Return the URI of the uploaded blob
        }
        public async Task<string> CreateBlobFromBytes(string containerName, string blobName,byte[] content)        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
               await containerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            using (MemoryStream stream = new MemoryStream(content))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.BlobContainerName + "/"+ blobClient.Name; // Return the URI of the uploaded blob
        }

        public async Task<List<byte[]>> GetAllContainerRecords(string containerName)
        {
            // List to store byte arrays of blob content
            List<byte[]> allBytes = new List<byte[]>();

            // Get a reference to a container
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // List all blobs in the container
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                // Get a reference to the blob
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);

                // Download the blob's content as a byte array
                BlobDownloadInfo download = await blobClient.DownloadAsync();
                byte[] bytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    await download.Content.CopyToAsync(ms);
                    bytes = ms.ToArray();
                }

                // Add the byte array to the list
                allBytes.Add(bytes);
            }

            // Return the list of byte arrays
            return allBytes;
        }

        private string GetContainerName(string blobUri)
        {
            Uri uri = new Uri(blobUri);
            string[] parts = uri.AbsolutePath.Trim('/').Split('/');
            return parts.Length > 0 ? parts[0] : null;
        }

        private string GetBlobName(string blobUri)
        {
            Uri uri = new Uri(blobUri);
            string[] parts = uri.AbsolutePath.Trim('/').Split('/');
            return string.Join("/", parts, 1, parts.Length - 1);
        }
    }

}
