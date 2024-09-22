using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace Flyurdreamapi.RoleMiddleware
{
    //
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                if (!string.IsNullOrEmpty(role))
                {
                    context.Request.Headers["role"] = role;
                }
            }

            await _next(context);
        }
    }

    public static class RoleMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleMiddleware>();
        }
    }
}