using Flyurdreamcommands.Helpers;
using Flyurdreamcommands.Repositories.Abstract;
using Flyurdreamcommands.Repositories.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Flyurdreamapi.Authorize
{
    public class AuthAttribute : TypeFilterAttribute
    {
        public AuthAttribute() : base(typeof(AuthorizeAction))
        {
        }
    }
    public class AuthorizeAction : IAsyncAuthorizationFilter
    {
        private readonly IPermissionRepository _permissionRepository;
        //public readonly IUserRepository _userRepository;
        public readonly IConfiguration _config;

        //
        //public AuthorizeAction(IPermissionRepository permissionRepository, IUserRepository userRepository, IConfiguration config)
        //{
        //    _permissionRepository = permissionRepository;
        //    _userRepository = userRepository;
        //    _config = config;

        //}

        public AuthorizeAction(IPermissionRepository permissionRepository, IConfiguration config)
        {
            _permissionRepository = permissionRepository;
            _config = config;
        }
        //public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        //{
        //    var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        //    int uId = 0;

        //    if (string.IsNullOrEmpty(token))
        //    {
        //        context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
        //        return;
        //    }

        //    try
        //    {
        //        // Check if the access token is expired
        //        if (SecurityHelper.IsTokenExpired(token))
        //        {
        //            var refreshToken = context.HttpContext.Request.Headers["RefreshToken"].FirstOrDefault();

        //            // Validate the token and create a handler to read it
        //            SecurityHelper.ValidateToken(token);
        //            var handler = new JwtSecurityTokenHandler();
        //            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

        //            // Extract claims and set the User principal
        //            var claims = jwtToken.Claims.ToList();
        //            var identity = new ClaimsIdentity(claims, "jwt");
        //            var principal = new ClaimsPrincipal(identity);
        //            context.HttpContext.User = principal;

        //            var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

        //            if (userIdClaim == null)
        //            {
        //                context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
        //                return;
        //            }

        //            string userId = userIdClaim.Value;

        //            if (string.IsNullOrEmpty(refreshToken))
        //            {
        //                context.Result = new JsonResult("Refresh token required") { StatusCode = 403 };
        //                return;
        //            }
        //            uId = Convert.ToInt32(userId);
        //            // Validate the refresh token
        //            var storedRefreshToken = await _userRepository.GetStoredRefreshTokenAsync(uId);
        //            if (!SecurityHelper.ValidateRefreshToken(refreshToken, storedRefreshToken))
        //            {
        //                context.Result = new JsonResult("Invalid refresh token") { StatusCode = 403 };
        //                return;
        //            }

        //            // Generate a new access token
        //            var userInfo = await _userRepository.GetUserDetailsAsync(uId);
        //            var newAccessToken = SecurityHelper.GenerateJSONWebToken(userInfo, _config, out var newRefreshToken);

        //            // Return the new tokens in the response headers
        //            context.HttpContext.Response.Headers.Add("AccessToken", newAccessToken);
        //            context.HttpContext.Response.Headers.Add("RefreshToken", newRefreshToken);
        //        }


        //        var endpoint = context.HttpContext.GetEndpoint();
        //        var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        //        if (descriptor == null)
        //        {
        //            context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
        //            return;
        //        }

        //        string methodName = $"{descriptor.ControllerName}Controller.{descriptor.ActionName}";

        //        bool hasPermission = await _permissionRepository.UserHasPermissionAsync(uId, methodName);

        //        if (!hasPermission)
        //        {
        //            context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
        //            return;
        //        }
        //    }
        //    catch (SecurityTokenException ex)
        //    {
        //        context.Result = new JsonResult("Invalid token!") { StatusCode = 403 };
        //        return;
        //    }
        //    catch (Exception ex)
        //    {
        //        context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
        //        return;
        //    }
        //}
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
                return;
            }

            try
            {
                // Validate the token and get the claims principal
                var principal = SecurityHelper.ValidateToken(token, _config);

                // Set the User principal
                context.HttpContext.User = principal;

                // Retrieve the user ID from the claims
                var userIdClaim = principal.Claims.LastOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
                    return;
                }

                string userId = userIdClaim.Value;
                context.HttpContext.Items["UserId"] = Convert.ToInt32(userId);

                // Get the controller action descriptor to identify the current method
                var endpoint = context.HttpContext.GetEndpoint();
                var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

                if (descriptor == null)
                {
                    context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
                    return;
                }

                string methodName = $"{descriptor.ControllerName}Controller.{descriptor.ActionName}";

                // Check if the user has permission to access the method
                bool hasPermission = await _permissionRepository.UserHasPermissionAsync(int.Parse(userId), methodName);

                if (!hasPermission)
                {
                    context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
                    return;
                }
            }
            catch (SecurityTokenException ex)
            {
                // Handle token validation errors
                context.Result = new JsonResult(ex.Message) { StatusCode = 403 };
                return;
            }
            catch (Exception ex)
            {
                // Handle any other errors
                context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
                return;
            }
        }

        //public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        //{
        //    var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        //    if (string.IsNullOrEmpty(token))
        //    {
        //        context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
        //        return;
        //    }

        //    SecurityHelper.ValidateToken(token);

        //    var handler = new JwtSecurityTokenHandler();
        //    JwtSecurityToken jwtToken = null;

        //    try
        //    {
        //        jwtToken = handler.ReadJwtToken(token);
        //    }
        //    catch
        //    {
        //        context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
        //        return;
        //    }

        //    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

        //    if (userIdClaim == null)
        //    {
        //        context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
        //        return;
        //    }

        //    string userId = userIdClaim.Value;

        //    var endpoint = context.HttpContext.GetEndpoint();
        //    var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        //    if (descriptor == null)
        //    {
        //        context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
        //        return;
        //    }

        //    string methodName = $"{descriptor.ControllerName+ "Controller"}.{descriptor.ActionName}";

        //    // Fetch permissions from database using IPermissionRepository
        //    bool hasPermission = await _permissionRepository.UserHasPermissionAsync(int.Parse(userId), methodName);

        //    if (!hasPermission)
        //    {
        //        context.Result = new JsonResult("Permission denied!") { StatusCode = 403 };
        //        return;
        //    }
        //}
    }


}
