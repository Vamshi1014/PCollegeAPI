using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Constants;
using Flyurdreamcommands.Helpers;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Models.Enum;
using Flyurdreamcommands.Repositories.Abstract;
using Flyurdreamcommands.Service.Abstract;
using Flyurdreamcommands.Service.Concrete;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spire.Pdf.Tables;
using Spire.Xls.Core.Parser.Biff_Records.Formula;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class DocumentRepository : DataRepositoryBase, IDocumentRepository
    {
        protected readonly IConfiguration _config;
        protected readonly ILogger _logger;
        protected readonly IBlobHandler _blobHandler;
        DataTables objDataTables = new DataTables();
        public DocumentRepository(IConfiguration config, ILogger<DocumentRepository> logger, IBlobHandler blobHandler) : base(logger, config)
        {
            _config = config;
            _logger = logger;
            _blobHandler = blobHandler;
        }

        public async Task<DocumentReponse> UpsertDocumentsAsync(DocumentReponse documentResponse, SqlTransaction transaction)
        {
            List<Document> insertedDocuments = new List<Document>();

            using (SqlCommand command = new SqlCommand("UpsertDocuments", transaction.Connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Create DataTable that matches the DocumentsTableType structure
                DataTable documentsTable = objDataTables.DocumentsDataTable();
                foreach (var companyDocuments in documentResponse.CompanyDocuments)
                {
                    if (companyDocuments != null && companyDocuments.Documents != null)
                    {
                        foreach (Document document in companyDocuments.Documents)
                        {
                            // Assuming UploadFileToBlob is a method that uploads a file and returns the updated document with the file path
                            Document updatedDocument = await UploadFileToBlob(document, documentResponse.CompanyId);
                            documentsTable.Rows.Add(
                                document.DocumentId,
                                updatedDocument.DocumentName,
                                updatedDocument.FilePath,
                                document.UploadedBy,
                                document.UploadedAt,
                                document.DocumentType.Id,
                                updatedDocument.ContainerName
                            );
                        }
                    }
                }

                SqlParameter parameter = command.Parameters.AddWithValue("@DocumentsData", documentsTable);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "dbo.DocumentsTableType";

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Document insertedDocument = new Document
                        {
                            DocumentId = reader.IsDBNull(0) ? default : reader.GetInt32(0),
                            DocumentName = reader.IsDBNull(1) ? null : reader.GetString(1),
                            FilePath = reader.IsDBNull(2) ? null : reader.GetString(2), // Assuming URL is stored in FilePath
                            UploadedBy = reader.IsDBNull(3) ? null : reader.GetString(3),
                            UploadedAt = reader.IsDBNull(4) ? default : reader.GetDateTime(4),
                            DocumentType = new DocumentType
                            {
                                Id = (DocumentTypeId)Enum.ToObject(typeof(DocumentTypeId), reader.GetInt32(5)),
                            },

                            ContainerName = reader.IsDBNull(6) ? null : reader.GetString(6),
                        };

                        insertedDocuments.Add(insertedDocument);

                    }
                }
            }

            // Update the partner's CompanyDocuments with the inserted documents
            foreach (var companyDocuments in documentResponse.CompanyDocuments)
            {
                companyDocuments.Documents = insertedDocuments;
            }

            return documentResponse;
        }


        public async Task<IList<Document>> UpsertDocumentAsync(IList<Document> documentResponse, int companyId, SqlTransaction transaction, string? id = null)
        {
            List<Document> insertedDocuments = new List<Document>();

            using (SqlCommand command = new SqlCommand("UpsertDocuments", transaction.Connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Create DataTable that matches the DocumentsTableType structure
                DataTable documentsTable = objDataTables.DocumentsDataTable();

                if (documentResponse != null && documentResponse.Count > 0)
                {
                    foreach (Document document in documentResponse)
                    {
                        // Assuming UploadFileToBlob is a method that uploads a file and returns the updated document with the file path
                        Document updatedDocument = await UploadFileToBlob(document, companyId, id);
                        documentsTable.Rows.Add(
                            document.DocumentId,
                            updatedDocument.DocumentName,
                            updatedDocument.FilePath,
                            document.UploadedBy,
                            document.UploadedAt,
                            document.DocumentType.Id,
                            updatedDocument.ContainerName
                        );
                    }
                }


                SqlParameter parameter = command.Parameters.AddWithValue("@DocumentsData", documentsTable);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "dbo.DocumentsTableType";
                int count = 0;
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Document insertedDocument = new Document
                        {
                            DocumentId = reader.IsDBNull(0) ? default : reader.GetInt32(0),
                            DocumentName = reader.IsDBNull(1) ? null : reader.GetString(1),
                            FilePath = reader.IsDBNull(2) ? null : reader.GetString(2), // Assuming URL is stored in FilePath
                            UploadedBy = reader.IsDBNull(3) ? null : reader.GetString(3),
                            UploadedAt = reader.IsDBNull(4) ? default : reader.GetDateTime(4),
                            DocumentType = new DocumentType
                            {
                                Id = (DocumentTypeId)Enum.ToObject(typeof(DocumentTypeId), reader.GetInt32(5)),
                            },

                            ContainerName = reader.IsDBNull(6) ? null : reader.GetString(6),

                        };

                        documentResponse[count].DocumentId = insertedDocument.DocumentId;
                        count++;
                        insertedDocuments.Add(insertedDocument);


                    }
                }
            }

            // Update the partner's CompanyDocuments with the inserted documents


            return documentResponse;
        }


        public async Task<Document> UploadFileToBlob(Document document, int companyId, string? id = null) //Id is companyId or userId
        {
            try
            {

                if (document.DocumentType.Id == DocumentTypeId.BusinessCertificate || document.DocumentType.Id == DocumentTypeId.CompanyProfile || document.DocumentType.Id == DocumentTypeId.OtherBusiness)
                {
                    document.DocumentName = companyId + "_" + Guid.NewGuid().ToString() + "_" + document.DocumentName;
                    document.ContainerName = DocumentFor.Business.DocumentForToEnumString().ToLower();
                    document.FilePath = await _blobHandler.CreateBlobFromBytes(document.ContainerName, companyId+"/"+document.DocumentName, document.Content);
                }
                else
                {
                    document.DocumentName = companyId + "_" + id + "_" + Guid.NewGuid().ToString() + "_" + document.DocumentName;
                    document.ContainerName = DocumentFor.Student.DocumentForToEnumString().ToLower();
                    document.FilePath = await _blobHandler.CreateBlobFromBytes(document.ContainerName, companyId + "/" + id+ "/" + document.DocumentName, document.Content);
                }

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }
        }

        public async Task<int> InsertUserDocumentAsync(int userId, int documentId, SqlTransaction transaction)
        {
            int userDocumentId;


            using (SqlCommand command = new SqlCommand("InsertUserDocument", transaction.Connection, transaction))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@DocumentID", documentId);

                SqlParameter userDocumentIdParam = new SqlParameter("@UserDocumentID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(userDocumentIdParam);

                await command.ExecuteNonQueryAsync();

                userDocumentId = (int)userDocumentIdParam.Value;
            }


            return userDocumentId;
        }
        public async Task<DocumentReponse> InsertCompanyDocumentsAsync(DocumentReponse documentResponse, SqlTransaction transaction)
        {
            // Create a DataTable that matches the CompanyDocumentsTableType structure
            DataTable companyDocumentsTable = new DataTable();
            companyDocumentsTable.Columns.Add("CompanyDocumentId", typeof(int));
            companyDocumentsTable.Columns.Add("CompanyId", typeof(int));
            companyDocumentsTable.Columns.Add("DocumentId", typeof(int));

            var companyId = documentResponse.CompanyId;
            try
            {
                // Add the provided data
                foreach (var companyDocuments in documentResponse.CompanyDocuments)
                {
                    if (companyDocuments.Documents != null)
                    {
                        foreach (var document in companyDocuments.Documents)
                        {
                            companyDocumentsTable.Rows.Add(0, companyId, document.DocumentId); // Assuming CompanyDocumentId is 0 for new entries
                        }
                    }
                }

                using (SqlCommand command = new SqlCommand("UpsertCompanyDocuments", transaction.Connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    // Add the table-valued parameter
                    SqlParameter tvpParam = command.Parameters.AddWithValue("@CompanyDocuments", companyDocumentsTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.CompanyDocumentsTableType"; // Ensure this matches your table type name

                    // Execute the command and read the results asynchronously
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        List<int> companyDocumentIds = new List<int>();
                        //int index = 0;

                        //while (await reader.ReadAsync())
                        //{
                        //    int companyDocumentId = reader.GetInt32(0);
                        //    documentResponse.CompanyDocuments[index].CompanyDocumentId = companyDocumentId;
                        //    companyDocumentIds.Add(companyDocumentId);
                        //    index++;
                        //}
                        int index = 0;
                        while (await reader.ReadAsync())
                        {
                            int companyDocumentId = reader.GetInt32(0);

                            // Ensure the list is initialized
                            if (documentResponse.CompanyDocuments == null)
                            {
                                documentResponse.CompanyDocuments = new List<CompanyDocuments>();
                            }

                            // Ensure the list has enough capacity
                            while (documentResponse.CompanyDocuments.Count <= index)
                            {
                                documentResponse.CompanyDocuments.Add(new CompanyDocuments());
                            }

                            documentResponse.CompanyDocuments[index].CompanyDocumentId = companyDocumentId;
                            companyDocumentIds.Add(companyDocumentId);
                            index++;
                        }

                        return documentResponse;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<DocumentUploadModel> DocumentAsync(DocumentUploadModel studentDocuments)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                await connection.OpenAsync(); // Open the connection asynchronously

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Upsert documents and update DocumentId in listStudentDocument
                        studentDocuments.Documents = await UpsertDocumentAsync(studentDocuments.Documents, studentDocuments.ListStudentDocument[0].CompanyId, transaction, studentDocuments.ListStudentDocument[0].UniqueId);

                        // Update DocumentId in listStudentDocument using LINQ
                        Enumerable.Range(0, studentDocuments.ListStudentDocument.Count).ToList().ForEach(i => studentDocuments.ListStudentDocument[i].DocumentId = studentDocuments.Documents[i].DocumentId);

                        // Upsert student documents
                        studentDocuments.ListStudentDocument = await UpsertStudentDocuments(studentDocuments.ListStudentDocument, transaction);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error upserting documents. Transaction rolled back.", ex);
                    }
                    finally
                    {
                        connection.Close(); // Close the connection
                    }
                }
            }

            // Return the updated studentDocuments object
            return studentDocuments;
        }
        public async Task<IList<StudentDocument>> UpsertStudentDocuments(IList<StudentDocument> liststudentDocuments, SqlTransaction sqlTransaction)
        {
            try
            {
                using (SqlCommand command = new SqlCommand("dbo.UpsertStudentDocuments", sqlTransaction.Connection, sqlTransaction))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Create a DataTable using objDataTables.StudentDocumentDataTable()
                    DataTable studentDocumentDataTable = objDataTables.StudentDocumentDataTable(); // Ensure this method creates the DataTable correctly

                    // Populate the DataTable with the list of StudentDocument objects
                    foreach (var studentDocument in liststudentDocuments)
                    {
                        studentDocumentDataTable.Rows.Add(studentDocument.StudentDocumentId, studentDocument.DocumentId, studentDocument.StudentId);
                    }

                    // Add the table-valued parameter
                    SqlParameter tvpParam = command.Parameters.AddWithValue("@StudentDocuments", studentDocumentDataTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.StudentDocumentTableType";

                    // Execute the stored procedure asynchronously
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // Read the success message from the result set
                        string successMessage = null;
                        while (reader.Read())
                        {
                            if (reader["SuccessMessage"] != DBNull.Value)
                            {
                                successMessage = reader["SuccessMessage"].ToString();
                            }
                        }

                        // Process the inserted/updated records
                        List<StudentDocument> updatedDocuments = new List<StudentDocument>();
                        do
                        {
                            while (reader.Read())
                            {
                                //// Read the StudentDocumentId, DocumentId, and StudentId from the reader
                                //int studentDocumentId = Convert.ToInt32(reader["StudentDocumentId"]);
                                //int documentId = Convert.ToInt32(reader["DocumentId"]);
                                //int studentId = Convert.ToInt32(reader["StudentId"]);

                                // Create a new StudentDocument object with the retrieved values
                                updatedDocuments.Add(new StudentDocument
                                {
                                    StudentDocumentId = Convert.ToInt32(reader["StudentDocumentId"]),
                                    DocumentId = Convert.ToInt32(reader["DocumentId"]),
                                    StudentId = Convert.ToInt32(reader["StudentId"]),
                                Response = successMessage // Assign the success message to Response property
                                });
                            }
                        } while (reader.NextResult());

                        // Update liststudentDocuments with the retrieved StudentDocumentIds and Response
                        liststudentDocuments = liststudentDocuments.Zip(updatedDocuments, (original, updated) =>
                        {
                            original.StudentDocumentId = updated.StudentDocumentId;
                            original.Response = successMessage;
                            return original;
                        }).ToList();

                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error and rethrow
                _logger.LogError(ex.Message);
                throw;
            }

            return liststudentDocuments;
        }

    }
}

