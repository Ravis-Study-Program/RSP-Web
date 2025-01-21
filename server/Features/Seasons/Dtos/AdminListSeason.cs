using System.ComponentModel.DataAnnotations;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Seasons.Dtos;

public record AdminListSeasonRequest { };

public record AdminListSeasonResponse
{
  [Required]
  public IList<SeasonEntity> Seasons { get; init; } = new List<SeasonEntity>();
}
