using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Service.Abstract
{
    public interface IBlobHandler
    {
        Task<string> GetBlobAsString(string blobUri);
        string GetBlobNameforUri(string blobUri);
        Task<Stream> GetBlobAsStream(string blobUri, string containerName);
        Task<string> MoveBlob(string blobUri, string fromFolder, String toFolder);
        Task<string> CreateBlobFromString(string blobUri, string content);
        Task<string> CreateBlobFromStream(string blobUri, Stream content);
        Task<string> CreateBlobFromBytes(string containerName, string blobName, byte[] content);

    }
}
