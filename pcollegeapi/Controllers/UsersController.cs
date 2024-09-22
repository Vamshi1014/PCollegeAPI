using Azure;
using Flyurdreamapi.Authorize;
using Flyurdreamcommands.Constants;
using Flyurdreamcommands.Helpers;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Flyurdreamcommands.Repositories.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Flyurdreamapi.Controllers
{
    [Route("api")]
    [ApiController]
    [EnableCors("cors")]
    public class UsersController : ControllerBase
    {
        public readonly IUserRepository _userRepository;

        public readonly IEmailService _emailService;
        private readonly IConfiguration _config;


        public UsersController(IUserRepository userRepository, IEmailService emailService, IConfiguration config)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _config = config;
        }
        // GET: api/<UsersController>
        [HttpGet]
        [Route("/Getping")]
        public IEnumerable<string> Getping()
        {
            return new string[] { DateTime.Now.ToString(), "Current time" };
        }
        [Auth]
        [HttpGet]
        [Route("/GetDbsuccess")]       
        public async Task<string> GetDbsuccess()
        {
            var response = "";
            try
            {
                response = await _userRepository.GetDatabsucces();


            }
            catch (Exception ex)
            {
                throw ex;
            }

            return response;
        }
        // Post api/<UsersController>/5
        [HttpPost]
        [Route("GetLoggedInUser"), AllowAnonymous]
        public async Task<User> GetLoggedInUser(User User)
        {
            User response = new User();
            try
            {
               
                response = await _userRepository.GetLoggedInUser(User);


            }
            catch 
            {
                throw;

            }

            return response;
        }

        // POST api/<UsersController>

        [HttpPost]
        [Route("UserRegistration"), AllowAnonymous]
        public async Task<User> UserRegistration(User User)
        {
            User response = new User();
            HttpRequest request = HttpContext.Request;
            try
            {
                response = await _userRepository.UserRegistration(User, request);



            }
            catch (Exception ex)
            {
                return response;
            }

            return response;
        }

        // GET: api/UserVerification
        [HttpGet]
        [Route("emailVerification/{token}"), AllowAnonymous]
        public async Task<IActionResult> VerifyUser(string token)
        {
            string response = string.Empty;
            try
            {
                // Call the repository method to verify the user based on the token
                response = await _userRepository.VerifyEmail(token);
                if (response != null)
                {
                    if (response == Const.EmailVerifiedSuccess || response == Const.UserAlreadyVerified)
                    {
                        return Ok(response);
                    }
                    else if (response == Const.InvalidOrExpiredToken)
                    {
                        return BadRequest(response);
                    }
                    // Add more conditions as needed
                    else
                    {
                        return StatusCode(500, "Unexpected response from server");
                    }
                }
                else
                {
                    return StatusCode(500, "Invalid response from server");
                }
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                // ...

                return StatusCode(500, "Internal server error");
            }
        }
       // [Auth]
        [HttpPost]
        [Route("changepassword"), AllowAnonymous]    
        public async Task<string> ChangePassword(string email, string newPassword)
        {
            string response = string.Empty;
            try
            {
                response = await _userRepository.ChangePassword(email, newPassword);
                return response;
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                // ...

                throw;
            }
        }

        [HttpGet]
        [Route("Forgotpassword"), AllowAnonymous]
        public async Task<ActionResult<UserPasscode>> Forgotpassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required.");
            }

            try
            {
                UserPasscode objUserPasscode = await _userRepository.GenerateAndInsertOTP(email);
                if (objUserPasscode == null)
                {
                    return NotFound("User not found or failed to generate OTP.");
                }
                return Ok(objUserPasscode);
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                // ...

                // Return a generic error message to the client
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        // Post api/<OtpValidation>/
        [HttpPost]
        [Route("OtpValidation"), AllowAnonymous]
        public async Task<ActionResult<bool>> OtpValidation(string email, string passcode)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(passcode))
            {
                return BadRequest("Email and passcode are required.");
            }

            try
            {
                bool isValid = await _userRepository.ValidateOTP(email, passcode);
                if (isValid)
                {
                    return Ok(true); // Return true if the OTP is valid
                }
                else
                {
                    return Ok(false); // Return false if the OTP is invalid
                }
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                // ...

                // Return a generic error message to the client
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

      //  [Auth]
        [HttpPost("UpdateUserDetails")]
        public async Task<ActionResult<User>> UpdateUserDetails([FromBody] User user)
        {
            if (user.UserId == 0)
            {
                return BadRequest("User object is null");
            }

            try
            {
                User response = await _userRepository.UpdateUserAddress(user);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return StatusCode(500, Const.InternalServerError);
            }
        }

        [HttpGet("RefreshTokenAsync")]
        public async Task<ActionResult> RefreshTokenAsync(string refreshToken)
        {
            
            int userId = await _userRepository.GetUserIdFromRefreshTokenAsync(refreshToken); //SecurityHelper.GetUserIdFromTokenAsync(refreshToken); // Implement this method to extract userId from refreshToken
            if (userId == 0)
                
                {
                    return Unauthorized("Invalid user token.");
                }
            // Retrieve the stored refresh token from the database
            var storedRefreshToken = await _userRepository.GetStoredRefreshTokenAsync(userId);
         
            if (storedRefreshToken != refreshToken)
            {
                return Unauthorized("Invalid refresh token.");
            }

            // Retrieve user info
            var userInfo = await _userRepository.GetUserDetailsAsync(userId);
            if (userInfo == null)
            {
                return Unauthorized("User not found.");
            }

            // Generate new access token and refresh token
            var (newAccessToken, newRefreshToken) = SecurityHelper.GenerateJSONWebToken(userInfo, _config);
            if (!string.IsNullOrEmpty(newRefreshToken))
            {
                // Convert the string to bytes
                byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes(newRefreshToken);
                // Encode the bytes to a Base64 string
                newRefreshToken = Convert.ToBase64String(tokenBytes);
            }
            // Store the new refresh token
            await _userRepository.StoreRefreshTokenAsync(userId, newRefreshToken);

            // Return new tokens
            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        // GET api/user
      //  [Auth]
        [HttpGet("GetUserDetails")]
        public async Task<ActionResult<User>> GetUsers([FromQuery] int? userId = null)
        {
            try
            {
                // Retrieve user details from the repository
                var user = await _userRepository.GetUserDetailsAsync(userId);

                // Return 404 Not Found if no users are found
                if (user == null )
                {
                    return NotFound("No users found");
                }

                // Return the list of users
                return Ok(user);
            }
            catch (Exception ex)
            {
                // Log the exception and return a generic error message
                // In production, consider using a logging framework
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
