using Flyurdreamcommands.Models.Enum;

namespace Flyurdreamcommands.Models.Datafields
{
    public class Questions
    {       
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public bool IsActive { get; set; }
        public bool IsMandatory { get; set; }
        public string? ValidationMessage { get; set; }
        public string? WarningMessage { get; set; }
        public int? CharLength { get; set; }
        public InputType InputType { get; set; } // enum
        public DocumentType DocumentType { get; set; } // enum
        public string? QuestionText { get; internal set; }
        public string? ClassId { get; internal set; }
        public string? ClassName { get; internal set; }
        public bool? FieldEnable { get; internal set; }
        public int? SortOrder { get; set; }
        public int? Tab { get; set; }



    }
    
    public class Responses
    {
        public int ResponsesId { get; set; }
        public int QuestionId { get; set; }
        public int CompanyId { get; set; }
        public string? ResponseText { get; set; }
        public int DocumentId { get; set; } // if its first time insert the documentid from documents model.

    }
}


 		