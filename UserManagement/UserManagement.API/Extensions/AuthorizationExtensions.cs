using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using UserManagement.Domain.Enums;

namespace UserManagement.API.Extensions
{
    public static class AuthorizationExtensions
    {
        public static void AddCustomAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireClaim("Role", UserRole.Admin.ToString()));

                options.AddPolicy("UserOrAdmin", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == "Role" &&
                            (c.Value == UserRole.User.ToString() ||
                             c.Value == UserRole.Admin.ToString()))));
            });
        }
    }
}