using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Enum;

namespace Flyurdreamcommands.Models.Datafields
{
    public class Document
    {
        public int DocumentId { get; set; }
        public string? DocumentName { get; set; }
        public string? FilePath { get; set; }
        public string? ContainerName { get; set; }
        public int UploadedBy { get; set; }        
        public byte[]? Content { get; set; }
        public DateTime UploadedAt { get; set; }
        public DocumentType DocumentType { get; set; }
        public bool IsUpload { get; set; }
    }
    public class DocumentType
    {
        public DocumentTypeId Id { get; set; }
        public string? Name { get; set; }
        public DocumentFor For { get; set; }
    }

}
