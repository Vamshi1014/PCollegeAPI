using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IPartnerRepository
    {
        Task<CompanyDetails> ExecuteUpsertCompanyDetailsAsync(CompanyDetails companyDetails, SqlTransaction transaction);
        Task<DocumentReponse> UpsertResponsesAsync(DocumentReponse documentReponse, SqlTransaction transaction);
         Task<DocumentReponse> UpsertResponseDetails(DocumentReponse questionReponse);
        Task<DocumentReponse> UpsertAgentDetails(DocumentReponse documentReponse);
        Task<Agent> UpsertCompanyDetailsAsync(Agent objagent);
        Task<CompanyDetails> UpsertPrimaryUserAgent(CompanyDetails companyDetails);
        Task<string> UpsertCompanyLogo(byte[] logocontent, string filename, int companyId);
        Task<string> UpdateCompanyLogopath(int companyId, string companyLogoPath);
        Task<string> GetCompanyLogoAsync(int companyId);
    //    Task<(List<CompanyBranches>, string)> GetCompanyBranchDetailsByUserAsync(int userId);
      //  Task<Agent> GetCompanyDetailsAsyncByCompanyId(int companyId);
        Task<Agent_Information> UpsertAgentInformation(Agent_Information agent, SqlTransaction transaction);
    }
}
