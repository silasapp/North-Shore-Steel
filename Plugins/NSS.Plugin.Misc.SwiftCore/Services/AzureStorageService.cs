using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public class AzureStorageService : IStorageService
    {
        public string UploadBlob(string accountName, string accountKey, string containerName, string blobName, byte[] content)
        {
            var credential = new StorageSharedKeyCredential(accountName, accountKey);

            var blobContainerUri = new Uri($"https://{accountName}.blob.core.windows.net/{containerName}"); 

            //BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            BlobContainerClient container = new BlobContainerClient(blobContainerUri, credential);
            container.CreateIfNotExists();

            // Get a reference to a blob named "sample-file" in a container named "sample-container"
            BlobClient blob = container.GetBlobClient(blobName);

            // Upload stream file
            using (Stream stream = new MemoryStream(content))
            {
                blob.Upload(stream, overwrite: true);
            }

            var blobSasUri = GetBlobSasUri(container, blobName, credential);

            return blobSasUri;
        }

        public void DeleteBlob(string accountName, string accountKey, string containerName, string blobName)
        {
            var credential = new StorageSharedKeyCredential(accountName, accountKey);

            var blobContainerUri = new Uri($"https://{accountName}.blob.core.windows.net/{containerName}");

            //BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            BlobContainerClient container = new BlobContainerClient(blobContainerUri, credential);
            container.CreateIfNotExists();

            // Get a reference to a blob named "sample-file" in a container named "sample-container"
            BlobClient blob = container.GetBlobClient(blobName);
            blob.DeleteIfExists(Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots);
        }

        private string GetBlobSasUri(BlobContainerClient container,
            string blobName, StorageSharedKeyCredential key, string storedPolicyName = null)
        {
            // Create a SAS token that's valid for one hour.
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = container.Name,
                BlobName = blobName,
                Resource = "b",
            };

            if (storedPolicyName == null)
            {
                sasBuilder.StartsOn = DateTimeOffset.UtcNow;
                sasBuilder.ExpiresOn = DateTimeOffset.MaxValue;
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
            }
            else
            {
                sasBuilder.Identifier = storedPolicyName;
            }

            // Use the key to get the SAS token.
            string sasToken = sasBuilder.ToSasQueryParameters(key).ToString();

            Console.WriteLine("SAS for blob is: {0}", sasToken);
            Console.WriteLine();

            return $"{container.GetBlockBlobClient(blobName).Uri}?{sasToken}";
        }


        private static string GetContainerSasUri(BlobContainerClient container,
            StorageSharedKeyCredential sharedKeyCredential, string storedPolicyName = null)
        {
            // Create a SAS token that's valid for one hour.
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = container.Name,
                Resource = "c",
            };

            if (storedPolicyName == null)
            {
                sasBuilder.StartsOn = DateTimeOffset.UtcNow;
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
            }
            else
            {
                sasBuilder.Identifier = storedPolicyName;
            }

            // Use the key to get the SAS token.
            string sasToken = sasBuilder.ToSasQueryParameters(sharedKeyCredential).ToString();

            Console.WriteLine("SAS token for blob container is: {0}", sasToken);
            Console.WriteLine();

            return $"{container.Uri}?{sasToken}";
        }
    }
}
