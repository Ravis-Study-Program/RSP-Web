using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class Season
{
  public Guid SeasonId { get; set; }

  [Required] public string Name { get; set; } = string.Empty;

  [Required] public string Slug { get; set; } = string.Empty;

  [Required] public DateTime StartDate { get; set; }

  [Required] public DateTime EndDate { get; set; }

  [Required] public string Location { get; set; } = string.Empty;

  [Required] public string ImageUrl { get; set; } = string.Empty;
}