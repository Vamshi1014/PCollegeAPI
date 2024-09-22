using Flyurdreamcommands.Models.Datafields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcollegecommands.Models.Datafields
{
    public class TargetCountries
    {
        public int Id { get; set; }

        public Country? Country { get; set; }

        public User? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }
        public User? UploadedBy { get; set; }

        public DateTime UploadedOn { get; set; }

        public int CompanyId { get; set; }

        public int BranchId { get; set; }
    }
    //
    public class EstimateStudentsperintake
    {
        public int Id { get; set; }

        public Intake? IntakeId { get; set; }

        public int EstimateOfStudents { get; set; }
        public User? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }
        public User? UploadedBy { get; set; }

        public DateTime UploadedOn { get; set; }

        public int CompanyId { get; set; }

        public int BranchId { get; set; }


    }
}