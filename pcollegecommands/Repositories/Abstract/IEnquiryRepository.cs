using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IEnquiryRepository
    {
       //   List<Enquiry> GetEnquiryiesByUniqueId(string? uniqueId);
        Task<(string statusMessage, List<Enquiry> enquiries)> GetEnquiries(int userId, string? companyId, string? branchId, int? countryIntrested
            , string? passportnumber, string? uniqueId);
        Task<Enquiry> InsertEnquiryAsync(int userid, Enquiry objenquiry, int? companyId, int? branchId);
        Task<(IEnumerable<Enquiry> enquiries, string status)> GetEnquiriesCompanyBranchByUserAsync(int userId, string companyId = null, string branchId = null);
    }
}
