using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class ReferenceRepository : DataRepositoryBase, IReferenceRepository
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        DataTables objDataTables = new DataTables();
        public ReferenceRepository(IConfiguration config, ILogger<ReferenceRepository> logger) : base(logger, config)
        {
            _config = config;
            _logger = logger;
        }
        public async Task<CompanyDetails> UpsertReferencesAsync(CompanyDetails companyDetails, SqlTransaction transaction)
        {
            try
            {
                using (SqlCommand command = new SqlCommand("UpsertReference", transaction.Connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Create table-valued parameter
                    DataTable referencesTable = objDataTables.ReferencesDataTable();
                    var companyReference = companyDetails.CompanyReferences;

                    if (companyReference != null && companyReference.Reference != null && companyReference.Reference.Count > 0)
                    {
                        foreach (var reference in companyReference.Reference)
                        {
                            referencesTable.Rows.Add(reference.ReferenceID, reference.FirstName, reference.LastName, reference.Organisation, reference.Telephone, reference.Email);
                        }
                    }



                    SqlParameter parameter = command.Parameters.AddWithValue("@ReferencesData", referencesTable);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.ReferenceTableType"; // Ensure this matches your table type name

                    SqlParameter successMessageParam = new SqlParameter("@SuccessMessage", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(successMessageParam);

                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    // Create a list to store the updated references
                    List<Reference> updatedReferences = new List<Reference>();

                    while (await reader.ReadAsync())
                    {
                        Reference updatedReference = new Reference
                        {
                            ReferenceID = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Organisation = reader.GetString(3),
                            Telephone = reader.GetString(4),
                            Email = reader.GetString(5)
                        };

                        updatedReferences.Add(updatedReference);
                    }
                    await reader.CloseAsync();

                    // Update the partner's references with the updated references from the database


                    // Update the partner's CompanyDocuments with the inserted documents
                    if (companyDetails.CompanyReferences != null)
                    {
                        companyDetails.CompanyReferences.Reference = updatedReferences;
                    }
                }

                return companyDetails;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<CompanyDetails> UpsertCompanyReferencesAsync(CompanyDetails companyDetails, SqlTransaction transaction)
        {
            if (companyDetails.CompanyReferences == null)
            {
                throw new ArgumentException("Company must have at least one Reference.");
            }
            var companyReference = companyDetails.CompanyReferences;
            List<CompanyReferences> upsertedCompanyReferences = new List<CompanyReferences>();

            using (SqlCommand command = new SqlCommand("UpsertCompanyReferences", transaction.Connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Create DataTable for companyReferences
                DataTable companyReferencesTable = objDataTables.CompanyReferenceTable();

            
                if (companyReference.Reference != null && companyReference.Reference.Count > 0)
                {
                    foreach (var reference in companyReference.Reference)
                    {
                        companyReferencesTable.Rows.Add(
                            companyReference.CompanyReferenceId,
                            companyReference.CompanyId,
                            reference.ReferenceID
                        );
                    }
                }

                SqlParameter companyReferencesParam = command.Parameters.AddWithValue("@CompanyReferences", companyReferencesTable);
                companyReferencesParam.SqlDbType = SqlDbType.Structured;
                companyReferencesParam.TypeName = "dbo.CompanyReferenceTableType";

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int companyReferenceId = reader.GetInt32(0);
                        int companyId = reader.GetInt32(1);
                        int referenceId = reader.GetInt32(2);

                        // Find existing CompanyReferences object or create new
                        //var companyReferences = upsertedCompanyReferences
                        //    .FirstOrDefault(cr => cr.CompanyReferenceId == companyReferenceId && cr.CompanyId == companyId);

                        ////if (companyReferences == null)
                        //{
                        //    companyReferences = new CompanyReferences
                        //    {
                        //        CompanyReferenceId = companyReferenceId,
                        //        CompanyId = companyId,
                        //        Reference = new List<Reference>()
                        //    };
                        //    upsertedCompanyReferences.Add(companyReferences);
                        //}

                        // Add the new Reference object to the existing or newly created CompanyReferences
                        companyReference.CompanyReferenceId = companyReferenceId;
                    }
                }
            }

            //// Assign the updated list of CompanyReferences to companyDetails
            //companyDetails.CompanyReferences = new CompanyReferences
            //{
            //    CompanyReferenceId = upsertedCompanyReferences[0].CompanyReferenceId,
            //    CompanyId = upsertedCompanyReferences[0].CompanyId,
            //    Reference = upsertedCompanyReferences[0].Reference.ToList()
            //};

            return companyDetails;
        }

         public async Task<List<EmergencyContact>> UpsertEmergencyContactsAsync(List<EmergencyContact> emergencyContacts)
        {
            List<EmergencyContact> updatedEmergencyContacts = new List<EmergencyContact>();

            try
            {
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                using (var command = new SqlCommand("UpsertEmergencyContacts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Create and add table-valued parameter
                    // Create and populate table-valued parameter
                    DataTable emergencycontactDataTable = objDataTables.EmergencyContactDataTable(); // Ensure this method creates the DataTable correctly
                    foreach (var emergencyContact in emergencyContacts)
                    {
                        emergencycontactDataTable.Rows.Add(
                            emergencyContact.EmergencyId,
                            emergencyContact.FirstName,
                            emergencyContact.LastName,
                            emergencyContact.Email,
                            emergencyContact.Telephone,
                            emergencyContact.StudentId
                        );
                    }

                    SqlParameter tableParameter = command.Parameters.AddWithValue("@EmergencyContacts", emergencycontactDataTable);
                    tableParameter.SqlDbType = SqlDbType.Structured;

                    // Add success message output parameter
                    SqlParameter successMessageParam = new SqlParameter("@SuccessMessage", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(successMessageParam);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                var updatedContact = new EmergencyContact
                                {
                                    EmergencyId = reader.GetInt32(0),
                                    FirstName = reader.GetString(1),
                                    LastName = reader.GetString(2),
                                    Telephone = reader.GetString(3),
                                    Email = reader.GetString(4),
                                    StudentId = reader.GetInt32(5)
                                };

                                updatedEmergencyContacts.Add(updatedContact);
                            }
                        }
                    }
                    string successMessage = (string)successMessageParam.Value;

                    // Assign the success message to each item in the list
                    foreach (var educationData in updatedEmergencyContacts)
                    {
                        educationData.Response = successMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }

            return updatedEmergencyContacts;
        }

     

    }


}