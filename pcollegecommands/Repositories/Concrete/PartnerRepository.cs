using System.Data;
using Flyurdreamcommands.Constants;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Models.Enum;
using Flyurdreamcommands.Repositories.Abstract;
using Flyurdreamcommands.Service.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System;
using Document = Flyurdreamcommands.Models.Datafields.Document;
using Microsoft.Identity.Client;
using System.Text;
using System.Reflection.Metadata;
using System.ComponentModel.Design;
using System.Diagnostics.Metrics;
using Flyurdreamcommands.Helpers;
using pcollegecommands.Models.Datafields;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class PartnerRepository : DataRepositoryBase, IPartnerRepository
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        DataTables _objDataTables = new DataTables();
        private readonly IDocumentRepository _documentRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IReferenceRepository _referenceRepository;
        private readonly IUserRepository _userRepository;
        protected readonly IBlobHandler _blobHandler;
        public PartnerRepository(IConfiguration config, ILogger<PartnerRepository> logger, IAddressRepository addressRepository
            , IDocumentRepository documentRepository, IReferenceRepository referenceRepository, IUserRepository userRepository, IBlobHandler blobHandler) : base(logger, config)
        {
            _config = config;
            _logger = logger;
            _addressRepository = addressRepository;
            _documentRepository = documentRepository;
            _referenceRepository = referenceRepository;
            _userRepository = userRepository;
            _blobHandler = blobHandler;
        }

        public async Task<CompanyDetails> ExecuteUpsertCompanyDetailsAsync(CompanyDetails companyDetails, SqlTransaction transaction)
        {
            if (companyDetails.Company != null)
            {
                var company = companyDetails.Company;
                using (SqlCommand command = new SqlCommand("UpsertCompanyDetails", transaction.Connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CompanyId", (object)company.CompanyID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CompanyName", company.CompanyName);
                    command.Parameters.AddWithValue("@BusinessRegistrationNumber", company.BusinessRegistrationNumber);
                    command.Parameters.AddWithValue("@CompanyWebAddress", (object)company.CompanyWebAddress ?? DBNull.Value);
                    command.Parameters.AddWithValue("@IsActive", company.IsActive);
                    command.Parameters.AddWithValue("@PortalWebAddress", companyDetails.PortalWebAddress);
                    command.Parameters.AddWithValue("@PortalWebAddress2", companyDetails.PortalWebAddress2);
                    command.Parameters.AddWithValue("@Portallocaladdress", companyDetails.PortalocalAddress);
                    command.Parameters.AddWithValue("@TradeName", companyDetails.TradeName);
                    command.Parameters.AddWithValue("@Mobile", companyDetails.Mobile);
                    command.Parameters.AddWithValue("@email", companyDetails.email);
                    command.Parameters.AddWithValue("@Status", 0);
                    //command.Parameters.AddWithValue("@UploadedOn", DateTime.Now);
                    command.Parameters.AddWithValue("@CreatedBy", (object)companyDetails.CompanyUser?.User?.UserId ?? DBNull.Value);
                    //command.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                    SqlParameter resultAddressIdParam = new SqlParameter("@ResultAddressID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(resultAddressIdParam);
                    SqlParameter successMessageParam = new SqlParameter("@SuccessMessage", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(successMessageParam);
                    await command.ExecuteNonQueryAsync();
                    company.CompanyID = resultAddressIdParam.Value != DBNull.Value ? (int)resultAddressIdParam.Value : (int?)null;
                    company.Result = successMessageParam.Value as string;
                }

            }

            return companyDetails;
        }



        public async Task<DocumentReponse> UpsertAgentDetails(DocumentReponse documentReponse)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    // documentReponse = await _documentRepository.UpsertDocumentsAsync(documentReponse, transaction);
                    //  documentReponse = await _documentRepository.InsertCompanyDocumentsAsync(documentReponse, transaction);
                    // Bulk upsert responses
                    //   documentReponse = await UpsertResponsesAsync(documentReponse, transaction);
                    // Commit transaction
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Rollback transaction on error
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    // Close connection
                    _ = connection.CloseAsync();
                }
            }

            return documentReponse;
        }

   
        private int GetDocumentIdForResponse(IList<CompanyDocuments> companyDocuments, int documentTypeId)
        {
            int documentId = 0;

            // Convert documentTypeId to DocumentTypeId enum
            DocumentTypeId type;
            switch (documentTypeId)
            {
                case 1:
                    type = DocumentTypeId.ICEFAccreditation;
                    break;
                case 2:
                    type = DocumentTypeId.LegalStatus;
                    break;
                default:
                    type = DocumentTypeId.OtherBusinessDocument; // Adjust default case as per your enum
                    break;
            }

            // Find the matching document in companyDocuments based on DocumentTypeId
            var companyDocument = companyDocuments?.FirstOrDefault();
            if (companyDocument != null)
            {
                var document = companyDocument.Documents?.FirstOrDefault(d => (DocumentTypeId)d.DocumentType.Id == type);
                if (document != null)
                {
                    documentId = document.DocumentId;
                }
            }

            return documentId;
        }



        private async Task<List<(int QuestionsId, int DocumentTypeId)>> GetQuestionsIdAndDocumentTypeByCompanyId(int companyId)
        {
            List<(int QuestionsId, int DocumentTypeId)> results = new List<(int, int)>();

            using (SqlConnection conn = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("GetQuestionsIdByFormdataIdAndCompanyId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", companyId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int questionsId = reader.GetInt32(0);
                            int documentTypeId = reader.GetInt32(1);

                            results.Add((questionsId, documentTypeId));
                        }
                    }
                }
            }

            return results;
        }

        public async Task<CompanyDetails> UpsertPrimaryUserAgent(CompanyDetails companyDetails)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    // Upsert company details
                    companyDetails.CompanyUser.User = await _userRepository.UpdateUser(companyDetails.CompanyUser?.User, transaction);
                    companyDetails = await _referenceRepository.UpsertReferencesAsync(companyDetails, transaction);
                    companyDetails = await _referenceRepository.UpsertCompanyReferencesAsync(companyDetails, transaction); // mapping
                    //partner.CompanyUser = await _userRepository.UpsertCompanyUserAsync(partner?.CompanyUser, transaction);

                    // Commit transaction
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Rollback transaction on error
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    // Close connection
                    _ = connection.CloseAsync();
                }
            }

            return companyDetails;
        }
        public async Task<Agent> UpsertCompanyDetailsAsync(Agent objagent)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                CompanyDetails companyDetails = objagent?.Company;
                Agent_Information agent_Information = objagent.AgentInformation;
                List<TargetCountries> target_Countries = objagent.listTargetCountries;
                List<EstimateStudentsperintake> estimateStudents_intake = objagent.EstimateStudentsperintake;

                try
                {
                    // Upsert company details
                    if (objagent.IsUpdate.Company_IsUpdate == true)
                    {
                        companyDetails = await ExecuteUpsertCompanyDetailsAsync(companyDetails, transaction);
                        if (objagent.IsUpdate.CompanyAddress_IsUpdate == true)
                        {
                            // Upsert address
                            companyDetails.CompanyAddress.Addresses = await _addressRepository.UpsertAddressAsync(companyDetails?.CompanyAddress?.Addresses, transaction);
                            companyDetails.CompanyAddress = await _addressRepository.UpsertCompanyAddressesAsync(companyDetails?.CompanyAddress, transaction);
                        }
                    }
                    companyDetails.SetCompanyId(companyDetails.Company);
                    if (objagent.IsUpdate.User_IsUpdate == true)
                    {
                        companyDetails.CompanyUser.User = await _userRepository.UpdateUser(companyDetails?.CompanyUser?.User, transaction);

                    }
                    if (objagent.IsUpdate.CompanyDocuments_IsUpdate == true)
                    {
                        DocumentReponse objDocumentReponse = new DocumentReponse();
                        for (int i = 0; i < objagent.listCompanyDocuments.Count; i++)
                        {
                            objagent.listCompanyDocuments[i].Documents = (List<Document>?)await _documentRepository.UpsertDocumentAsync(objagent.listCompanyDocuments[i].Documents, (int)objagent.listCompanyDocuments[i]?.CompanyId, transaction, null);
                        } 
                        objDocumentReponse.CompanyDocuments = objagent.listCompanyDocuments;
                        objDocumentReponse.CompanyId = (int)objagent.listCompanyDocuments[0]?.CompanyId;
                        objDocumentReponse = await _documentRepository.InsertCompanyDocumentsAsync(objDocumentReponse, transaction);
                    }
                    if (objagent.IsUpdate.AgentInformation_IsUpdate == true)
                    {
                        agent_Information = await UpsertAgentInformation(agent_Information, transaction);

                    }
                    if (objagent.IsUpdate.listTargetCountries_IsUpdate == true)
                    {
                        target_Countries = await UpsertTargetCountriesInformation(target_Countries, transaction);
                    }
                    if (objagent.IsUpdate.EstimateStudentsperintake_IsUpdate == true)
                    {
                        estimateStudents_intake = await UpsertEstimateStudentsperintakeInformation(estimateStudents_intake, transaction);
                    }
                    // Commit transaction
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Rollback transaction on error
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    // Close connection
                    _ = connection.CloseAsync();
                }
            }

            return objagent;
        }

        public async Task<string> UpsertCompanyLogo(byte[] logocontent, string filename, int companyId)
        {
            try
            {
                string result = string.Empty;
                string containername = Const.BusinessDocumentsBlobContainer;
                string blobName = $"{companyId}/{Const.LogoFolder}/{filename}";
                string filepath = await _blobHandler.CreateBlobFromBytes(containername, blobName, logocontent);
                if (filepath != null)
                    result = await UpdateCompanyLogopath(companyId, filepath);
                return filepath;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<string> GetCompanyLogoAsync(int companyId)

        {
            string companyLogo = string.Empty;


            string logoPath = null;

            using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("GetCompanyLogo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@CompanyId", companyId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            logoPath = reader["CompanyLogo"].ToString();
                        }
                    }
                }
                await connection.CloseAsync();
                if (!string.IsNullOrEmpty(logoPath))
                {
                    logoPath = ConfigurationData.BlobRootURI + logoPath;
                    companyLogo = await _blobHandler.GetBlobAsString(logoPath);
                }
                else
                { companyLogo = Const.Logo_Not_Uploaded; }
            }
            return companyLogo;
        }

        public async Task<string> UpdateCompanyLogopath(int companyId, string companyLogoPath)
        {

            try
            {// Replace with your actual connection string
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("UpdateCompanyLogo", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CompanyID", companyId);
                        cmd.Parameters.AddWithValue("@CompanyLogo", companyLogoPath);

                        // Optionally, retrieve the output parameter or result set
                        SqlParameter returnValue = cmd.Parameters.Add("return_value", SqlDbType.VarChar);
                        returnValue.Direction = ParameterDirection.ReturnValue;
                        await cmd.ExecuteNonQueryAsync();
                        // Optionally, retrieve the result from the stored procedure
                        string result = returnValue.Value.ToString();
                        await connection.CloseAsync();
                        return result;
                    }

                }
            }
            catch (Exception ex) { throw; }

        }

        public async Task<string> GetStringBase64(string content)
        {
            // Convert the content string to a byte array
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);

            // Convert the byte array to a base64 encoded string
            string base64Content = Convert.ToBase64String(contentBytes);

            // Return the base64 encoded string
            return await Task.FromResult(base64Content);
        } 

    
        public async Task<Agent_Information> UpsertAgentInformation(Agent_Information agent, SqlTransaction transaction)
        {

            await using (SqlCommand cmd = new SqlCommand("UpsertAgentInformation", transaction.Connection, transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Add parameters for the procedure
                cmd.Parameters.AddWithValue("@AgentID", (object)agent.AgentID ?? DBNull.Value);  // Pass DBNull for NULL values
                cmd.Parameters.AddWithValue("@AgentName", agent.AgentName);
                cmd.Parameters.AddWithValue("@CertifyingPersonName", (object)agent.CertifyingPersonName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CertifyingPersonRole", (object)agent.CertifyingPersonRole ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Signature", (object)agent.Signature ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Date", agent.Date);
                cmd.Parameters.AddWithValue("@ICEFAccreditation", (object)agent.ICEFAccreditation ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ICEFDocument", agent.ICEFDocument);
                cmd.Parameters.AddWithValue("@LegalStatus", (object)agent.LegalStatus ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LegalStatusDocument", agent.LegalStatusDocument);
                cmd.Parameters.AddWithValue("@ServiceCharges", (object)agent.ServiceCharges ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UploadedBy", (object)agent.UploadedBy.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UploadedOn", DateTime.Now);
                cmd.Parameters.AddWithValue("@CreatedBy", (object)agent.UploadedBy.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                cmd.Parameters.AddWithValue("@Status", (object)agent.Status ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", agent.IsActive);
                cmd.Parameters.AddWithValue("@CompanyId", agent.CompanyId);
                cmd.Parameters.AddWithValue("@BranchId", agent.BranchId);

                // If we are updating, use ExecuteNonQuery
                if (agent.AgentID == 0)
                {
                    // If inserting, use ExecuteScalar to get the new identity value
                    agent.AgentID = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return agent;


                }
                else
                {

                    await cmd.ExecuteNonQueryAsync();
                    return agent;

                }
            }

        }

        public async Task<List<TargetCountries>> UpsertTargetCountriesInformation(List<TargetCountries> targetCountries, SqlTransaction transaction)
        {
            try
            {
                DataTables objDataTables = new DataTables();
                List<TargetCountries> updateTargetCountries = new List<TargetCountries>();               
                using (SqlCommand command = new SqlCommand("UpsertTargetCountries", transaction.Connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    DataTable targetCountriesDataTable = objDataTables.TargetCountriesType(); // Ensure this method creates the DataTable correctly
                    foreach (var targetCountrie in targetCountries)
                    {
                        targetCountriesDataTable.Rows.Add(
                            targetCountrie.Id,
                           targetCountrie.Country?.CountryID,
                           targetCountrie.Country?.CountryName,
                           targetCountrie.CreatedBy?.UserId,
                           targetCountrie.CreatedOn,
                           targetCountrie.UploadedBy?.UserId,
                           targetCountrie.UploadedOn,
                           targetCountrie.CompanyId,
                           targetCountrie.BranchId
                        );
                    }
                    SqlParameter parameter = command.Parameters.AddWithValue("@Countries", targetCountriesDataTable);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.TargetCountriesType";
                    await command.ExecuteNonQueryAsync();

                }
               
                return targetCountries;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public async Task<List<EstimateStudentsperintake>> UpsertEstimateStudentsperintakeInformation(List<EstimateStudentsperintake> estimateStudentsperintakes, SqlTransaction transaction)
        {
            try
            {
                DataTables objDataTables = new DataTables();
                List<EstimateStudentsperintake> updateEstimateStudentsperintakes = new List<EstimateStudentsperintake>();
                using (SqlCommand command = new SqlCommand("UpsertEstimateNumberOfStudents", transaction.Connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    DataTable targetCountriesDataTable = objDataTables.EstimateNumberOfStudentsPerIntakeType(); // Ensure this method creates the DataTable correctly
                    foreach (var estimateStudentsperintake in estimateStudentsperintakes)
                    {
                        targetCountriesDataTable.Rows.Add(
                            estimateStudentsperintake.Id,
                           estimateStudentsperintake.IntakeId?.IntakeId,
                           estimateStudentsperintake.EstimateOfStudents,
                           estimateStudentsperintake.CreatedBy?.UserId,
                           estimateStudentsperintake.CreatedOn,
                           estimateStudentsperintake.UploadedBy?.UserId,
                           estimateStudentsperintake.UploadedOn,
                           estimateStudentsperintake.CompanyId,
                           estimateStudentsperintake.BranchId
                        );
                    }
                    SqlParameter parameter = command.Parameters.AddWithValue("@Estimate", targetCountriesDataTable);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.EstimateNumberOfStudentsPerIntakeType";
                    await command.ExecuteNonQueryAsync();
                }
                
                return estimateStudentsperintakes;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }

}

