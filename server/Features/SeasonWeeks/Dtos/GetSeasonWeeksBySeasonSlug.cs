using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.SeasonWeeks.Dtos;

public record GetSeasonWeeksBySeasonSlugRequest
{
  [Required]
  public string SeasonSlug { get; set; } = string.Empty;
}

public record GetSeasonWeeksBySeasonSlugResponse
{
  [Required]
  public IList<SeasonWeekEntity> SeasonWeeks { get; init; } = new List<SeasonWeekEntity>();
}

public class GetSeasonWeeksBySeasonSlugRequestValidator
  : AbstractValidator<GetSeasonWeeksBySeasonSlugRequest>
{
  public GetSeasonWeeksBySeasonSlugRequestValidator()
  {
    RuleFor(c => c.SeasonSlug).NotEmpty();
  }
}
