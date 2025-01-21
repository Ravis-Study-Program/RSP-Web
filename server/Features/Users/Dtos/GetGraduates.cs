using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Features.Users.Dtos;

public record GetGraduatesRequest { };

public record GraduateDto
{
  [Required]
  public string UserId { get; set; } = string.Empty;

  [Required]
  public string DiscordId { get; set; } = string.Empty;

  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string Email { get; set; } = string.Empty;

  [Required]
  public string ProfileImage { get; set; } = string.Empty;
}

public record GetGraduatesResponse
{
  [Required]
  public IList<GraduateDto> Graduates { get; init; } = new List<GraduateDto>();
}
