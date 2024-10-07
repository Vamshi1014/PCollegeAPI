using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Flyurdreamcommands.Helpers;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Models.Enum;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using pcollegecommands.Models.Datafields;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class AgentRepository :  DataRepositoryBase, IAgentRepository
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        DataTables objDataTables = new DataTables();
        public AgentRepository(IConfiguration config, ILogger<AgentRepository> logger) : base(logger, config)
        {
            _config = config;
            _logger = logger;
        }
        public List<Responses> BulkUpsertResponses(List<Responses> responses)
        {
            List<Responses> insertedResponses = new List<Responses>();

            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                SqlCommand command = new SqlCommand("BulkUpsertResponses", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Create table-valued parameter
                DataTable responsesTable = objDataTables.ResponsesDataTable();
                foreach (Responses response in responses)
                {
                    responsesTable.Rows.Add(response.ResponsesId, response.QuestionId, response.CompanyId, response.ResponseText);
                }

                SqlParameter parameter = command.Parameters.AddWithValue("@ResponsesData", responsesTable);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "dbo.ResponsesTableType"; // Change to match your table type name
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Responses insertedResponse = new Responses
                    {
                        ResponsesId = !reader.IsDBNull(0) ? reader.GetInt32(0) : default(int),
                        QuestionId = !reader.IsDBNull(1) ? reader.GetInt32(1) : default(int),
                        CompanyId = !reader.IsDBNull(2) ? reader.GetInt32(2) : default(int),
                        ResponseText = !reader.IsDBNull(3) ? reader.GetString(3) : null
                    };

                    insertedResponses.Add(insertedResponse);
                }
            }

            return insertedResponses;
        }

        public async Task<Agent> CompanyDetailsAsyncByCompanyId(int comanyId)
        {
            CompanyDetails companyDetails = new CompanyDetails();
            Agent agent = new Agent();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("GetCompanyDetailsByCompanyId", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CompanyId", comanyId);  // Replace 'stud18' with the actual unique ID you want to query
                        cmd.Parameters.AddWithValue("@IsParent", 1);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            do
                            {
                                if (reader.HasRows)
                                {
                                    if (reader.Read())
                                    {
                                        if (companyDetails.Company == null)
                                        {
                                            companyDetails.Company = new Company()
                                            {
                                                CompanyID = reader.GetInt32OrDefault("companyid"),
                                                CompanyName = reader.GetSafeString("CompanyName"),
                                                BusinessRegistrationNumber = reader.GetSafeString("BusinessRegistrationNumber"),
                                                CompanyWebAddress = reader.GetSafeString("CompanyWebAddress"),
                                                IsActive = reader.GetBoolean("IsActive"),
                                                CompanyLogo = reader.GetSafeString("CompanyLogo")

                                            };
                                            companyDetails.PortalWebAddress = reader.GetSafeString("PortalWebAddress");
                                            companyDetails.PortalWebAddress2 = reader.GetSafeString("PortalWebAddress2");
                                            companyDetails.PortalocalAddress = reader.GetSafeString("PortalocalAddress");
                                            companyDetails.TradeName = reader.GetSafeString("PortalocalAddress");
                                            companyDetails.Mobile = reader.GetSafeString("mobile");
                                            companyDetails.email = reader.GetSafeString("email");
                                        }
                                    }
                                    if (reader.NextResult() && reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("CompanyUserId"))
                                            {
                                                companyDetails.CompanyUser ??= new CompanyUser
                                                {
                                                    CompanyId = reader.GetInt32OrDefault("companyid"),
                                                    BranchId = reader.GetInt32OrDefault("BranchId"),
                                                    CompanyUserId = reader.GetInt32OrDefault("CompanyUserId"),
                                                    IsParent = reader.GetInt32OrDefault("IsParent"),
                                                    IsPrimaryContact = reader.GetInt32OrDefault("IsPrimaryContact")
                                                };
                                            }
                                        }
                                    }
                                    if (reader.NextResult() && reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("userid"))
                                            {
                                                companyDetails.CompanyUser.User ??= new User
                                                {
                                                    UserId = reader.GetInt32OrDefault("userid"),
                                                    Email = reader.GetSafeString("email"),
                                                    PasswordHash = reader.GetSafeString("password_hash"),
                                                    CreatedAt = reader.GetSafeDateTime("created_at"),
                                                    Token = reader.GetSafeString("token"),
                                                    UserVerified = reader.GetBoolean("user_verified"),
                                                    VerificationUrl = reader.GetSafeString("verification_url"),
                                                    Salutation = reader.GetSafeString("salutation"),
                                                    GroupId = reader.GetInt32OrDefault("groupId"),
                                                    FirstName = reader.GetSafeString("firstname"),
                                                    lastName = reader.GetSafeString("lastname"),
                                                    logintoken = reader.GetSafeString("logintoken"),
                                                    IsActive = reader.GetBoolean("isActive"),
                                                    Mobile = reader.GetSafeString("mobile"),
                                                    CountryCode = reader.GetInt32OrDefault("countrycode")
                                                };
                                            }
                                        }
                                    }
                                    agent.Company = companyDetails;
                                    if (reader.NextResult() && reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("AgentID"))
                                            {
                                                agent.AgentInformation ??= new Agent_Information
                                                {
                                                    AgentID = reader.GetInt32OrDefault("AgentID"),
                                                    AgentName = reader.GetSafeString("AgentName"),
                                                    CertifyingPersonName = reader.GetSafeString("CertifyingPersonName"),
                                                    CertifyingPersonRole = reader.GetSafeString("CertifyingPersonRole"),
                                                    Signature = reader.GetSafeString("Signature"),
                                                    Date = reader.GetSafeDateTime("Date"),
                                                    ICEFAccreditation = reader.GetSafeString("ICEFAccreditation"),
                                                    ICEFDocument = reader.GetInt32OrDefault("ICEFDocument"),
                                                    LegalStatus = reader.GetSafeString("LegalStatus"),
                                                    LegalStatusDocument = reader.GetInt32OrDefault("LegalStatusDocument"),
                                                    ServiceCharges = reader.GetSafeString("ServiceCharges"),
                                                    UploadedOn = reader.GetSafeDateTime("UploadedOn"),
                                                    CreatedOn = reader.GetSafeDateTime("CreatedOn"),
                                                    //Status = reader.GetInt32OrDefault("Status"),
                                                    //IsActive = reader.GetBoolean("isActive"),
                                                    CompanyId = reader.GetInt32OrDefault("CompanyId"),
                                                    BranchId = reader.GetInt32OrDefault("BranchId"),
                                                    Agent_Unique_Id = reader.GetSafeString("Agent_Unique_Id")
                                                };
                                            }
                                        }
                                    }

                                    if (reader.NextResult() && reader.HasRows) // Move to the next result set
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("EstimateOfStudents"))
                                            {
                                                agent.EstimateStudentsperintake ??= new List<EstimateStudentsperintake>();

                                                EstimateStudentsperintake estimateStudentsperintake = new EstimateStudentsperintake
                                                {
                                                    Id = reader.GetInt32OrDefault("id"),
                                                    IntakeId =new Intake { IntakeId= reader.GetInt32OrDefault("IntakeId") },
                                                    EstimateOfStudents = reader.GetInt32OrDefault("EstimateOfStudents"),
                                                    CreatedOn = reader.GetSafeDateTime("CreatedOn"),
                                                    UploadedOn = reader.GetSafeDateTime("UploadedOn"),
                                                    CompanyId = reader.GetInt32OrDefault("CompanyId"),
                                                    BranchId = reader.GetSafeInt("BranchId")
                                                };

                                                agent.EstimateStudentsperintake.Add(estimateStudentsperintake);
                                            }
                                        }
                                    }

                                    if (reader.NextResult() && reader.HasRows) // Move to the next result set
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("CountryId"))
                                            {
                                                agent.listTargetCountries ??= new List<TargetCountries>();

                                                TargetCountries targetCountries = new TargetCountries
                                                {
                                                    Id = reader.GetInt32OrDefault("id"),    
                                                    Country = new Country { CountryID = reader.GetInt32OrDefault("CountryId"),
                                                    CountryName= reader.GetSafeString("CountryName")
                                                    },
                                                    CreatedOn = reader.GetSafeDateTime("CreatedOn"),
                                                    UploadedOn = reader.GetSafeDateTime("UploadedOn"),
                                                    CompanyId = reader.GetInt32OrDefault("CompanyId"),
                                                    BranchId = reader.GetInt32OrDefault("BranchId")
                                                };
                                                agent.listTargetCountries.Add(targetCountries);
                                            }
                                        }
                                    }

                                    if (reader.NextResult() && reader.HasRows) // Move to the next result set
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("document_type"))
                                            {
                                                agent.listCompanyDocuments ??= new List<CompanyDocuments>();

                                                Document document = new Document { 
                                                DocumentId= reader.GetInt32OrDefault("DocumentId"),
                                                DocumentName = reader.GetSafeString("document_name"),
                                                FilePath = reader.GetSafeString("file_path"),
                                                    UploadedAt = reader.GetSafeDateTime("uploaded_at"),
                                                    DocumentType = new DocumentType
                                                    {
                                                        Id = (DocumentTypeId)Enum.ToObject(typeof(DocumentTypeId), reader.GetInt32("document_type")),
                                                    },
                                                    ContainerName = reader.GetSafeString("container_name")
                                                };
                                                CompanyDocuments companyDocuments = new CompanyDocuments
                                                {
                                                    CompanyDocumentId = reader.GetInt32OrDefault("CompanyDocumentId"),
                                                    CompanyId= reader.GetInt32OrDefault("CompanyId"),
                                                                                                       
                                                };
                                                companyDocuments.Documents.Add(document);
                                                agent.listCompanyDocuments.Add(companyDocuments);
                                            }
                                        }
                                    }
                                }
                            }
                            
                            while (await reader.NextResultAsync());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return agent;
        }
    }
}
