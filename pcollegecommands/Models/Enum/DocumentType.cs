using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Enum
{
    public enum DocumentTypeId
    {
        BusinessCertificate = 6,
        CompanyProfile = 7,
        OtherBusiness = 8,
        SSC = 9,
        Passport = 10,
        Intermediate = 11,
        Bachelors = 31,
        Masters = 12,
        OtherCandidate = 14,
        Resume =15,
        OfferLetter =16,
        SOP = 17,
        LanguageExam = 18,
        LOR = 19

    }

    public enum DocumentFor
    {
        Business,
        Student
    }

}