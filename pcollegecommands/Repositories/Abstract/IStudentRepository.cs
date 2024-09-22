using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;
using Microsoft.Data.SqlClient;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IStudentRepository
    {
        Task<Student> UpsertStudentAsync(Student student, int? companyId, int? branchId, SqlTransaction transaction);
        Task<Student> UpdateStudentndAddress(Student studentm, int? companyId, int? branchId);
        Task<Student> GetStudentByIdAsync(int studentId);
        Task<List<EducationData>> UpsertEducationAsync(List<EducationData> listEducationData);
        Task<List<VisaRefusal>> UpsertVisaRefusalsAsync(List<VisaRefusal> visaRefusals);
        Task<List<WorkExperience>> UpsertWorkExperienceAsync(List<WorkExperience> listWorkExperienceData);
        Task<List<EnglishExam>> UpsertEnglishExamscoreAchieved(List<EnglishExam> listexamAchievedData);
        Task<StudentDetails> GetStudentDetailsAsyncByStudentId(String UniqueId);
        Task<string> UpsertApplicationDataAsync(Application applicationData);
        Task<List<Application>> GetApplicationDataByUniqueIDAsync(
    string? uniqueId, string? passportNo, string? studentUniqueId, string? assignedUsers,
    int? createdBy, string? emailId, int? status);
    }
}
