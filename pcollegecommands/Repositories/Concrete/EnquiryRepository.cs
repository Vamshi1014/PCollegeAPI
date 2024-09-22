using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class EnquiryRepository:DataRepositoryBase,IEnquiryRepository
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        public EnquiryRepository(IConfiguration config, ILogger<EnquiryRepository> logger) : base(logger, config)
        {
            _config = config;
            _logger = logger;
        }

        //public  List<Enquiry> GetEnquiryiesByUniqueId(string? uniqueId)
        //{
        //    List<Enquiry> enquiries = new List<Enquiry>();

        //    using (SqlConnection conn = new SqlConnection(ConfigurationData.DbConnectionString))
        //    {
        //        using (SqlCommand cmd = new SqlCommand("GetEnquiries", conn))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            if (!string.IsNullOrEmpty(uniqueId))
        //            {
        //                cmd.Parameters.Add(new SqlParameter("@Unique_Id", uniqueId));
        //            }
        //            else
        //            {
        //                cmd.Parameters.Add(new SqlParameter("@Unique_Id", DBNull.Value));
        //            }

        //            conn.Open();
        //            using (SqlDataReader reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    Enquiry enquiry = new Enquiry();

        //                    enquiry.Unique_Id = reader.IsDBNull(reader.GetOrdinal("Unique_Id")) ? null : reader.GetValue(reader.GetOrdinal("Unique_Id")).ToString();
        //                    enquiry.FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? null : reader.GetString(reader.GetOrdinal("FirstName"));
        //                    enquiry.LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? null : reader.GetString(reader.GetOrdinal("LastName"));
        //                    enquiry.Mobile_Number = reader.IsDBNull(reader.GetOrdinal("Mobile_Number")) ? null : reader.GetString(reader.GetOrdinal("Mobile_Number"));
        //                    enquiry.Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email"));
        //                    enquiry.Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address"));
        //                    enquiry.Country.CountryName = reader.IsDBNull(reader.GetOrdinal("Country")) ? null : reader.GetString(reader.GetOrdinal("Country"));
        //                    enquiry.State.StateName = reader.IsDBNull(reader.GetOrdinal("State")) ? null : reader.GetString(reader.GetOrdinal("State"));
        //                    enquiry.City.CityName = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City"));
        //                    enquiry.Postal_code = reader.IsDBNull(reader.GetOrdinal("Postal_code")) ? null : reader.GetString(reader.GetOrdinal("Postal_code"));
        //                    enquiry.Passport_Number = reader.IsDBNull(reader.GetOrdinal("passport_number")) ? null : reader.GetString(reader.GetOrdinal("passport_number"));
        //                    enquiry.Married = reader.IsDBNull(reader.GetOrdinal("married")) ? false : reader.GetBoolean(reader.GetOrdinal("married"));
        //                    enquiry.Nationality = reader.IsDBNull(reader.GetOrdinal("Nationality")) ? null : reader.GetString(reader.GetOrdinal("Nationality"));
        //                    enquiry.Date_Of_Birth = reader.IsDBNull(reader.GetOrdinal("Date_of_birth")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("Date_of_birth"));
        //                    enquiry.Current_Education = reader.IsDBNull(reader.GetOrdinal("current_education")) ? null : reader.GetString(reader.GetOrdinal("current_education"));
        //                    enquiry.Country_Interested = reader.IsDBNull(reader.GetOrdinal("country_interested")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("country_interested"));
        //                    enquiry.Previous_Visa_Refusal = reader.IsDBNull(reader.GetOrdinal("previous_visa_refusal")) ? false : reader.GetBoolean(reader.GetOrdinal("previous_visa_refusal"));
        //                    enquiry.Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes"));
        //                    enquiry.CreatedOn = reader.IsDBNull(reader.GetOrdinal("createdon")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("createdon"));

        //                    enquiries.Add(enquiry);
        //                }
        //            }
        //        }
        //    }

        //    return enquiries;
        //}

        public async Task<(string statusMessage, List<Enquiry> enquiries)> GetEnquiries(int userId, string? companyId, string? branchId, int? countryIntrested
            , string? passportnumber, string? uniqueId)
        {
            List<Enquiry> enquiries = new List<Enquiry>();
            string statusMessage = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("GetEnquiries", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        // Check for "null" string and convert it to actual null
                        passportnumber = passportnumber == "null" ? null : passportnumber;
                        uniqueId = uniqueId == "null" ? null : uniqueId;
                        companyId = companyId == "null" ? null : companyId;
                        branchId = branchId == "null" ? null : branchId;

                        cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                        cmd.Parameters.Add(new SqlParameter("@CompanyId", (object?)companyId ?? DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@BranchId", (object?)branchId ?? DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@country_interested", (object?)countryIntrested ?? DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@passportnumber", (object?)passportnumber ?? DBNull.Value));
                        cmd.Parameters.Add(new SqlParameter("@UniqueId", (object?)uniqueId ?? DBNull.Value));


                        SqlParameter statusParam = new SqlParameter("@status", SqlDbType.NVarChar, 100)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(statusParam);

                        conn.Open();

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                Enquiry enquiry = new Enquiry
                                {

                                    _Enquiry = reader.IsDBNull(reader.GetOrdinal("Enquiry")) ? null : reader.GetValue(reader.GetOrdinal("Enquiry")).ToString(),
                                    Enquiry_Id = reader.IsDBNull(reader.GetOrdinal("enquiry_id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("enquiry_id")),
                                    Unique_Id = reader.IsDBNull(reader.GetOrdinal("Unique_Id")) ? null : reader.GetString(reader.GetOrdinal("Unique_Id")),
                                    FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? null : reader.GetString(reader.GetOrdinal("FirstName")),
                                     LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? null : reader.GetString(reader.GetOrdinal("LastName")),
                                    Mobile_Number = reader.IsDBNull(reader.GetOrdinal("Mobile_Number")) ? null : reader.GetString(reader.GetOrdinal("Mobile_Number")),
                                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                                    Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                                    Country = new Country
                                    {
                                        CountryName = reader.IsDBNull(reader.GetOrdinal("Country")) ? null : reader.GetString(reader.GetOrdinal("Country")),
                                        Dial = (int)(reader.IsDBNull(reader.GetOrdinal("dial")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("dial")))
                                    },
                                    State = new State
                                    {
                                        StateName = reader.IsDBNull(reader.GetOrdinal("State")) ? null : reader.GetString(reader.GetOrdinal("State"))
                                    },
                                    City = new City
                                    {
                                        CityName = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City"))
                                    },
                                    Postal_code = reader.IsDBNull(reader.GetOrdinal("Postal_code")) ? null : reader.GetString(reader.GetOrdinal("Postal_code")),
                                    Passport_Number = reader.IsDBNull(reader.GetOrdinal("passport_number")) ? null : reader.GetString(reader.GetOrdinal("passport_number")),
                                    Married = reader.IsDBNull(reader.GetOrdinal("married")) ? false : reader.GetBoolean(reader.GetOrdinal("married")),
                                    Nationality = reader.IsDBNull(reader.GetOrdinal("Nationality")) ? null : reader.GetString(reader.GetOrdinal("Nationality")),                                   
                                    Date_Of_Birth = reader.IsDBNull(reader.GetOrdinal("Date_of_birth")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("Date_of_birth")),
                                    Current_Education = reader.IsDBNull(reader.GetOrdinal("current_education")) ? null : reader.GetString(reader.GetOrdinal("current_education")),
                                    Country_Interested = reader.IsDBNull(reader.GetOrdinal("country_interested")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("country_interested")),
                                    Previous_Visa_Refusal = reader.IsDBNull(reader.GetOrdinal("previous_visa_refusal")) ? false : reader.GetBoolean(reader.GetOrdinal("previous_visa_refusal")),
                                    Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
                                    CreatedOn = reader.IsDBNull(reader.GetOrdinal("createdon")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("createdon"))
                                };

                                enquiries.Add(enquiry);
                            }
                        }

                        // Retrieve the output parameter value
                        statusMessage = statusParam.Value as string;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }
            return (statusMessage, enquiries);
        }

        public async Task<Enquiry> InsertEnquiryAsync(int userid,Enquiry enquiry, int? companyId, int? branchId)
        {
            Enquiry responseenquiry = new Enquiry();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    // Create a SqlCommand object to execute the stored procedure
                    using (SqlCommand command = new SqlCommand("InsertEnquiry", connection))
                    {
                        // Specify that the SqlCommand is a stored procedure
                        command.CommandType = CommandType.StoredProcedure;
                        // Add parameters to the SqlCommand object
                        command.Parameters.AddWithValue("@user_id", userid);
                        command.Parameters.AddWithValue("@FirstName", enquiry.FirstName);
                        command.Parameters.AddWithValue("@LastName", enquiry.LastName);
                        command.Parameters.AddWithValue("@Mobile_Number", enquiry.Mobile_Number);
                        command.Parameters.AddWithValue("@Email", enquiry.Email);
                        command.Parameters.AddWithValue("@Address", enquiry.Address);
                        command.Parameters.AddWithValue("@Country", enquiry.Country.CountryID);
                        command.Parameters.AddWithValue("@State", enquiry.State.StateID);
                        command.Parameters.AddWithValue("@City", enquiry.City.CityID);
                        command.Parameters.AddWithValue("@Postal_code", enquiry.Postal_code);
                        command.Parameters.AddWithValue("@passport_number", enquiry.Passport_Number);
                        command.Parameters.AddWithValue("@married", enquiry.Married);
                        command.Parameters.AddWithValue("@Nationality", enquiry.Nationality);
                        command.Parameters.AddWithValue("@Date_of_birth", enquiry.Date_Of_Birth);
                        command.Parameters.AddWithValue("@current_education", enquiry.Current_Education);
                        command.Parameters.AddWithValue("@country_interested", enquiry.Country_Interested);
                        command.Parameters.AddWithValue("@CountryCode", enquiry.Country.CountryID);                        
                        command.Parameters.AddWithValue("@previous_visa_refusal", enquiry.Previous_Visa_Refusal);
                        command.Parameters.AddWithValue("@notes", enquiry.Notes);
                        command.Parameters.AddWithValue("@CompanyId", companyId);
                        command.Parameters.AddWithValue("@BranchId", branchId);
                        command.Parameters.Add(new SqlParameter("@Message", SqlDbType.VarChar, 150) { Direction = ParameterDirection.Output });
                        command.Parameters.Add(new SqlParameter("@return_Unique_Id", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output });

                        // Open the connection
                        await connection.OpenAsync();
                        // Execute the stored procedure asynchronously
                        await command.ExecuteNonQueryAsync();
                        // Retrieve the output values
                        responseenquiry.response = command.Parameters["@Message"].Value.ToString();
                        responseenquiry.Unique_Id = command.Parameters["@return_Unique_Id"].Value.ToString();
                        return responseenquiry;
                    }
                }
            }
            catch (Exception ex){
                _logger.LogError(ex.Message);
                throw;
            }
        }


        public async Task<(IEnumerable<Enquiry> enquiries, string status)> GetEnquiriesCompanyBranchByUserAsync(int userId, string companyId = null, string branchId = null)
        {
            using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId, DbType.Int32);
                parameters.Add("@CompanyId", companyId, DbType.String);
                parameters.Add("@BranchId", branchId, DbType.String);
                parameters.Add("@status", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

                var result = await connection.QueryAsync<Enquiry>("[dbo].[GetEnquiriesCompanyBranchByUser]",parameters,commandType: CommandType.StoredProcedure);

                string status = parameters.Get<string>("@status");

                return (result, status);
            }
        }
    }

}

