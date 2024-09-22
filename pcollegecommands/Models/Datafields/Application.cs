using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{
   public class Application
    {
        public int ApplicationId { get; set; }
        public string Unique_ID { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public DateTime DOB { get; set; } // Date of Birth
        public int University { get; set; }

        public int Course { get; set; }
        public int AcademicLevel { get; set; }
        public string Intake { get; set; }
        public int Assigned_Users { get; set; }
        public int Status { get; set; }
        public int Created_By { get; set; }
        public DateTime Created_On { get; set; }
        public string Comments { get; set; }
        public string EmailID { get; set; }
        public string Passport_No { get; set; }
        public DateTime? Updated_On { get; set; } // Nullable DateTime for optional updates
        public int? Updated_By { get; set; } // Nullable int for optional updates
        public string StudentUniqueId { get; set; } // Note: Changed to PascalCase for consistency
    }
}
