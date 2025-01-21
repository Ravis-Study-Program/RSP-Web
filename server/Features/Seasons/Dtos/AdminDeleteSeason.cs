using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.Seasons.Dtos;

public record AdminDeleteSeasonRequest
{
  [Required]
  public string SeasonId { get; set; } = string.Empty;
}

public record AdminDeleteSeasonResponse { }

public class AdminDeleteSeasonRequestValidator : AbstractValidator<AdminDeleteSeasonRequest>
{
  public AdminDeleteSeasonRequestValidator()
  {
    RuleFor(c => c.SeasonId).NotEmpty();
  }
}
