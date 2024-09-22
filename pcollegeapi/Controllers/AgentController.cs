using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Flyurdreamcommands.Repositories.Concrete;
using Flyurdreamapi.Authorize;

namespace Flyurdreamapi.Controllers
{

    [Route("api")]
    [ApiController]
    [EnableCors("cors")]
    
    public class AgentController : ControllerBase
    {
        public readonly IAgentRepository AgentRepository;
        public AgentController(IAgentRepository agentRepository)
        {
            AgentRepository = agentRepository;
        }
        [HttpPost("/GetResponses")]
        public ActionResult<IList<Responses>> GetResponses(List<Responses> responses)
        {
            List<Responses> response = AgentRepository.BulkUpsertResponses(responses);
            return response;
        }
    }
}
