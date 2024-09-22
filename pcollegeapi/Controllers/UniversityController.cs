using System.Data;
using System.Security.Claims;
using Flyurdreamapi.Authorize;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Flyurdreamcommands.Repositories.Concrete;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Flyurdreamapi.Controllers
{
    [Route("api")]
    [ApiController]
    [EnableCors("cors")]
    public class UniversityController : ControllerBase
    {
        private readonly ExcelToDataTable _excelToDataTable;
        public readonly IInsertMasterData IInsertMasterData;
        public readonly IProgramRepository _programRepository;
        public readonly IUniversityRepository _universityRepository;
        private readonly IMemoryCache _cache; // Add this line
        private readonly CachingSettings _cachingSettings;
        public UniversityController(IInsertMasterData iInsertMasterData, ExcelToDataTable excelToDataTable, IProgramRepository iProgramRepository
            ,IUniversityRepository universityRepository, IMemoryCache cache, IOptions<CachingSettings> cachingSettings)
        {
            IInsertMasterData = iInsertMasterData;
            _excelToDataTable = excelToDataTable;
            _programRepository = iProgramRepository;
            _universityRepository  = universityRepository;
            _cache = cache; // Initialize the cache
               _cachingSettings = cachingSettings.Value;
        }
        [Auth]
        [HttpPost("/bulkInsert")]
        public async Task<IActionResult> BulkInsert(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

                // Ensure the directory exists
                Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, file.FileName);

                // Create the file stream and copy the file content
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                DataTable dataTable = _excelToDataTable.ReadExcel(filePath);

                // Adjust the batch size as needed
                int batchSize = 5000; // Example batch size

                // Call the BulkInsert method with the data table and batch size
                string response = await IInsertMasterData.BulkInsert(dataTable, batchSize);

                return Ok(response);
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"Directory not found: {ex.Message}");
                // Handle the specific case where the directory is not found
                return StatusCode(StatusCodes.Status500InternalServerError, "Directory not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Handle other potential exceptions
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during file processing.");
            }
        }

        [Auth]
        [HttpGet("/GetUniversityPrograms")]
        public async Task<IActionResult> GetUniversityProgramsAsync(string? universityName = null, string? programName = null,
  string? academicLevel = null, int countryId = 0, int pageNumber = 1, int pageSize = 50)
        {
            // Assuming token validation or user ID is necessary for authorization
            var claimsIdentity = User.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                // Extract the claims from the ClaimsIdentity
                var claims = claimsIdentity.Claims;

                // Extract specific claims, e.g., NameIdentifier and Name
                var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var userNameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var userEmail = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                // Store these in variables or use them as needed
                string userId = userIdClaim;   // User ID extracted from the claims
                string userName = userNameClaim; // User Name extracted from the claims

                // If you want to extract more claims, just follow the same pattern
                // Example: Extracting a custom claim
                var customClaim = claims.FirstOrDefault(c => c.Type == "CustomClaimType")?.Value;

                // Example, use the relevant user identifier
                var cacheKey = $"UniversityPrograms-{userId}-{universityName}-{programName}-{academicLevel}-{countryId}-{pageNumber}-{pageSize}";

                if (!_cache.TryGetValue(cacheKey, out var cachedResult))
                {
                    // Perform authorization checks here if needed
                    if (!User.Identity.IsAuthenticated)
                    {
                        return Unauthorized();
                    }

                    var result = await _programRepository.GetUniversityProgramsAsync(universityName, programName, academicLevel, countryId, pageNumber, pageSize);

                    if (result.Programs == null || result.Programs.Count == 0)
                    {
                        return Ok("No Data");
                    }

                    cachedResult = new
                    {
                        TotalCount = result.TotalCount,
                        Programs = result.Programs
                    };

                    // Set cache options
                    var cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cachingSettings.AbsoluteExpirationMinutes),
                        SlidingExpiration = TimeSpan.FromMinutes(_cachingSettings.SlidingExpirationMinutes)
                    };
                    _cache.Set(cacheKey, cachedResult, cacheEntryOptions);
                }

                return Ok(cachedResult);

            }
            else
            {
                // Handle the case where User.Identity is null or not a ClaimsIdentity
                // This might happen if the user is not authenticated or there's no valid identity
                // Handle this according to your application's needs
                throw new InvalidOperationException("User.Identity is not a ClaimsIdentity.");
            }
         
        }
        [Auth]
        [HttpGet("/GetUniversities")]
        public async Task<ActionResult<List<UniversityMaster>>> GetUniversities(string? universityName)
        {
            List<UniversityMaster> universityMaster = await _universityRepository.GetUniversities(universityName);

            if (universityMaster == null || universityMaster.Count == 0)
            {
                return NotFound();
            }

            return Ok(universityMaster);
        }
        [Auth]
        [HttpGet]
        [Route("/GetEntryRequirementsByCountryAndUniversity")]
        public async Task<IActionResult> GetEntryRequirementsByCountryAndUniversity(
            int universityCountry,  int universityID, int academicCategoryId)
        {
            if (universityCountry==0)
            {
                return BadRequest("UniversityCountry is required.");
            }

            if (universityID <= 0)
            {
                return BadRequest("UniversityID must be a positive integer.");
            }

            try
            {
                List<EntryRequirement> entryRequirements = await _universityRepository.GetEntryRequirementsByCountryAndUniversityAsync(universityCountry, universityID, academicCategoryId);

                if (entryRequirements == null || entryRequirements.Count == 0)
                {
                    return Ok("No entry requirements found for the specified parameters.");
                }

                return Ok(entryRequirements);
            }
            catch (Exception ex)
            {
                // Log the exception
                // e.g., _logger.LogError(ex, "An error occurred while fetching entry requirements.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [Auth]
        [HttpPost("/UpsertEntryRequirement")]
        public async Task<IActionResult> UpsertEntryRequirement(EntryRequirement entryRequirement)
        {
            if (entryRequirement == null)
            {
                return BadRequest("Invalid data.");
            }
            var id = Convert.ToInt32(HttpContext.Items["UserId"]);
 
            if (id == 0 )
            {
                return Forbid("User ID not found.");
            }

            try
            {
                EntryRequirement updatedEntryRequirement = await _universityRepository.UpsertEntryRequirementAsync(entryRequirement, id);

                if (updatedEntryRequirement != null)
                {
                    return Ok(updatedEntryRequirement);
                }
                else
                {
                    return StatusCode(500, "Upsert failed.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [Auth]
        [HttpGet("/GetHigherSecondaryBoards")]
        public async Task<ActionResult<List<HigherSecondaryBoard>>> GetHigherSecondaryBoard(string? searchHSCName)
        {
            try
            {
                var boards = await _universityRepository.GetHigherSecondaryBoardsAsync(searchHSCName);

                if (boards == null || boards.Count == 0)
                {
                    return NotFound("No Higher Secondary Boards found.");
                }

                return Ok(boards);
            }
            catch (Exception ex)
            {
                // Log the exception (handled internally in your service)
                return StatusCode(500, "Internal server error.");
            }
        }

        [Auth]
        [HttpGet("/GetCountrySpecificUniversities")]
        public async Task<ActionResult<List<CountrySpecificUniversity>>> GetCountrySpecificUniversitiesAsync(string? searchHSCName)
        {
            try
            {
                var universities = await _universityRepository.GetCountrySpecificUniversitiesAsync(searchHSCName);

                if (universities == null || universities.Count == 0)
                {
                    return Ok("No Data");
                }

                return Ok(universities);
            }
            catch (Exception ex)
            {
                // Log the exception (handled internally in your service)
                return StatusCode(500, "Internal server error.");
            }
        }

        [Auth]
        [HttpPost("/UpsertUniversityEntryRequirement")]
        public async Task<IActionResult> UpsertUniversityEntryRequirementsAsync([FromBody] List<UniversityEntryRequirements> listUniversityEntryRequirementsData)
        {
            if (listUniversityEntryRequirementsData == null || listUniversityEntryRequirementsData.Count == 0)
            {
                return BadRequest("Invalid input data.");
            }

            // Check if any entry has Typeid as 0
            if (listUniversityEntryRequirementsData.Any(e => e.Typeid == 0))
            {
                return BadRequest("Typeid cannot be 0.");
            }

            try
            {
                List<UniversityEntryRequirements> result = await _universityRepository.UpsertUniversityEntryRequirementsAsync(listUniversityEntryRequirementsData);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // Constructor injection to get the repository

        [Auth]
        [HttpGet("/GetUniversityEntryRequirements")]
        public async Task<ActionResult<List<UniversityEntryRequirements>>> GetUniversityEntryRequirementsAsync(
            [FromQuery] int universityId,
            [FromQuery] int entryCountryId,
            [FromQuery] int universityCountryId,
            [FromQuery] int? typeId)
        {
            try
            {
                var results = await _universityRepository.GetUniversityEntryRequirementsAsync(
                    universityId,
                    entryCountryId,
                    universityCountryId,
                    typeId);

                return Ok(results);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Auth]
        [HttpPost("/InsertUniversityEnglishExamRequirements")]
        public async Task<IActionResult> InsertUniversityEnglishExamRequirements([FromBody] List<UniversityExamRequirement> requirements)     
        {
            if (requirements == null || !requirements.Any())
            {
                return BadRequest("No records provided.");
            }

            try
            {
                List<UniversityExamRequirement> inserteddata = await _universityRepository.InsertUniversityExamRequirements(requirements);
                return Ok(inserteddata);
            }
            catch (Exception ex)
            {
                // Handle the exception (logging, etc.)
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        /// <summary>
        /// Gets the university English exam requirements based on the provided filters.
        /// </summary>
        /// <param name="universityCountryId">The ID of the university country.</param>
        /// <param name="academicCategoryId">The ID of the academic category.</param>
        /// <param name="universityId">The ID of the university.</param>
        /// <returns>A list of university exam requirements.</returns>
        [Auth]
        [HttpGet("/GetUniversityEnglishExamRequirements")]
        public async Task<ActionResult<List<UniversityExamRequirement>>> GetUniversityEnglishExamRequirements(
            [FromQuery] int universityCountryId = 0,
            [FromQuery] int academicCategoryId = 0,
            [FromQuery] int universityId = 0)
        {
            try
            {
                var requirements = await _universityRepository.GetUniversityEnglishExamRequirementsAsync(universityCountryId, academicCategoryId, universityId);

                if (requirements == null || requirements.Count == 0)
                {
                    return Ok("No Data");
                }

                return Ok(requirements);
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here for brevity)
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}



