using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Enum;

namespace Flyurdreamcommands.Helpers
{
    public static class EnumExtensions
    {
        public static string DocumentForToEnumString(this DocumentFor value)
        {
            return value switch
            {
                DocumentFor.Business => "Business",
                DocumentFor.Student => "Student",
                _ => throw new ArgumentException($"Unsupported enum value: {value}"),
            };
        }
        public static string DocumentTypeToEnumString(this DocumentTypeId value)
        {
            return value switch
            {
                DocumentTypeId.BusinessCertificate => "BusinessCertificate",
                DocumentTypeId.CompanyProfile => "CompanyProfile",
                DocumentTypeId.OtherBusiness => "OtherBusiness",
                DocumentTypeId.SSC => "SSC",
                DocumentTypeId.Passport => "Passport",
                DocumentTypeId.Intermediate => "Intermediate",
                DocumentTypeId.Bachelors => "Bachelors",
                DocumentTypeId.Masters => "Masters",
                DocumentTypeId.OtherCandidate => "OtherCandidate",
                _ => throw new ArgumentException($"Unsupported enum value: {value}"),
            };
        }
    }
}
