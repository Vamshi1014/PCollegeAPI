using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Enum;

namespace Flyurdreamcommands.Models.Datafields
{
  
        public class Address
        {
            public int AddressId { get; set; }
            public string? HouseNumber { get; set; }
            public string? BuildingName { get; set; }
            public string? AddressLine1 { get; set; }
            public string? AddressLine2 { get; set; }
            public int CityID { get; set; }
            public int DistrictID { get; set; }
            public int StateID { get; set; }
            public int CountryID { get; set; }
            public string? ZipCode { get; set; }
            public bool IsActive { get; set; }
            public AddressType AddressType { get; set; }
           public string? Result { get; set; }

        
    }

    
}
