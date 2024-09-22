using Flyurdreamapi.Authorize;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Flyurdreamapi.Controllers
{
    [Route("api")]
    [ApiController]
    [EnableCors("cors")]
    public class DocumentController : ControllerBase
    {
        public readonly IDocumentRepository DocumentRepository;
        public DocumentController(IDocumentRepository documentRepository)
        {
            DocumentRepository = documentRepository;
        }

        [HttpPost("/UploadDocuments")]
      //  [Auth]
        public async Task<ActionResult<DocumentUploadModel>> UploadStudentDocuments(DocumentUploadModel studentDocuments)
        {
            try
            {
                DocumentUploadModel uploadedDocuments = await DocumentRepository.DocumentAsync(studentDocuments);
                return Ok(uploadedDocuments);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error uploading documents: {ex.Message}");
            }
        }
        [HttpPost("/GetStudentDocuments")]
       // [Auth]
        public async Task<ActionResult<DocumentUploadModel>> GetStudentDocuments(DocumentUploadModel studentDocuments)
        {
            try
            {
                DocumentUploadModel uploadedDocuments = await DocumentRepository.DocumentAsync(studentDocuments);
                return Ok(uploadedDocuments);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error Fetching documents: {ex.Message}");
            }
        }

    }

}
