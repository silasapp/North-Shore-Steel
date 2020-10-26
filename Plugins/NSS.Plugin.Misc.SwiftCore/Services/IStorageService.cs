using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftCore.Services
{
    public interface IStorageService
    {
        string UploadBlob(string accountName, string accountKey, string containerName, string blobName, byte[] content);
        void DeleteBlob(string accountName, string accountKey, string containerName, string blobName);
    }
}
