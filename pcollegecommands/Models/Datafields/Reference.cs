using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{
    public class Reference
    {
        public int ReferenceID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Organisation { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }

        public int? Type { get; set; }
        public string? Result { get; set; }
    }
}
