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
    //[Auth]

    public class ProgramController : ControllerBase
    {
        public readonly IProgramRepository _programRepository;
        public ProgramController( IProgramRepository iProgramRepository)
        {
            _programRepository = iProgramRepository;
        }


        [HttpGet("/GetPrograms")]
        public async Task<ActionResult<List<ProgramMaster>>> GetPrograms(string? programName=null)
        {
            List<ProgramMaster> programMaster = await _programRepository.GetPrograms(programName);

            if (programMaster == null || programMaster.Count == 0)
            {
                return NotFound();
            }

            return Ok(programMaster);
        }
        [HttpGet("/GetAcademicLevels")]
        public async Task<ActionResult<List<AcademicLevel>>> GetAcademicLevels(string? levelName = null)
        {
            List<AcademicLevel> academicLevel = await _programRepository.GetAcademicLevels(levelName);

            if (academicLevel == null || academicLevel.Count == 0)
            {
                return NotFound();
            }

            return Ok(academicLevel);
        }
    }
}
