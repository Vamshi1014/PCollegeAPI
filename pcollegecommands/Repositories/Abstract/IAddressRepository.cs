using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;
using Microsoft.Data.SqlClient;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IAddressRepository
    {
        Task<Address> UpsertAddressAsync(Address companyAddress, SqlTransaction transaction);
   //     Task<Address> UpsertAddressAsync(Address address, SqlTransaction transaction);
        Task<CompanyAddress> UpsertCompanyAddressesAsync(CompanyAddress partner, SqlTransaction transaction);
        Task<UserAddress> UpsertUserAddressesAsync(UserAddress userAddress, SqlTransaction transaction);
        Task<StudentAddress> UpsertStudentAddressesAsync(StudentAddress studentAddress, SqlTransaction transaction);

    }
}
