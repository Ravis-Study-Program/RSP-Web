using System.ComponentModel.DataAnnotations;
using System.Net;
using Carter;
using FluentValidation;
using MediatR;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;
using AdminCreateSeasonResult =
  Microsoft.AspNetCore.Http.HttpResults.Results<
    Microsoft.AspNetCore.Http.HttpResults.Ok<
      RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.Seasons.AdminCreateSeasonResponse>>,
    Microsoft.AspNetCore.Http.HttpResults.NotFound<
      RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.Seasons.AdminCreateSeasonResponse>>, Microsoft.AspNetCore.Http.
    HttpResults.BadRequest<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.Seasons.AdminCreateSeasonResponse>>
  >;

namespace RSPWebAPI.Features.Seasons;

public static class AdminCreateSeason
{
  public class Command : AdminAuthRequest<ApiResult<AdminCreateSeasonResponse>>
  {
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime StartDateInclusiveUtc { get; set; }
    public DateTime EndDateInclusiveUtc { get; set; }
    public string Location { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.Name).NotEmpty();
      RuleFor(c => c.Slug).NotEmpty();
      RuleFor(c => c.StartDateInclusiveUtc).NotEmpty();
      RuleFor(c => c.EndDateInclusiveUtc)
        .NotEmpty()
        .GreaterThan(c => c.StartDateInclusiveUtc)
        .WithMessage("End date must be greater than start date.");
      RuleFor(c => c.Location).NotEmpty();
      RuleFor(c => c.ImageUrl).NotEmpty();
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminCreateSeasonResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public async Task<ApiResult<AdminCreateSeasonResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var season = new SeasonEntity
      {
        SeasonId = Database.Constants.GeneratePrimaryKeyId(),
        Name = request.Name,
        Slug = request.Slug,
        StartDateInclusiveUtc = request.StartDateInclusiveUtc,
        EndDateInclusiveUtc = request.EndDateInclusiveUtc,
        Location = request.Location,
        ImageUrl = request.ImageUrl
      };

      try
      {
        _dbContext.Add(season);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResult<AdminCreateSeasonResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminCreateSeasonResponse
          {
            SeasonId = season.SeasonId,
            Name = season.Name,
            StartDateInclusiveUtc = season.StartDateInclusiveUtc,
            EndDateInclusiveUtc = season.EndDateInclusiveUtc,
            Slug = season.Slug,
            Location = season.Location,
            ImageUrl = season.ImageUrl
          },
          SuccessMessage = Message.SeasonCreatedSuccessfully
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, Message.SeasonCreationUnexpectedError);

        return new ApiResult<AdminCreateSeasonResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError(Message.SeasonCreationUnexpectedError)
        };
      }
    }
  }
}

public class AdminCreateSeasonEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
         "api/admin/seasons",
         async Task<AdminCreateSeasonResult> (
           AdminCreateSeasonRequest request, ISender sender) =>
         {
           var command = new AdminCreateSeason.Command
           {
             Name = request.Name,
             Slug = request.Slug,
             StartDateInclusiveUtc = request.StartDateInclusiveUtc,
             EndDateInclusiveUtc = request.EndDateInclusiveUtc,
             Location = request.Location,
             ImageUrl = request.ImageUrl
           };
           var response = await sender.Send(command);

           return ApiResultHelper.FormatResponse(response);
         }
       )
       .WithName("AdminCreateSeason");
  }
}

public record AdminCreateSeasonRequest
{
  [Required] public string Name { get; set; } = string.Empty;
  [Required] public string Slug { get; set; } = string.Empty;
  [Required] public DateTime StartDateInclusiveUtc { get; set; }
  [Required] public DateTime EndDateInclusiveUtc { get; set; }
  [Required] public string Location { get; set; } = string.Empty;
  [Required] public string ImageUrl { get; set; } = string.Empty;
}

public class AdminCreateSeasonResponse
{
  [Required] public string SeasonId { get; set; } = string.Empty;
  [Required] public string Name { get; set; } = string.Empty;
  [Required] public string Slug { get; set; } = string.Empty;
  [Required] public DateTime StartDateInclusiveUtc { get; set; }
  [Required] public DateTime EndDateInclusiveUtc { get; set; }
  [Required] public string Location { get; set; } = string.Empty;
  [Required] public string ImageUrl { get; set; } = string.Empty;
}
