using System.Text.Json.Nodes;
using Flyurdreamapi.Authorize;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Flyurdreamcommands.Repositories.Concrete;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Flyurdreamapi.Controllers
{
    [Route("api")]
    [ApiController]
    [EnableCors("cors")]
    [Auth]
    public class VoipController : ControllerBase
    {
        public readonly IVoipRepository VoipRepository;
        public VoipController(IVoipRepository voipRepository)
        {     
            VoipRepository = voipRepository;
        }


        [HttpGet("/GetRequestYay")]
        public async Task<IActionResult> GetRequestYay()
        {
            // JsonObject jsonResponse = await VoipRepository.GetRequestYay();
            JsonObject jsonResponse = null;//
            await VoipRepository.GetRequestYay();
            if (jsonResponse != null)
            {
                return Ok(jsonResponse);
            }
            else
            {
                return StatusCode(500, "Failed to get a valid response from the API.");
            }
        }
        [HttpPost("/InitiateCall")]
        public async Task<IActionResult> InitiateCall()
        {
            try
            {
                //   var result = await VoipRepository.InitiateCallAsync();
                var result = "";
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
