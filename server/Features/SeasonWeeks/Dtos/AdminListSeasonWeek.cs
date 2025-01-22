using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.SeasonWeeks.Dtos;

public record AdminListSeasonWeekRequest { }

public record AdminListSeasonWeekResponse
{
  [Required]
  public IList<SeasonWeekEntity> SeasonWeeks { get; init; } = new List<SeasonWeekEntity>();
}
