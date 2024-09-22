using Flyurdreamapi.Authorize;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Flyurdreamcommands.Repositories.Concrete;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Flyurdreamapi.Controllers
{
    [Route("api")]
    [ApiController]
    [EnableCors("cors")]
    //[Auth]
    public class EnquiryController : ControllerBase
    {
        public readonly IEnquiryRepository EnquiryRepository;
        private readonly IMemoryCache _cache; // Add this line
        private readonly CachingSettings _cachingSettings;
        public EnquiryController(IEnquiryRepository enquiryRepository, IMemoryCache cache, IOptions<CachingSettings> cachingSettings)
        {
            EnquiryRepository = enquiryRepository;
            _cache = cache; // Initialize the cache
            _cachingSettings = cachingSettings.Value;
        }

        [HttpGet("/GetEnquiries")]
        public async Task<IActionResult> GetEnquiriesByUser(int userId, string? companyId, string? branchId, int? countryIntrested,
     string? passportnumber, string? uniqueId)
        {
            string statusMessage;
            IEnumerable<Enquiry> enquiries; // Assuming Enquiry is a type that represents the enquiry data

            // Assuming GetEnquiries returns a tuple of (string, IEnumerable<Enquiry>)
            (statusMessage, enquiries) = await EnquiryRepository.GetEnquiries(userId, companyId, branchId, countryIntrested, passportnumber, uniqueId);

            if (enquiries == null || !enquiries.Any())
            {
                return NotFound(statusMessage); // You may want to return the status message if no enquiries are found
            }

            return Ok(enquiries);
        }
        [HttpPost("/RefreshCache")]
        public IActionResult RefreshCache(int userId, string? companyId, string? branchId, int? countryIntrested,
            string? passportnumber, string? uniqueId)
        {
            string cacheKeyPrefix = $"UniversityPrograms-{userId}-{companyId}-{branchId}-{countryIntrested}-{passportnumber}-{uniqueId}";
            // Use CacheService to remove entries
            _cache.Remove(cacheKeyPrefix);

            return Ok("Cache refreshed");
        }
        [HttpPost]
        [Route("/InsertEnquiry")]
        public async Task<Enquiry> InsertEnquiry(int userid, Enquiry objenquiry, int? companyId, int? branchId )
        {
            Enquiry result = await EnquiryRepository.InsertEnquiryAsync(userid,objenquiry,companyId, branchId);
            return result;
        }

        [HttpGet("/GetEnquiriesCompanyBranchByUser")]
        public async Task<IActionResult> GetEnquiriesCompanyBranchByUser(int userId, string? companyId = null, string? branchId = null)
        {
            // Create a unique cache key based on the query parameters
            string cacheKey = $"EnquiriesCompanyBranch-{userId}-{companyId}-{branchId}";

            // Try to get the cached data
            if (!_cache.TryGetValue(cacheKey, out List<Enquiry>? cachedEnquiries))
            {
                // Cache not found, fetch data from the repository
                var (fetchedEnquiries, status) = await EnquiryRepository.GetEnquiriesCompanyBranchByUserAsync(userId, companyId, branchId);

                if (!string.IsNullOrEmpty(status))
                {
                    return BadRequest(new { status });
                }

                // Set cache options
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10), // Cache for 10 minutes
                    SlidingExpiration = TimeSpan.FromMinutes(5) // Sliding expiration of 5 minutes
                };

                // Save data in the cache
                _cache.Set(cacheKey, fetchedEnquiries, cacheEntryOptions);

                // Return the data
                return Ok(fetchedEnquiries);
            }

            // Return cached data
            return Ok(cachedEnquiries);
        }

    }
}
