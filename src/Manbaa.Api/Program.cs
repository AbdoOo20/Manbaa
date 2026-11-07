using System.Text.Json.Serialization;
using Manbaa.Api.BackgroundServices;
using Manbaa.Api.ExceptionHandling;
using Manbaa.Api.Extensions;
using Manbaa.Api.Hubs;
using Manbaa.Api.OpenApi;
using Manbaa.Api.Services;
using Manbaa.Application;
using Manbaa.Application.Abstractions;
using Manbaa.Application.Settings;
using Manbaa.Infrastructure;
using Manbaa.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("The 'Jwt' configuration section was not found.");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services.Configure<SystemAdminSettings>(builder.Configuration.GetSection(SystemAdminSettings.SectionName));

builder.Services.Configure<SubscriptionSettings>(builder.Configuration.GetSection(SubscriptionSettings.SectionName));

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddApiResponses();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserAppService, UserAppService>();

builder.Services.AddSignalR().AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton<INotificationPublisher, SignalRNotificationPublisher>();

builder.Services.AddApplication();

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddJwtAuthentication(jwtSettings);

builder.Services.AddAuthorization();

builder.Services.AddApiRateLimiting();

builder.Services.AddFrontendCors(builder.Configuration);

builder.Services.AddHostedService<SubscriptionReminderWorker>();

var app = builder.Build();

await app.Services.SeedIdentityAsync();

app.UseExceptionHandler();

app.UseApiStatusCodePages();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.MapOpenApi();

    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Manbaa API v1"));
}

app.UseHttpsRedirection();

app.UseCors(CorsExtensions.PolicyName);

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationsHub>(NotificationsHub.Route);

app.Run();

/// <summary>Exposed so integration tests can start the app.</summary>
public partial class Program;
