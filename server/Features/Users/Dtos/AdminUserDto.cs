using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Features.Users.Dtos;

public record AdminUserDto
{
  [Required]
  public string UserId { get; set; } = string.Empty;

  public string? DiscordId { get; set; }

  [Required]
  public string Email { get; set; } = string.Empty;

  [Required]
  public bool IsAdmin { get; set; }

  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string Slug { get; set; } = string.Empty;

  public string? ProfileImage { get; set; }
  public DateTime? DeletedAtUtc { get; set; }
}
