using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{
    public class Branch
    {
        public int? BranchId { get; set; }

        public string? BranchName { get; set; }

        public bool IsActive { get; set; }

        public string? Result { get; set; }
    }

    public class CompanyBranches
    {
        public Company Company { get; set; }
        public List<Branch> Branches { get; set; }
    }
}

