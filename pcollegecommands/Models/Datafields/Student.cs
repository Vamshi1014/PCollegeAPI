using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Flyurdreamcommands.Models.Datafields
{
    public class Student
    {
        public int StudentId { get; set; }
        public string? Student_UniqueId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? MaritalStatus { get; set; }
        public string? Mobile { get; set; }
        public string? Dial { get; set; }
        public string? Email { get; set; }
        public int IsActive { get; set; }
        public int CreatedBy { get; set; }
        public int UpdateBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdateOn { get; set; }
        public string? Response { get; set; }
        public StudentAddress? Address { get; set; }
    }

    public class StudentAddress
    {
        public int StudentAddressId { get; set; }
        public int? StudentId { get; set; }
        public Address? Address { get; set; }
        public bool IsUpdate { get; set; }
    }

    public class StudentDocument
    {
        public int StudentDocumentId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }

        public int DocumentId { get; set; }

        public int StudentId { get; set; }

        public string? UniqueId { get; set; }
        public string Response { get; set; }

    }
    public class DocumentUploadModel
    {
        public IList<Document> Documents { get; set; }
        public IList<StudentDocument> ListStudentDocument { get; set; }
    }

    public class StudentDetails
    {
        public Student Student { get; set; }

        public IList<EducationData> EducationData { get; set; }
        public IList<WorkExperience> WorkExperience { get; set; }
        public IList<VisaRefusal> VisaRefusal { get; set; }
        public IList<EmergencyContact> EmergencyContact { get; set; }
        public IList<EnglishExam> EnglishExamAchieved { get; set; }

        public DocumentUploadModel DocumentUploadModel { get; set; }  //List


    }
}


