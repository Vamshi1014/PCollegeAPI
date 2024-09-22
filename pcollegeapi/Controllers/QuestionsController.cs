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
    public class QuestionsController : ControllerBase
    {
        public readonly IQuestionsRepository QuestionRepository;
        public QuestionsController(IQuestionsRepository questionRepository)
        {
            QuestionRepository = questionRepository;
        }
        [HttpGet("/GetQuestions")]
        public async Task<ActionResult<List<Questions>>> GetQuestionBasedonCompanyId(int companyId, int formId)
        {
            List<Questions> questions = await QuestionRepository.GetQuestionsFromDatabase(companyId, formId);

            if (questions == null || questions.Count == 0)
            {
                return NotFound();
            }

            return Ok(questions);
        }

    }
}
