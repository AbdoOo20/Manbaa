using Microsoft.AspNetCore.Mvc;

using Manbaa.Application.Common;

namespace Manbaa.Api.Extensions;

public static class ApiResponseExtensions
{
    public static IServiceCollection AddApiResponses(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState.Values
                    .SelectMany(entry => entry.Errors)
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "The request is invalid." : error.ErrorMessage)
                    .ToList();

                var result = Result.BadRequest(errors);

                return new ObjectResult(result) { StatusCode = result.StatusCode };
            };
        });

        return services;
    }

    public static IApplicationBuilder UseApiStatusCodePages(this IApplicationBuilder app)
    {
        return app.UseStatusCodePages(async context =>
        {
            var response = context.HttpContext.Response;

            var status = response.StatusCode;

            var result = Enum.IsDefined(typeof(ResultStatus), status)
                ? Result.Failure((ResultStatus)status, null)
                : status >= 500 ? Result.InternalServerError() : Result.BadRequest("The request could not be processed.");

            await response.WriteAsJsonAsync(result, result.GetType());
        });
    }
}
