using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.SeasonWeeks.Dtos;

public record AdminDeleteSeasonWeekRequest
{
  [Required]
  public string SeasonWeekId { get; set; } = string.Empty;
}

public record AdminDeleteSeasonWeekResponse { }

public class AdminDeleteSeasonWeekRequestValidator : AbstractValidator<AdminDeleteSeasonWeekRequest>
{
  public AdminDeleteSeasonWeekRequestValidator()
  {
    RuleFor(c => c.SeasonWeekId).NotEmpty();
  }
}
