using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{
   
        public class WorkExperience
        {
            public int WorkExperienceId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string Designation { get; set; }
            public string Responsibilities { get; set; }
            public string ProjectName { get; set; }
            public DateTime CreatedOn { get; set; }
            public DateTime? UpdatedOn { get; set; }
            public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public bool Till_Now { get; set; }
        public string CompanyName { get; set; }
        public int StudentId { get; set; }
        public string Response { get; set; }
        }
    }


