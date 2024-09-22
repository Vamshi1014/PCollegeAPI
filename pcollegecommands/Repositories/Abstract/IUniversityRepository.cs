using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;
using Microsoft.Data.SqlClient;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IUniversityRepository
    {
        Task<List<UniversityMaster>> GetUniversities(string? universityName);
        Task<List<EntryRequirement>> GetEntryRequirementsByCountryAndUniversityAsync(int universityCountry, int universityID, int academicCategoryId);
        Task<EntryRequirement> UpsertEntryRequirementAsync(EntryRequirement entryRequirement, int? userId=0);
        Task<List<CountrySpecificUniversity>> GetCountrySpecificUniversitiesAsync(string? searchUniversityName);
        Task<List<HigherSecondaryBoard>> GetHigherSecondaryBoardsAsync(string? searchHSCName);
        Task<List<UniversityEntryRequirements>> UpsertUniversityEntryRequirementsAsync(List<UniversityEntryRequirements> listUniversityEntryRequirementsData);
        Task<List<UniversityEntryRequirements>> GetUniversityEntryRequirementsAsync(int universityId, int entryCountryId, int universityCountryId, int? typeId);
        Task<List<UniversityExamRequirement>> InsertEnglishExamRequirementsAsync(List<UniversityExamRequirement> universityEnglishExamRequirement, SqlTransaction transaction);
        Task<List<UniversityExamRequirement>> InsertUniversityExamRequirementsAsync(List<UniversityExamRequirement> universityEnglishExamRequirement, SqlTransaction transaction);
        Task<List<UniversityExamRequirement>> InsertUniversityExamRequirements(List<UniversityExamRequirement> universityEnglishExamRequirement);
        Task<List<UniversityExamRequirement>> GetUniversityEnglishExamRequirementsAsync(
    int universityCountryId = 0,
    int academicCategoryId = 0,
    int universityId = 0);
    }
}
