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


{
  var host = Environment.GetEnvironmentVariable("PGHOST");
  var port = Environment.GetEnvironmentVariable("PGPORT");
  var database = Environment.GetEnvironmentVariable("PGDATABASE");
  var username = Environment.GetEnvironmentVariable("PGUSER");
  var password = Environment.GetEnvironmentVariable("PGPASSWORD");
  var serverPort = Environment.GetEnvironmentVariable("PORT");
  var auth0Domain = Environment.GetEnvironmentVariable("AUTH0_DOMAIN");
  var auth0Audience = Environment.GetEnvironmentVariable("AUTH0_AUDIENCE");

  var connectionString =
    $"Host={host};"
    + $"Port={port};"
    + $"Database={database};"
    + $"Username={username};"
    + $"Password={password};";
  var assembly = typeof(Program).Assembly;
  builder.WebHost.UseUrls($"http://*:{serverPort}");

  builder
    .Services.AddCors(options =>
    {
      options.AddPolicy(
        "CorsPolicy",
        policyBuilder =>
        {
          policyBuilder.AllowAnyHeader().AllowAnyMethod().WithOrigins("*");
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
              Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer",
              },
            },
            new string[] { }
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
      config.RegisterServicesFromAssembly(assembly);
      config.AddOpenBehavior(typeof(LoggingPipelineBehaviour<,>));
      config.AddOpenBehavior(typeof(AuthenticationPipelineBehavior<,>));
      config.AddOpenBehavior(typeof(AdminAuthenticationPipelineBehaviour<,>));
      config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
    })
    .AddValidatorsFromAssembly(assembly, includeInternalTypes: true)
    .AddCarter()
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
      options.Authority = auth0Domain;
      options.Audience = auth0Audience;
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
}

var app = builder.Build();


{
  app.UseCors("CorsPolicy");
  app.UseSwagger()
    .UseSwaggerUI()
    .UseExceptionHandler("/error")
    .UseHttpsRedirection()
    .UseAuthentication()
    .ApplyMigrations();
  app.MapCarter();
  app.Map(
    "/error",
    (HttpContext context) =>
    {
      var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
      return new ApiResult<string> { Error = new ApiError(exception?.Message ?? "Error occurred") };
    }
  );
  app.MapHealthChecks("/health").AllowAnonymous();
}

app.Run();
