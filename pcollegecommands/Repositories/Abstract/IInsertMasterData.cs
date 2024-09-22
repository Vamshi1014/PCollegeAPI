using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IInsertMasterData
    {
        Task<int> GetCountryId(string countryName);
        Task<int> GetOrInsertUniversityId(UniversityMaster objUniversityMaster);
        Task<int> GetOrInsertAcademicLevelId(AcademicLevel objAcademicLevel);
        Task<int> GetOrInsertProgram(ProgramMaster objProgramMaster);
        Task<int> GetInsertUniversityMapping(int countryId, int universityId, int academicLevelId, int programId, int programDetailsId);
        Task<string> BulkInsert(DataTable dataTable, int batchSize);
    }
}
