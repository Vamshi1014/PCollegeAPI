using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;
using Microsoft.Data.SqlClient;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IReferenceRepository
    {
        Task<CompanyDetails> UpsertReferencesAsync(CompanyDetails companyDetails, SqlTransaction transaction);
        Task<CompanyDetails> UpsertCompanyReferencesAsync(CompanyDetails companyDetails, SqlTransaction transaction);
        Task<List<EmergencyContact>> UpsertEmergencyContactsAsync(List<EmergencyContact> emergencyContacts);
       
    }
}
