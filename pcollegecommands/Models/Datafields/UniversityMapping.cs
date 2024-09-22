using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{
    public class UniversityProgram
    {
        public int UniversityProgramId { get; set; }
        public UniversityMaster? UniversityMaster { get; set; }
        public ProgramMaster? ProgramMaster { get; set; }
        public AcademicLevel? AcademicLevel { get; set; }
        public Country? Country { get; set; }
        public List<UniversityExamRequirement>? ListUniversityExamRequirement { get; set; }
    }
}
