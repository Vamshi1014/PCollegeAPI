using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{

    public class Partner
    {
        public User? User { get; set; }
        public Company? Company { get; set; }
        public CompanyUser? CompanyUser { get; set; }
        public IList<CompanyAddress>? CompanyAddresses { get; set; } = new List<CompanyAddress>();
        public IList<CompanyDocuments>? CompanyDocuments { get; set; } = new List<CompanyDocuments>();
        public IList<Questions>? Questions { get; set; } = new List<Questions>();
        public IList<Responses>? Responses { get; set; } = new List<Responses>();
        public IList<CompanyReferences>? CompanyReferences { get; set; } = new List<CompanyReferences>();

        public void SetCompany(Company company)
        {
            Company = company;

            if (company != null)
            {
                if (CompanyUser != null)
                {
                    CompanyUser.CompanyId = company.CompanyID;
                }

                foreach (var address in CompanyAddresses)
                {
                    address.CompanyId = company.CompanyID;
                }
                foreach (var document in CompanyDocuments)
                {
                    document.CompanyId = company.CompanyID;
                }
                foreach (var reference in CompanyReferences)
                {
                    reference.CompanyId = company.CompanyID;
                }
                foreach (var response in Responses)
                {
                    response.CompanyId = (int)company.CompanyID;
                }
            }
        }

  

        public void SetCompanyUser(CompanyUser companyUser)
        {
            CompanyUser = companyUser;

            if (companyUser != null)
            {
                if (Company != null)
                {
                    companyUser.CompanyId = Company.CompanyID;
                }

                //if (User != null)
                //{
                //    companyUser.User = User.UserId;
                //    ;
                //}
            }
        }
    }

    public class CompanyUser
    {
        public int CompanyUserId { get; set; }
        public User User { get; set; }
        public int? CompanyId { get; set; }
        public int? IsPrimaryContact { get; set; }
        public int? BranchId { get; set; }
        public int? IsParent{ get; set; }

        


    }

    public class CompanyAddress
    {
        public int CompanyAddressId { get; set; }
        public int? CompanyId { get; set; }
        public Address? Addresses { get; set; }
    }

    public class CompanyDocuments
    {
        public int CompanyDocumentId { get; set; }
        public int? CompanyId { get; set; }
        public IList<Document>? Documents { get; set; } = new List<Document>();
    }

    public class CompanyReferences
    {
        public int CompanyReferenceId { get; set; }
        public int? CompanyId { get; set; }
        public IList<Reference>? Reference { get; set; }
    }

    public class CompanyDetails
    {
        public CompanyUser? CompanyUser { get; set; }
        public CompanyAddress? CompanyAddress { get; set; }
        public Company? Company { get; set; }
        public CompanyReferences? CompanyReferences { get; set; }        
        public void SetCompanyId(Company company)
        {
            Company = company;

            if (company != null)
            {
                if (CompanyUser != null)
                {
                    CompanyUser.CompanyId = company.CompanyID;
                }

                if (CompanyAddress != null)
                {
                    CompanyAddress.CompanyId = company.CompanyID;
                }

            }
        }
    }

    public class DocumentReponse 
    {
        public int CompanyId { get; set; }
        public IList<CompanyDocuments>? CompanyDocuments { get; set; } = new List<CompanyDocuments>();
        public IList<Responses>? Responses { get; set; } = new List<Responses>();
        public IList<Questions>? Questions { get; set; } = new List<Questions>();
    }
    public class QuestionReponse
    {
        public int CompanyId { get; set; }

        public IList<Responses>? Responses { get; set; } = new List<Responses>();
        public IList<Document>? Documents { get; set; } = new List<Document>();
    }

}
