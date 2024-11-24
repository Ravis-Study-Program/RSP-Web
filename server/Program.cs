using System.Security.Claims;
using System.Text.Json.Serialization;
using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RSPWebAPI.Database;
using RSPWebAPI.Database.Interceptors;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();

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
  })
  .AddHttpContextAccessor()
  .AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString).AddInterceptors(new SoftDeleteInterceptor())
  )
  .AddMediatR(config =>
  {
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(LoggingPipelineBehaviour<,>));
    config.AddOpenBehavior(typeof(AuthenticationPipelineBehavior<,>));
    config.AddOpenBehavior(typeof(AdminAuthenticationPipelineBehaviour<,>));
    config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
  })
  .AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true)
  .AddCarter()
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
  {
    options.Authority = config.Auth0Domain;
    options.Audience = config.Auth0Audience;
    options.TokenValidationParameters = new TokenValidationParameters
    {
      NameClaimType = ClaimTypes.Email,
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
    };
  });

builder
  .Services.AddControllers()
  .AddJsonOptions(options =>
  {
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
  });

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors("CorsPolicy");
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseSwagger();
app.UseSwaggerUI();
app.ApplyMigrations();

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
  public string Auth0Audience =>
    Environment.GetEnvironmentVariable("AUTH0_AUDIENCE")
    ?? throw new ArgumentNullException("AUTH0_AUDIENCE");
  public List<string> AllowedOrigins =>
    Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',').ToList()
    ?? throw new ArgumentNullException("ALLOWED_ORIGINS");

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
      "AUTH0_AUDIENCE",
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
