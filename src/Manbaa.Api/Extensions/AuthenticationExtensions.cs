using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.IdentityModel.Tokens;

using Manbaa.Api.Hubs;

using Manbaa.Application.Abstractions;

using Manbaa.Application.Common;

using Manbaa.Application.Settings;

namespace Manbaa.Api.Extensions;

public static class AuthenticationExtensions
{
    private const string TeacherNotActive = "The teacher account is not active.";

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,

                    ValidIssuer = jwtSettings.Issuer,

                    ValidateAudience = true,

                    ValidAudience = jwtSettings.Audience,

                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),

                    ValidateLifetime = true,

                    ClockSkew = TimeSpan.FromMinutes(1),

                    NameClaimType = AppClaimTypes.Name,

                    RoleClaimType = AppClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments(NotificationsHub.Route))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },

                    // A teacher whose subscription ended (after the grace days) or who was suspended is locked out
                    // together with all of the teacher's staff, students and parents.
                    OnTokenValidated = async context =>
                    {
                        if (!int.TryParse(context.Principal?.FindFirst(AppClaimTypes.TeacherId)?.Value, out var teacherId))
                        {
                            return;
                        }

                        var teacherAccess = context.HttpContext.RequestServices.GetRequiredService<ITeacherAccessService>();

                        if (!await teacherAccess.IsActiveAsync(teacherId, context.HttpContext.RequestAborted))
                        {
                            context.Fail(TeacherNotActive);
                        }
                    },

                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        var message = context.AuthenticateFailure?.Message == TeacherNotActive ? ITeacherAccessService.NotActiveMessage : null;

                        await context.Response.WriteResultAsync(Result.Unauthorized(message), context.HttpContext.RequestAborted);
                    },

                    OnForbidden = context => context.Response.WriteResultAsync(Result.Forbidden(), context.HttpContext.RequestAborted)
                };
            });

        return services;
    }
}
