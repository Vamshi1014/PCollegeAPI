using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Enum
{
    public enum DocumentTypeId
    {

        ICEFAccreditation = 6,
        LegalStatus = 7,
        OtherBusinessDocument = 8,
        SSC = 9,
        Passport = 10,
        Intermediate = 11,
        Masters = 12,
        OtherCandidate = 13,
        Resume = 14,
        OfferLetter = 15,
        SOP = 16,
        LanguageExam = 17,
        LOR = 18,
        Bachelors = 31

    }

    public enum DocumentFor
    {
        Business,
        Student,
        Pcollege
    }

}