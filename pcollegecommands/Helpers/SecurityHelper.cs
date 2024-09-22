using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Models.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Flyurdreamcommands.Helpers
{
    public static class SecurityHelper
    {
        private static readonly int SaltSize = 16; // Size of the salt in bytes
        private static readonly int RefreshTokenExpiryDays = 30;
        //public static string GenerateJSONWebToken(User userInfo, IConfiguration _config)
        //{
        //    try
        //    {
        //        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        //        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        //        // Calculate expiration time
        //        DateTime expiration = DateTime.UtcNow.AddMinutes(120);
        //        var claims = new[]
        //        {
        //          new Claim(JwtRegisteredClaimNames.Name, userInfo.FirstName),
        //    new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
        //    new Claim(ClaimTypes.Role, Enum.GetName(typeof(UserRole), userInfo.GroupId)), // Convert enum to role name // Standard claim type for role 
        //   new Claim(ClaimTypes.NameIdentifier, userInfo.UserId.ToString()), // Standard claim type for user ID
        //    new Claim("DateOfJoing", userInfo.CreatedAt.ToString("yyyy-MM-dd")),
        //    new Claim(JwtRegisteredClaimNames.Exp, ((DateTimeOffset)expiration).ToUnixTimeSeconds().ToString()), // Expiration claim
        //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //    };

        //        var token = new JwtSecurityToken(_config["Jwt:Issuer"],
        //            _config["Jwt:Audience"],
        //            claims,
        //            expires: expiration,
        //            signingCredentials: credentials);

        //        var tokenHandler = new JwtSecurityTokenHandler();
        //        string refreshToken = GenerateRefreshToken(); // Generate the refresh token



        //        return new JwtSecurityTokenHandler().WriteToken(token);
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}
        public static (string Token, string RefreshToken) GenerateJSONWebToken(User userInfo, IConfiguration config)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                // Calculate expiration time
                DateTime expiration = DateTime.UtcNow.AddMinutes(30);

                var claims = new[]
                {
       new Claim(JwtRegisteredClaimNames.Sub, userInfo.Email), // Sub claim is generally used for the user identifier
            new Claim(JwtRegisteredClaimNames.Name, userInfo.FirstName),
            new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
            new Claim(ClaimTypes.Role, Enum.GetName(typeof(UserRole), userInfo.GroupId)), // Convert enum to role name
            new Claim(ClaimTypes.NameIdentifier, userInfo.UserId.ToString()), // Set the NameIdentifier to the user's ID
            new Claim("DateOfJoining", userInfo.CreatedAt.ToString("yyyy-MM-dd")), // Custom claim
            new Claim(JwtRegisteredClaimNames.Exp, ((DateTimeOffset)expiration).ToUnixTimeSeconds().ToString()), // Expiration claim
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID claim
        };


                var token = new JwtSecurityToken(
                    config["Jwt:Issuer"],
                    config["Jwt:Audience"],
                    claims,
                    expires: expiration,
                    signingCredentials: credentials);

                var tokenHandler = new JwtSecurityTokenHandler();
                string jwtToken = tokenHandler.WriteToken(token); // Write JWT token to string

                string refreshToken = GenerateRefreshToken(); // Generate the refresh token

                return (jwtToken, refreshToken); // Return both JWT and refresh token
            }
            catch (Exception ex)
            {
                // Log exception if needed
                throw new ApplicationException("An error occurred while generating the JWT token.", ex);
            }
        }
        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        public static bool ValidateRefreshToken(string refreshToken, string storedRefreshToken)
        {
            return refreshToken == storedRefreshToken;
        }

        public static bool IsTokenExpired(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return true;

            return jwtToken.ValidTo < DateTime.UtcNow;
        }
      
            //create has password
            public static (string hashedPassword, byte[] salt) HashPassword(string password)
        {
            const int SaltSize = 16; // Choose an appropriate salt size

            // Generate a random salt
            byte[] salt;
            using (var rng = new RNGCryptoServiceProvider())
            {
                salt = new byte[SaltSize];
                rng.GetBytes(salt);
            }

            // Create the salted hash
            string hashedPassword;
            using (var sha256Hash = SHA256.Create())
            {
                // Combine the password and salt
                byte[] combinedBytes = new byte[password.Length + SaltSize];
                Buffer.BlockCopy(Encoding.UTF8.GetBytes(password), 0, combinedBytes, 0, password.Length);
                Buffer.BlockCopy(salt, 0, combinedBytes, password.Length, SaltSize);

                // Compute the hash
                byte[] hashBytes = sha256Hash.ComputeHash(combinedBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                hashedPassword = builder.ToString();
            }

            return (hashedPassword, salt);
        }
        public static ClaimsPrincipal ValidateToken(string token, IConfiguration config)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true, // Ensure the token is still valid
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero // No tolerance for expiration
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                throw new SecurityTokenException("Token has expired.");
            }
            catch (Exception)
            {
                throw new SecurityTokenException("Token validation failed.");
            }
        }
    

        //public static void ValidateToken(string token)
        //{
        //    var handler = new JwtSecurityTokenHandler();
        //    JwtSecurityToken jwtToken = null;

        //    try
        //    {
        //        jwtToken = handler.ReadJwtToken(token);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error reading JWT token: {ex.Message}");
        //        // Handle token read errors
        //        return;
        //    }

        //    // Example: Retrieving user ID claim
        //    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        //    if (userIdClaim != null)
        //    {
        //        var userId = userIdClaim.Value;
        //        // Process user ID
        //    }
        //    else
        //    {
        //        Console.WriteLine("User ID claim not found in token.");
        //        // Handle missing user ID claim
        //    }
        //}

        // Verify Password
        public static bool VerifyPassword(string enteredPassword, string storedHash, byte[] storedSalt)
        {
            // Compute the hash of the entered password with the stored salt
            byte[] enteredHashBytes;
            using (var sha256Hash = SHA256.Create())
            {
                // Combine the entered password and stored salt
                byte[] combinedBytes = new byte[enteredPassword.Length + storedSalt.Length];
                Buffer.BlockCopy(Encoding.UTF8.GetBytes(enteredPassword), 0, combinedBytes, 0, enteredPassword.Length);
                Buffer.BlockCopy(storedSalt, 0, combinedBytes, enteredPassword.Length, storedSalt.Length);

                // Compute the hash
                enteredHashBytes = sha256Hash.ComputeHash(combinedBytes);
            }

            // Convert the byte array to a hexadecimal string
            StringBuilder builder = new StringBuilder();
            foreach (byte b in enteredHashBytes)
            {
                builder.Append(b.ToString("x2"));
            }
            string enteredHash = builder.ToString();

            // Compare the computed hash with the stored hash
            return enteredHash == storedHash;
        }

        public static string GetRootDomain(HttpRequest request)
        {
            // Get the scheme (http or https)
            string scheme = request.Scheme;

            // Get the host (domain name)
            string host = request.Host.Value;

            // Construct and return the root domain URL
            return $"{scheme}://{host}";
        }
    }

}
