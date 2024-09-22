using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{
    public class UniversityMaster
    {
        public int UniversityId { get; set; }
        public string Univ_Name { get; set; }
        public string Univ_Description { get; set; }
        public string Univ_Logo { get; set; }
        public string Univ_Phone { get; set; }
        public string Univ_Email { get; set; }
        public string Univ_Website { get; set; }
        public string Assigned_Users { get; set; }
        public bool Is_Active { get; set; }
    }
    public class CountrySpecificUniversity
    {
        public int CountrySpecificUniversityId { get; set; }
        public string? CountrySpecificUniversityName { get; set; }
        public bool Is_Active { get; set; }

        public Country? Country { get; set; }
    }
    public class HigherSecondaryBoard
    {
        public int HSCID { get; set; }
        public string? HSCName { get; set; }
        public bool Is_Active { get; set; }

        public Country? Country { get; set; }
    }
    public class UniversityEntryRequirements
    {
        public int UniversityEntryRequirementsId { get; set; }
        public int Typeid { get; set; }
        public int Universityid { get; set; }
        public int EntryCountryid { get; set; }
        public int UniversityCountryid { get; set; }
        public int HSCId { get; set; }
        public decimal Percentage { get; set; }
        public int MOIId { get; set; }
        public int Education_Gap { get; set; }
        public int Created_By { get; set; }  // Default value 0
        public DateTime Created_On { get; set; } = DateTime.Now;  // Default to current date/time
        public int Updated_By { get; set; }   // Default value 0
        public DateTime Updated_On { get; set; }
        public bool IsActive { get; set; } = false;  // Default value false
        public string Response { get; set; }
    }

}
