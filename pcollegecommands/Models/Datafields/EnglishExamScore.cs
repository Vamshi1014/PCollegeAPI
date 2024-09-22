using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{
    public class EnglishExamScore
    {
        public int UniversityId { get; set; }
        public int AcademicCategoryID { get; set; }

        public string? AcademicCategoryName { get; set; }
        public string? ExamName { get; set; }
        public int ExamId { get; set; }
        public string? ListeningScore { get; set; }
        public string? ReadingScore { get; set; }
        public string? WritingScore { get; set; }
        public string? SpeakingScore { get; set; }
        public string? OverAllScore { get; set; }
        public string? MinimumScore { get; set; }
    }

    public class EnglishExam
    {
        public int ExamRequirementId { get; set; }
        public int? ExamTypeId { get; set; }
        public int StudentId { get; set; }
        public string? ListeningScore { get; set; }
        public string? SpeakingScore { get; set; }
        public string? ReadingScore { get; set; }
        public string? WritingScore { get; set; }
        public string? VerbalReasoningScore { get; set; }
        public string? QuantitativeReasoningScore { get; set; }
        public string? AnalyticalWritingScore { get; set; }
        public string? MinimumScore { get; set; }
        public string? OverallScore { get; set; }
        public string? LiteracyScore { get; set; }
        public string? ConversationScore { get; set; }
        public string? ComprehensionScore { get; set; }
        public string? ProductionScore { get; set; }
        public DateTime Createdon { get; set; }
        public string? Response { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
    public class EntryRequirement
    {
        public int EntryRequirementID { get; set; }
        public int UniversityID { get; set; }
        public Country EntryCountry { get; set; }
        public AcademicCategory AcademicCategory { get; set; }
        public string AcademicEntryRequirements { get; set; }
        public string MathematicsEntryRequirements { get; set; }
        public string EnglishLanguageRequirements { get; set; }
        public string EnglishLanguageWaiver { get; set; }

        public Country UniversityCountry { get; set; }

    }
    public class UniversityExamRequirement
    {
        public int UniversityExamRequirementId { get; set; }
        public int UniversityId { get; set; }
        public int AcademicCategoryId { get; set; }
        public EnglishExam EnglishExamRequirement { get; set; }
        public int IsActive { get; set; }
        public int UniversityCountryId { get; set; }

    }
}
