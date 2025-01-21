using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.Seasons.Dtos;

public record AdminUpdateSeasonRequest
{
  [Required]
  public string SeasonId { get; set; } = string.Empty;

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

public record AdminUpdateSeasonResponse { }

public class AdminUpdateSeasonRequestValidator : AbstractValidator<AdminUpdateSeasonRequest>
{
  public AdminUpdateSeasonRequestValidator()
  {
    RuleFor(c => c.SeasonId).NotEmpty();
    RuleFor(c => c.Name).NotEmpty();
    RuleFor(c => c.Slug).NotEmpty();
    RuleFor(c => c.StartDateInclusiveUtc).NotEmpty();
    RuleFor(c => c.EndDateInclusiveUtc).NotEmpty();
    RuleFor(c => c.Location).NotEmpty();
    RuleFor(c => c.ImageUrl).NotEmpty();
  }
}
