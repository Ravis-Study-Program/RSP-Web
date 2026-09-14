using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Carter;
using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using RSPWebAPI.Clients.Interfaces;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Auth;
using RSPWebAPI.Common.Cache;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Common.Middlewares;
using RSPWebAPI.Database;
using RSPWebAPI.Database.Interceptors;
using RSPWebAPI.Extensions;
using RSPWebAPI.Features.DummyData;
using RSPWebAPI.Features.DummyData.Interfaces;
using RSPWebAPI.Features.Enrollments;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.LeetcodeProblemRecommendations;
using RSPWebAPI.Features.LeetcodeProblemRecommendations.Interfaces;
using RSPWebAPI.Features.Leetcodes;
using RSPWebAPI.Features.Leetcodes.Interfaces;
using RSPWebAPI.Features.Mentorships;
using RSPWebAPI.Features.Mentorships.Interfaces;
using RSPWebAPI.Features.MockInterviews;
using RSPWebAPI.Features.MockInterviews.Interfaces;
using RSPWebAPI.Features.ProblemAttempts;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.Seasons;
using RSPWebAPI.Features.Seasons.Interfaces;
using RSPWebAPI.Features.SeasonWeeks;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users;
using RSPWebAPI.Features.Users.Interfaces;
using RSPWebAPI.Features.BackgroundServices;
using RSPWebAPI.Jobs;
using RSPWebAPI.Shared;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);
Env.Load();

var config = new AppConfiguration();
config.ValidateEnvironmentVariables();

var connectionString =
  $"Host={config.PgHost};Port={config.PgPort};Database={config.PgDatabase};Username={config.PgUser};Password={config.PgPassword};";

builder.WebHost.UseUrls($"http://*:{config.ServerPort}");

builder
  .Services.AddCors(options =>
  {
    options.AddPolicy(
      "CorsPolicy",
      policyBuilder =>
      {
        policyBuilder
          .AllowAnyHeader()
          .AllowAnyMethod()
          .WithOrigins(config.AllowedOrigins.ToArray())
          .SetPreflightMaxAge(TimeSpan.FromHours(1));
      }
    );
  })
  .AddMemoryCache()
  .AddEndpointsApiExplorer()
  .AddSwaggerGen(options =>
  {
    options.SchemaFilter<EnumSchemaFilter>();
    options.AddSecurityDefinition(
      "Bearer",
      new OpenApiSecurityScheme
      {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer",
      }
    );
    options.AddSecurityRequirement(
      new OpenApiSecurityRequirement
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
          },
          Array.Empty<string>()
        },
      }
    );
    options.SwaggerDoc(
      "v1",
      new OpenApiInfo
      {
        Title = "Client",
        Version = "v1",
        Description = "RSP Web Application Backend Endpoints",
      }
    );
    options.CustomOperationIds(r => r.ActionDescriptor.RouteValues["action"]);
  })
  .AddHttpContextAccessor()
  .AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString).AddInterceptors(new SoftDeleteInterceptor(), new AuditableEntityInterceptor())
  )
  .AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true)
  .AddCarter()
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.Authority = config.Auth0CustomDomain;
    options.Audience = config.Auth0Audience;
    options.TokenValidationParameters = new TokenValidationParameters
    {
      NameClaimType = ClaimTypes.Email,
      ValidateIssuer = true,
      ValidIssuer = config.Auth0CustomDomain,
      ValidateAudience = true,
      ValidAudience = config.Auth0Audience,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
    };
  });

// Add custom claims transformation
builder.Services.AddScoped<IClaimsTransformation, UserClaimsTransformation>();

builder
  .Services.AddControllers()
  .AddJsonOptions(options =>
  {
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
  });

builder.Services.AddValidatorsFromAssemblyContaining<UserTestDtoValidator>();
builder.Services.AddFluentValidationAutoValidation(configuration =>
{
  // Replace the default result factory with a custom implementation.
  configuration.OverrideDefaultResultFactoryWith<CustomValidatorResultFactory>();
});

builder.Services.AddHealthChecks();

var env = builder.Environment;
if (!env.IsDevelopment())
{
  builder.Services.AddHostedService<LeetcodeQuestionScraper>();
  builder.Services.AddHostedService<UpdateGraduateStatusService>();
}
builder.Services.AddScoped<DbContext, ApplicationDbContext>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<AuthAttribute>();
builder.Services.AddScoped<AdminAuthAttribute>();

builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ILeetcodeService, LeetcodeService>();
builder.Services.AddScoped<
  ILeetcodeProblemRecommendationService,
  LeetcodeProblemRecommendationService
>();
builder.Services.AddScoped<IMentorshipService, MentorshipService>();
builder.Services.AddScoped<IMockInterviewService, MockInterviewService>();
builder.Services.AddScoped<IProblemAttemptService, ProblemAttemptService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<ISeasonWeekService, SeasonWeekService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDummyDataService, DummyDataService>();
builder.Services.AddLazyResolution();

builder.Services.AddSingleton(config);
builder.Services.AddScoped<IRequestCache, MemoryRequestCache>();
builder.Services.AddScoped<IUserIdentityService, UserIdentityService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseRouting();
app.UseCors("CorsPolicy");

app.UseHttpsRedirection();
app.UseAuthentication();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}
app.ApplyMigrations();
app.UseHttpMetrics();
app.MapMetrics();

app.MapCarter();

app.Map(
  "/error",
  (HttpContext context) =>
  {
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    return new ApiResult<string>
    {
      Error = new ApiError(exception?.Message ?? "An error occurred"),
    };
  }
);

app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public class AppConfiguration
{
  public string PgHost =>
    Environment.GetEnvironmentVariable("PGHOST") ?? throw new ArgumentNullException("PGHOST");
  public string PgPort =>
    Environment.GetEnvironmentVariable("PGPORT") ?? throw new ArgumentNullException("PGPORT");
  public string PgDatabase =>
    Environment.GetEnvironmentVariable("PGDATABASE")
    ?? throw new ArgumentNullException("PGDATABASE");
  public string PgUser =>
    Environment.GetEnvironmentVariable("PGUSER") ?? throw new ArgumentNullException("PGUSER");
  public string PgPassword =>
    Environment.GetEnvironmentVariable("PGPASSWORD")
    ?? throw new ArgumentNullException("PGPASSWORD");
  public string ServerPort =>
    Environment.GetEnvironmentVariable("PORT") ?? throw new ArgumentNullException("PORT");
  public string Auth0Domain =>
    Environment.GetEnvironmentVariable("AUTH0_DOMAIN")
    ?? throw new ArgumentNullException("AUTH0_DOMAIN");
  public string Auth0ManagementApiDomain =>
    Environment.GetEnvironmentVariable("AUTH0_MANAGEMENT_API_DOMAIN")
    ?? throw new ArgumentNullException("AUTH0_MANAGEMENT_API_DOMAIN");
  public string Auth0CustomDomain =>
    Environment.GetEnvironmentVariable("AUTH0_CUSTOM_DOMAIN")
    ?? throw new ArgumentNullException("AUTH0_CUSTOM_DOMAIN");
  public string Auth0Audience =>
    Environment.GetEnvironmentVariable("AUTH0_AUDIENCE")
    ?? throw new ArgumentNullException("AUTH0_AUDIENCE");
  public List<string> AllowedOrigins =>
    Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',').ToList()
    ?? throw new ArgumentNullException("ALLOWED_ORIGINS");

  public string Auth0ClientId =>
    Environment.GetEnvironmentVariable("AUTH0_CLIENT_ID")
    ?? throw new ArgumentNullException("AUTH0_CLIENT_ID");

  public string Auth0ClientSecret =>
    Environment.GetEnvironmentVariable("AUTH0_CLIENT_SECRET")
    ?? throw new ArgumentNullException("AUTH0_CLIENT_SECRET");

  public void ValidateEnvironmentVariables()
  {
    var requiredVars = new[]
    {
      "PGHOST",
      "PGPORT",
      "PGDATABASE",
      "PGUSER",
      "PGPASSWORD",
      "AUTH0_DOMAIN",
      "AUTH0_MANAGEMENT_API_DOMAIN",
      "AUTH0_CUSTOM_DOMAIN",
      "AUTH0_AUDIENCE",
      "AUTH0_CLIENT_ID",
      "AUTH0_CLIENT_SECRET",
    };
    foreach (var varName in requiredVars)
    {
      if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(varName)))
      {
        throw new InvalidOperationException($"Environment variable '{varName}' is not set.");
      }
    }
  }
}

public partial class Program { }
