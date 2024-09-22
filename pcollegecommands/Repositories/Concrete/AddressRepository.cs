using System.Data;
using System.Net;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class AddressRepository : DataRepositoryBase, IAddressRepository
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        DataTables objDataTables = new DataTables();
        public AddressRepository(IConfiguration config, ILogger<AddressRepository> logger) : base(logger, config)
        {
            _config = config;
            _logger = logger;
        }
        public async Task<Address> UpsertAddressAsync(Address address, SqlTransaction transaction)
        {
            // Assuming there's only one address for the partner
            try
            {
                if (address != null)
                {
                    using (SqlCommand command = new SqlCommand("UpsertAddress", transaction.Connection, transaction))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@AddressID", address.AddressId);
                        command.Parameters.AddWithValue("@HouseNumber", address.HouseNumber);
                        command.Parameters.AddWithValue("@BuildingName", address.BuildingName);
                        command.Parameters.AddWithValue("@AddressLine1", address.AddressLine1);
                        command.Parameters.AddWithValue("@AddressLine2", address.AddressLine2);
                        command.Parameters.AddWithValue("@CityID", address.CityID);
                        command.Parameters.AddWithValue("@DistrictID", address.DistrictID);
                        command.Parameters.AddWithValue("@StateID", address.StateID);
                        command.Parameters.AddWithValue("@CountryID", address.CountryID);
                        command.Parameters.AddWithValue("@ZipCode", address.ZipCode);
                        command.Parameters.AddWithValue("@IsActive", address.IsActive);
                        command.Parameters.AddWithValue("@AddressType", address.AddressType);


                        SqlParameter resultAddressIDParam = new SqlParameter("@ResultAddressID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(resultAddressIDParam);

                        SqlParameter successMessageParam = new SqlParameter("@SuccessMessage", SqlDbType.NVarChar, 100)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(successMessageParam);

                        await command.ExecuteNonQueryAsync();

                        // Retrieve the output parameters
                        address.AddressId = (int)resultAddressIDParam.Value;
                        address.Result = successMessageParam.Value.ToString();
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return address;
        }

        public async Task<CompanyAddress> UpsertCompanyAddressesAsync(CompanyAddress companyAddress, SqlTransaction transaction)
        {
            if (companyAddress == null)
            {
                throw new ArgumentException("companyAddress is null");
            }

            using (SqlCommand command = new SqlCommand("UpsertCompanyAddresses", transaction.Connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters to the stored procedure
                command.Parameters.AddWithValue("@CompanyAddressId", companyAddress.CompanyAddressId > 0 ? (object)companyAddress.CompanyAddressId : DBNull.Value);
                command.Parameters.AddWithValue("@CompanyId", (object)companyAddress.CompanyId ?? DBNull.Value);
                command.Parameters.AddWithValue("@AddressId", companyAddress.Addresses.AddressId);

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        // Assuming that the stored procedure returns the updated CompanyAddress details
                        companyAddress.CompanyAddressId = reader.GetInt32(reader.GetOrdinal("CompanyAddressId"));
                        companyAddress.CompanyId = reader.IsDBNull(reader.GetOrdinal("CompanyId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CompanyId"));
                        companyAddress.Addresses.AddressId = reader.GetInt32(reader.GetOrdinal("AddressId"));
                        //    Addresses = new List<Address>
                        //    {
                        //        new Address
                        //        {
                        //            AddressId =reader.GetInt32(reader.GetOrdinal("AddressId"));
                        //}
                    }
                }
                return companyAddress;
            }
        }

        //public async Task<Address> UpsertAddressAsync(Address address, SqlTransaction transaction)
        //{

        //    try
        //    {
        //        if (address != null)
        //        {
        //            using (SqlCommand command = new SqlCommand("UpsertAddress", transaction.Connection, transaction))
        //            {
        //                command.CommandType = CommandType.StoredProcedure;
        //                command.Parameters.AddWithValue("@AddressID", address.AddressId);
        //                command.Parameters.AddWithValue("@HouseNumber", address.HouseNumber);
        //                command.Parameters.AddWithValue("@BuildingName", address.BuildingName);
        //                command.Parameters.AddWithValue("@AddressLine1", address.AddressLine1);
        //                command.Parameters.AddWithValue("@AddressLine2", address.AddressLine2);
        //                command.Parameters.AddWithValue("@CityID", address.CityID);
        //                command.Parameters.AddWithValue("@DistrictID", address.DistrictID);
        //                command.Parameters.AddWithValue("@StateID", address.StateID);
        //                command.Parameters.AddWithValue("@CountryID", address.CountryID);
        //                command.Parameters.AddWithValue("@ZipCode", address.ZipCode);
        //                command.Parameters.AddWithValue("@IsActive", address.IsActive);
        //                command.Parameters.AddWithValue("@AddressType", address.AddressType);

        //                SqlParameter resultAddressIDParam = new SqlParameter("@ResultAddressID", SqlDbType.Int)
        //                {
        //                    Direction = ParameterDirection.Output
        //                };
        //                command.Parameters.Add(resultAddressIDParam);

        //                SqlParameter successMessageParam = new SqlParameter("@SuccessMessage", SqlDbType.NVarChar, 100)
        //                {
        //                    Direction = ParameterDirection.Output
        //                };
        //                command.Parameters.Add(successMessageParam);

        //                await command.ExecuteNonQueryAsync();

        //                // Retrieve the output parameters
        //                address.AddressId = (int)resultAddressIDParam.Value;
        //                address.Result = successMessageParam.Value.ToString();
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    return address;
        //}

        ////public async Task<Company> UpsertCompanyAddressesAsync(Company companyAddress, SqlTransaction transaction)
        ////{
        ////    if (companyAddress.Address == null || companyAddress.Address.Count == 0)
        ////    {
        ////        throw new ArgumentException("Comany Address is null or empty");
        ////    }

        ////    foreach (var address in companyAddress.Address)
        ////    {
        ////        using (SqlCommand command = new SqlCommand("upsertUserAddress", transaction.Connection, transaction))
        ////        {
        ////            command.CommandType = CommandType.StoredProcedure;

        ////            // Add parameters to the stored procedure
        ////            command.Parameters.AddWithValue("@CompanyAddressId", address.CompanyAddressId > 0 ? (object)address.CompanyAddressId : DBNull.Value);
        ////            command.Parameters.AddWithValue("@CompanyId", address.CompanyId);
        ////            command.Parameters.AddWithValue("@AddressId", address.Addresses.AddressId);

        ////            using (SqlDataReader reader = await command.ExecuteReaderAsync())
        ////            {
        ////                if (await reader.ReadAsync())
        ////                {
        ////                    // Assuming that the stored procedure returns the updated UserAddress details
        ////                    address.CompanyAddressId = reader.GetInt32(reader.GetOrdinal("CompanyAddressId"));
        ////                    address.CompanyId = reader.GetInt32(reader.GetOrdinal("CompanyId"));
        ////                    address.Addresses.AddressId = reader.GetInt32(reader.GetOrdinal("AddressId"));
        ////                }
        ////            }
        ////        }
        ////    }

        ////    return companyAddress;
        ////}
        public async Task<UserAddress> UpsertUserAddressesAsync(UserAddress userAddress, SqlTransaction transaction)
        {
            if (userAddress == null)
            {
                throw new ArgumentException("User Address is null or empty");
            }

            using (SqlCommand command = new SqlCommand("upsertUserAddress", transaction.Connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters to the stored procedure
                command.Parameters.AddWithValue("@UserAddressId", userAddress.UserAddressId );
                command.Parameters.AddWithValue("@UserId", userAddress.UserId);
                command.Parameters.AddWithValue("@AddressId", userAddress.Addresses.AddressId);

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        // Assuming that the stored procedure returns the updated UserAddress details
                        userAddress.UserAddressId = reader.GetInt32(reader.GetOrdinal("UserAddressId"));
                        userAddress.UserId = reader.GetInt32(reader.GetOrdinal("UserId"));
                        userAddress.Addresses.AddressId = reader.GetInt32(reader.GetOrdinal("AddressId"));
                    }
                }
            }

            return userAddress;
        }

        public async Task<StudentAddress> UpsertStudentAddressesAsync(StudentAddress studentAddress, SqlTransaction transaction)
        {
            if (studentAddress == null)
            {
                throw new ArgumentException("student Address is null or empty");
            }

            using (SqlCommand command = new SqlCommand("upsertStudentAddress", transaction.Connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters to the stored procedure
                command.Parameters.AddWithValue("@StudentAddressId", studentAddress.StudentAddressId);
                command.Parameters.AddWithValue("@StudentId", studentAddress.StudentId);
                command.Parameters.AddWithValue("@AddressId", studentAddress.Address.AddressId);

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        // Assuming that the stored procedure returns the updated UserAddress details
                        studentAddress.StudentAddressId = reader.GetInt32(reader.GetOrdinal("StudentAddressId"));
                        studentAddress.StudentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                        studentAddress.Address.AddressId = reader.GetInt32(reader.GetOrdinal("AddressId"));
                    }
                }
            }

            return studentAddress;
        }

    }
    
}




