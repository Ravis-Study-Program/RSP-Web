using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.SeasonWeeks.Dtos;

public record AdminListSeasonWeekRequest
{
  [Required]
  public string? SeasonId { get; set; }
}

public record AdminListSeasonWeekResponse
{
  [Required]
  public IList<SeasonWeekEntity> SeasonWeeks { get; init; } = new List<SeasonWeekEntity>();
}

public class AdminListSeasonWeekRequestValidator : AbstractValidator<AdminListSeasonWeekRequest>
{
  public AdminListSeasonWeekRequestValidator()
  {
    RuleFor(c => c.SeasonId).NotEmpty();
  }
}
