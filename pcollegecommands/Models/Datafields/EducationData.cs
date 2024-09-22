using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{
    public class EducationData
    {
        public int EducationId { get; set; }
        public string? EducationLevel { get; set; }
        public string? SchoolName { get; set; }
        public decimal? Percentage { get; set; }
        public string? Grade { get; set; }
        public decimal? GPA { get; set; }
        public int? EnglishMarks { get; set; }
        public int? MathsMarks { get; set; }
        public int? PhysicsMarks { get; set; }
        public int? ChemistryMarks { get; set; }
        public int StudentId { get; set; }
        public string? Response { get; set; }
    }

}
