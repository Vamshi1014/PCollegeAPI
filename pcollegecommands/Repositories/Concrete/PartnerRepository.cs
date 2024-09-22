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

        public async Task<DocumentReponse> UpsertResponseDetails(DocumentReponse documentResponse)
        {
            string Id = string.Empty;
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {// Get the base64 encoded content

                    //RGVmYXVsdCBWYWx1ZQ== //c3RyaW5n
                    // Compare the content with the expected base64 value
                    if (documentResponse.CompanyDocuments[0].Documents[0].Content == null)
                    {
                        // Perform necessary operations if contents match
                    }
                    else
                    {
                        IList<Document> documents = new List<Document>();
                        // Upsert and insert documents if contents do not match
                        documents = await _documentRepository.UpsertDocumentAsync(documentResponse.CompanyDocuments[0].Documents, documentResponse.CompanyId, transaction, Id);
                        foreach (var companyDocuments in documentResponse.CompanyDocuments)
                        {
                            companyDocuments.Documents = documents;
                        }
                        // documentResponse = await _documentRepository.InsertCompanyDocumentsAsync(documentResponse, transaction);
                    }
                    // Bulk upsert responses
                    documentResponse = await UpsertResponsesAsync(documentResponse, transaction);
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

            return documentResponse;
        }
        public async Task<DocumentReponse> UpsertResponsesAsync(DocumentReponse documentResponse, SqlTransaction transaction)
        {
            try
            {
                DataTable responsesTable = _objDataTables.ResponsesDataTable();

                // Fetch QuestionsId and DocumentType from database
                List<(int QuestionsId, int DocumentTypeId)> results = await GetQuestionsIdAndDocumentTypeByCompanyId(documentResponse.CompanyId);

                if (documentResponse.Responses != null)
                {
                    foreach (var response in documentResponse.Responses)
                    {
                        // Find the matching result for the current response's QuestionId                           
                        var result = results.FirstOrDefault(r => r.QuestionsId == response.QuestionId);
                        if (result != default)
                        {
                            int documentId = GetDocumentIdForResponse(documentResponse.CompanyDocuments, result.DocumentTypeId);

                            // Update responsesTable with response data
                            responsesTable.Rows.Add(response.ResponsesId, response.QuestionId, documentResponse.CompanyId, response.ResponseText, documentId);
                        }
                        else
                        {
                            // If no matching result is found, add with DocumentId = 0
                            responsesTable.Rows.Add(response.ResponsesId, response.QuestionId, documentResponse.CompanyId, response.ResponseText, 0);
                        }
                    }
                }

                using (SqlCommand command = new SqlCommand("BulkUpsertResponses", transaction.Connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add responsesTable as TVP parameter
                    SqlParameter parameter = command.Parameters.AddWithValue("@ResponsesData", responsesTable);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.ResponsesTableType"; // Ensure this matches your table type name

                    // Execute the stored procedure and get the updated responses
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    // Create a list to store the updated responses
                    List<Responses> updatedResponses = new List<Responses>();

                    while (await reader.ReadAsync())
                    {
                        Responses updatedResponse = new Responses
                        {
                            ResponsesId = reader.GetInt32(0),
                            QuestionId = reader.GetInt32(1),
                            CompanyId = reader.GetInt32(2),
                            ResponseText = reader.GetString(3),
                            DocumentId = reader.GetInt32(4),
                        };

                        updatedResponses.Add(updatedResponse);
                    }

                    await reader.CloseAsync();

                    // Update the documentResponse object with the updated responses
                    documentResponse.Responses = updatedResponses;
                }

                return documentResponse;
            }
            catch (Exception e)
            {
                throw new Exception("Error upserting responses.", e);
            }
        }


        private int GetDocumentIdForResponse(IList<CompanyDocuments> companyDocuments, int documentTypeId)
        {
            int documentId = 0;

            // Convert documentTypeId to DocumentTypeId enum
            DocumentTypeId type;
            switch (documentTypeId)
            {
                case 1:
                    type = DocumentTypeId.BusinessCertificate;
                    break;
                case 2:
                    type = DocumentTypeId.CompanyProfile;
                    break;
                default:
                    type = DocumentTypeId.OtherBusiness; // Adjust default case as per your enum
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
                try
                {
                    // Upsert company details
                    if (objagent.IsUpdate.Company_IsUpdate == true)
                    {
                        companyDetails = await ExecuteUpsertCompanyDetailsAsync(companyDetails, transaction);
                        // Upsert address
                        companyDetails.CompanyAddress.Addresses = await _addressRepository.UpsertAddressAsync(companyDetails?.CompanyAddress?.Addresses, transaction);
                        companyDetails.CompanyAddress = await _addressRepository.UpsertCompanyAddressesAsync(companyDetails?.CompanyAddress, transaction);
                    }
                    companyDetails.SetCompanyId(companyDetails.Company);
                    if (objagent.IsUpdate.User_IsUpdate == true)
                    {
                        companyDetails.CompanyUser.User = await _userRepository.UpdateUser(companyDetails?.CompanyUser?.User, transaction);
                        companyDetails.CompanyUser = await _userRepository.UpsertCompanyUserAsync(companyDetails?.CompanyUser, transaction);

                    }
                    if (objagent.IsUpdate.AgentInformation_IsUpdate == true)
                    {
                        agent_Information= await UpsertAgentInformation(agent_Information, transaction);


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

        
        public static Document? GetMatchingDocument(Partner partner, Questions question)
        {
            foreach (var companyDocumentGroup in partner.CompanyDocuments)
            {
                foreach (var companyDocument in companyDocumentGroup.Documents)
                {
                    if (companyDocument.DocumentType.Id == question.DocumentType.Id)
                    {
                        return companyDocument;
                    }
                }
            }
            return null;
        }

        public void UpdateResponsesWithDocumentId(Partner partner)
        {
            var questions = partner.Questions;
            var responses = partner.Responses;

            foreach (var response in responses)
            {
                var question = questions.FirstOrDefault(q => q.QuestionId == response.QuestionId);
                if (question != null && question.InputType == InputType.FileUpload)
                {
                    var matchingDocument = GetMatchingDocument(partner, question);
                    if (matchingDocument != null)
                    {
                        response.DocumentId = matchingDocument.DocumentId;
                    }
                }
            }

            // Optionally, save or return the updated responses list
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


    }


}

