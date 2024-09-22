using Flyurdreamapi.Authorize;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Flyurdreamcommands.Repositories.Concrete;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Flyurdreamapi.Controllers
{

    [Route("api")]
    [ApiController]
    [EnableCors("cors")]
    public class CommonController : ControllerBase
    {
        public readonly ICommonRepository CommonRepository;
        public CommonController(ICommonRepository commonRepository)
        {
            CommonRepository = commonRepository;
        }
        [HttpGet("/GetCountries")]
        public ActionResult<List<Country>> GetCountries(string? searchKeyword)
        {
            List<Country> countries = CommonRepository.GetCountries(searchKeyword);
            return countries;
        }
        [HttpPost("/GetStates")]
        public ActionResult<List<State>> GetStates(int countryId, string? searchKeyword)
        {
            List<State> state = CommonRepository.GetStates(countryId,searchKeyword);
            return state;
        }

        [HttpGet("/GetCities")]
        public ActionResult<List<City>> GetCities(int stateId, string? searchKeyword)
        {
            List<City> city = CommonRepository.GetCity(stateId, searchKeyword);
            return city;
        }
       
        [HttpGet("/GetTypeRecords")]
        public async Task<ActionResult<List<Types>>> GetTypeRecords(string? description = null, string? typeFor = null)
        {
            try
            {
                List<Types> types = await CommonRepository.GetTypeRecords(description, typeFor);
                return Ok(types);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
