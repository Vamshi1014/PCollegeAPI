using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spire.Xls;

namespace Flyurdreamcommands.Models.Datafields
{
    public class EmergencyContact
    {
        public int EmergencyId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public int StudentId { get; set; }
        public string Response { get; set; }
    }
  
     public class VisaRefusal
    {
        public int VisarefusalId { get; set; }
        public string RefusalCountry { get; set; }
        public DateTime RefusalDate { get; set; }
        public string Description { get; set; }
        public int StudentId { get; set; }
        public string Response { get; set; }
    }

}
