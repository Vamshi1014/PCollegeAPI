using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{

    public class AcademicLevel
    {
        public int AcademicLevelId { get; set; }
        public string? LevelName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public AcademicCategory? AcademicCategory { get; set; }
    }

    public class AcademicCategory
    {
        public int AcademicCategoryID { get; set; }
        public string? AcademicCategoryName { get; set; }
        public string? Description { get; set; }
    }

}
