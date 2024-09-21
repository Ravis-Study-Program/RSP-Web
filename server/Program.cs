using System.Security.Claims;
using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RSPWebAPI.Database;
using RSPWebAPI.Shared.Behaviours;

var builder = WebApplication.CreateBuilder(args);
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var assembly = typeof(Program).Assembly;

    builder.Services
        .AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policyBuilder =>
            {
                policyBuilder.AllowAnyHeader().AllowAnyMethod().WithOrigins("*");
            });
        })
        .AddEndpointsApiExplorer()
        .AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Client",
                Version = "v1",
                Description = "RSP Web Application Backend Endpoints",
            });
        })
        .AddHttpContextAccessor()
        .AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString))
        .AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(LoggingPipelineBehaviour<,>));
            config.AddOpenBehavior(typeof(AuthenticationPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        })
        .AddValidatorsFromAssembly(assembly, includeInternalTypes: true)
        .AddCarter()
        .AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            };
        })
        .AddValidatorsFromAssembly(assembly)
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
            options.Audience = builder.Configuration["Auth0:Audience"];
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = ClaimTypes.NameIdentifier
            };
        });
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
    app.Map("/error", (HttpContext context) =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        return Results.Problem(exception?.Message, statusCode: StatusCodes.Status404NotFound);
    });
}

app.Run();
