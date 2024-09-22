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
  //  [Auth]
    public class StudentController : ControllerBase
    {
        public readonly IStudentRepository StudentRepository; public readonly IReferenceRepository ReferenceRepository;
        public StudentController(IStudentRepository studentRepository,IReferenceRepository referenceRepository)
        {
            StudentRepository = studentRepository;
            ReferenceRepository = referenceRepository;

        }
        [HttpPost("/upsertStudent")]
        public async Task<ActionResult<Student>> UpsertStudent([FromBody] Student student, int? companyId=0, int? branchId=0)
        {
            if (student == null)
            {
                return BadRequest("Student data is null.");
            }

            Student message = await StudentRepository.UpdateStudentndAddress(student, companyId,branchId);

            return Ok(new { Message = message });
        }

        [HttpGet("/GetStudentBy/{id}")]      
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await StudentRepository.GetStudentByIdAsync(id);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            return Ok(student);
        }
        [HttpPost("/educationdata")]
        public async Task<ActionResult<List<EducationData>>> PostEducationData([FromBody] List<EducationData> listEducationData)
        {
            if (listEducationData == null || listEducationData.Count == 0)
            {
                return BadRequest("Education data is null or empty.");
            }

            try
            {
                List<EducationData> updatedEducationData = await StudentRepository.UpsertEducationAsync(listEducationData);

                // If needed, you can check if any records were updated
                // Example: if (updatedEducationData.Count == 0) { /* Handle no updates */ }
                return Ok(new { updatedEducationData });
                // return Ok(new { Message = "Education data upserted successfully.", Data = updatedEducationData });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error upserting education data: {ex.Message}");
            }
        }

        [HttpPost("/emergencycontactdata")]
        public async Task<ActionResult<List<EmergencyContact>>> PostEmergencyContactData([FromBody] List<EmergencyContact> listEmergencyContact)
        {
            if (listEmergencyContact == null || listEmergencyContact.Count == 0)
            {
                return BadRequest("Emergency data is null or empty.");
            }

            try
            {
                List<EmergencyContact> updatedEmergencyContact = await ReferenceRepository.UpsertEmergencyContactsAsync(listEmergencyContact);

                // If needed, you can check if any records were updated
                // Example: if (updatedEmergencyData.Count == 0) { /* Handle no updates */ }
                return Ok(new { updatedEmergencyContact });
                // return Ok(new { Message = "Emeregency data upserted successfully.", Data = updatedEmergencyContact });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error upserting emergency data: {ex.Message}");
            }
        }
        [HttpPost("/VisaRefusalData")]
        public async Task<ActionResult<List<VisaRefusal>>> VisaRefusalData([FromBody] List<VisaRefusal> listVisaRefusal)
        {
            if (listVisaRefusal == null || listVisaRefusal.Count == 0)
            {
                return BadRequest("VisaRefusal data is null or empty.");
            }

            try
            {
                List<VisaRefusal> updatedVisaRefusal = await StudentRepository.UpsertVisaRefusalsAsync(listVisaRefusal);

                // If needed, you can check if any records were updated
                // Example: if (updatedVisaRefusal.Count == 0) { /* Handle no updates */ }
                return Ok(new { updatedVisaRefusal });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error upserting Visarefusal data: {ex.Message}");
            }
        }
        [HttpPost("/WorkExperienceData")]
        public async Task<ActionResult<List<WorkExperience>>> WorkExperienceData([FromBody] List<WorkExperience> listWorkExperience)
        {
            if (listWorkExperience == null || listWorkExperience.Count == 0)
            {
                return BadRequest("WorkExperience data is null or empty.");
            }

            try
            {
                List<WorkExperience> updatedWorkExperience = await StudentRepository.UpsertWorkExperienceAsync(listWorkExperience);

                // If needed, you can check if any records were updated UpsertWorkExperienceAsync
                // Example: if (updatedWorkExperience.Count == 0) { /* Handle no updates */ }

                return Ok(new { updatedWorkExperience });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error upserting WorkExperience data: {ex.Message}");
            }
        }
        [HttpPost("/EnglishExamAchieved")]
        public async Task<ActionResult<List<EnglishExam>>> EnglishExamAchieved([FromBody] List<EnglishExam> listEnglishExamAchieved)
        {
            if (listEnglishExamAchieved == null || listEnglishExamAchieved.Count == 0)
            {
                return BadRequest("WorkExperience data is null or empty.");
            }

            try
            {
                List<EnglishExam> englishExamAchieved = await StudentRepository.UpsertEnglishExamscoreAchieved(listEnglishExamAchieved);

                // If needed, you can check if any records were updated UpsertWorkExperienceAsync
                // Example: if (updatedWorkExperience.Count == 0) { /* Handle no updates */ }

                return Ok(new { englishExamAchieved });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error upserting WorkExperience data: {ex.Message}");
            }
        }
        [HttpGet("/StudentDetails")]
        public async Task<ActionResult<StudentDetails>> GetStudentDetailsAsyncByUniqueId(string UniqueId)
        {
            if (string.IsNullOrEmpty(UniqueId))
            {
                return BadRequest("UniqueId is null or empty.");
            }

            try
            {
                StudentDetails objStudentDetails = await StudentRepository.GetStudentDetailsAsyncByStudentId(UniqueId);
                return Ok(new { objStudentDetails });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error Fetching Student data: {ex.Message}");
            }
        }

        [HttpPost("/UpsertApplication")]
        public async Task<IActionResult> UpsertApplicationAsync([FromBody] Application applicationData)
        {
            if (applicationData == null)
            {
                return BadRequest("Invalid application data.");
            }

            try
            {
                var successMessage = await StudentRepository.UpsertApplicationDataAsync(applicationData);
                return Ok(new { Message = successMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves application data based on the Unique_ID.
        /// </summary>
        /// <param name="uniqueId">The unique identifier for the application.</param>
        /// <returns>A list of applications matching the Unique_ID.</returns>
        [HttpGet("/GetApplicationData")]
        public async Task<IActionResult> GetApplicationDataAsync(string? uniqueId, string? passportNo, string? studentUniqueId, string? assignedUsers,
     int? createdBy, string? emailId, int? status)
        {
            try
            {
                var applications = await StudentRepository.GetApplicationDataByUniqueIDAsync(uniqueId, passportNo, studentUniqueId, assignedUsers, createdBy,emailId, status);

                if (applications == null || applications.Count == 0)
                {
                    return NotFound("No application data found for the given Unique_ID.");
                }

                return Ok(applications);
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., logging)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
