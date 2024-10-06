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

        public void SetCompany(Company company)
        {
            Company = company;

            if (company != null)
            {
                if (CompanyUser != null)
                {
                    CompanyUser.CompanyId = (int)company.CompanyID;
                }

                foreach (var address in CompanyAddresses)
                {
                    address.CompanyId = company.CompanyID;
                
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
                    companyUser.CompanyId = (int)Company.CompanyID;
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
        public int CompanyId { get; set; }
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
        public int CompanyId { get; set; }
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

        public string? PortalWebAddress { get; set; }

        public string? PortalWebAddress2 { get; set; }
        public string? PortalocalAddress { get; set; }
        public string? TradeName { get; set; }
        public string? Mobile { get; set; }
        public string? email { get; set; }
        public User? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public User? UploadedBy { get; set; }
        public DateTime UploadedOn { get; set; }
        public void SetCompanyId(Company company)
        {
            Company = company;

            if (company != null)
            {
                if (CompanyUser != null)
                {
                    CompanyUser.CompanyId = (int)company.CompanyID;
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
        //public IList<Responses>? Responses { get; set; } = new List<Responses>();
        //public IList<Questions>? Questions { get; set; } = new List<Questions>();
    }


}
