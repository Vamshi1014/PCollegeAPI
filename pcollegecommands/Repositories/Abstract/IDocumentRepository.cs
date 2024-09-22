using Flyurdreamcommands.Models.Datafields;
using Microsoft.Data.SqlClient;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IDocumentRepository
    {
        Task<DocumentReponse> UpsertDocumentsAsync(DocumentReponse documentReponse, SqlTransaction transaction);
        Task<DocumentReponse> InsertCompanyDocumentsAsync(DocumentReponse documentReponse, SqlTransaction transaction);
        Task<IList<Document>> UpsertDocumentAsync(IList<Document> documentResponse, int companyId,  SqlTransaction transaction, string? id = null);
        Task<DocumentUploadModel> DocumentAsync(DocumentUploadModel studentDocuments);
        Task<IList<StudentDocument>> UpsertStudentDocuments(IList<StudentDocument> liststudentDocuments, SqlTransaction sqlTransaction);
    }
}
                                                                                                                                                                                                      