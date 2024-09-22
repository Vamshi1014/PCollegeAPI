using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IProgramRepository
    {
        Task<(int TotalCount, List<UniversityProgram> Programs)> GetUniversityProgramsAsync(string? universityName = null, string? programName = null,
            string? academicLevel = null, int countryId = 0, int pageNumber = 1, int pageSize = 1000);
        Task<List<ProgramMaster>> GetPrograms(string? programName = null);
        Task<List<AcademicLevel>> GetAcademicLevels(string? levelName = null);
    }
}
