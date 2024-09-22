using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Flyurdreamcommands.Repositories.Concrete;
using Flyurdreamcommands.Constants;
using Microsoft.IdentityModel.Tokens;

namespace Flyurdreamapi.Controllers
{

    [Route("api")]
    [ApiController]
    [EnableCors("cors")]
    public class PartnerController : ControllerBase
    {
        public readonly IPartnerRepository _PartnerRepository;
        public PartnerController(IPartnerRepository partnerRepository)
        {
            _PartnerRepository = partnerRepository;
        }

      

        [HttpPost("/UpsertCompany")]
        public async Task<ActionResult<Agent>> UpsertCompany(Agent companyDetails)
        {
            if (companyDetails == null)
            {
                return BadRequest("Data object is null");
            }

            try
            {
                Agent response = await _PartnerRepository.UpsertCompanyDetailsAsync(companyDetails);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                 return StatusCode(500, Const.InternalServerError);
            }
        }
        [HttpPost("/UpsertCompanyPrimaryUser")]
        public async Task<ActionResult<CompanyDetails>> UpsertCompanyPrimaryUser([FromBody] CompanyDetails companyDetails)
        {
            if (companyDetails == null)
            {
                return BadRequest("Data object is null");
            }

            try
            {
                CompanyDetails response = await _PartnerRepository.UpsertPrimaryUserAgent(companyDetails);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return StatusCode(500, Const.InternalServerError);
            }
        }
        //[HttpPost("/UpsertAgnetDetails")]
        //public async Task<ActionResult<DocumentReponse>> UpsertAgnetDetails([FromBody] DocumentReponse documentReponse)
        //{
        //    if (documentReponse == null)
        //    {
        //        return BadRequest("Data object is null");
        //    }

        //    try
        //    {
        //        DocumentReponse response = await _PartnerRepository.UpsertAgentDetails(documentReponse);
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception (ex) as needed
        //        return StatusCode(500, "Internal server error");
        //    }
        //}
        [HttpPost("/UpsertAgentDetails")]
        public async Task<ActionResult<DocumentReponse>> UpsertAgentDetails([FromBody] DocumentReponse questionReponse)
        {
            if (questionReponse == null)
            {
                return BadRequest("Data object is null");
            }

            try
            {
                DocumentReponse response = await _PartnerRepository.UpsertResponseDetails(questionReponse);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return StatusCode(500, Const.InternalServerError);
            }
        }

        [HttpPost("/UpsertLogo")]
        public async Task<ActionResult<string>> UpsertLogo(IFormFile logo, int companyId)
        {
            if (logo == null|| logo.ContentType == null)
            {
                return BadRequest("upload logo");
            }

            try
            {
                string filename = logo.FileName;
                using var memoryStream = new MemoryStream();
                await logo.CopyToAsync(memoryStream);
                var content = memoryStream.ToArray();
                var response = await _PartnerRepository.UpsertCompanyLogo(content, filename, companyId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return StatusCode(500, Const.InternalServerError);
            }
        }
        [HttpGet("GetCompanyLogo")]
        public async Task<ActionResult> GetCompanyLogo(int companyId)
        {
            if (companyId <= 0)
            {
                return BadRequest("Invalid Company ID");
            }
            try
            {
                var logoBase64 = await _PartnerRepository.GetCompanyLogoAsync(companyId);

                if (logoBase64 == Const.Logo_Not_Uploaded)
                {
                    return BadRequest(Const.Logo_Not_Uploaded);
                }
                if (string.IsNullOrEmpty(logoBase64))
                {
                    return NotFound(Const.Logo_Not_Uploaded);
                }
                // Convert base64 string to byte[]
                byte[] imageBytes = Convert.FromBase64String(logoBase64);
                // Return the byte[] as a FileStreamResult (or any suitable format)
                return File(imageBytes, Const.ImagePng); //Image/png
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return StatusCode(500, Const.InternalServerError);
            }
        }      


   
    }
}
