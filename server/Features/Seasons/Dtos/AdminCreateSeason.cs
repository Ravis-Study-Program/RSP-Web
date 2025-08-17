using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Features.Constants;

namespace RSPWebAPI.Features.Seasons.Dtos;

public record AdminCreateSeasonRequest
{
  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string Slug { get; set; } = string.Empty;

  [Required]
  public DateTime StartDateInclusiveUtc { get; set; }

  [Required]
  public DateTime EndDateInclusiveUtc { get; set; }

  [Required]
  public string Location { get; set; } = string.Empty;

  [Required]
  public string ImageUrl { get; set; } = string.Empty;
}

public record AdminCreateSeasonResponse
{
  [Required]
  public string SeasonId { get; set; } = string.Empty;
}

public class AdminCreateSeasonRequestValidator : AbstractValidator<AdminCreateSeasonRequest>
{
  public AdminCreateSeasonRequestValidator()
  {
    RuleFor(c => c.Name).NotEmpty();
    RuleFor(c => c.Slug).NotEmpty();
    RuleFor(c => c.StartDateInclusiveUtc).NotEmpty();
    RuleFor(c => c.EndDateInclusiveUtc)
      .NotEmpty()
      .GreaterThan(c => c.StartDateInclusiveUtc)
      .WithMessage(Messages.Common.EndDateMustBeGreaterThanStartDate);
    RuleFor(c => c.Location).NotEmpty();
    RuleFor(c => c.ImageUrl).NotEmpty();
  }
}
