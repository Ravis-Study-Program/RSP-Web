using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Features.Users.Dtos;

public record CreateUserIfNotExistsRequest
{
  public string DiscordId { get; set; } = string.Empty;

  public string Name { get; set; } = string.Empty;

  public string ProfileImage { get; set; } = string.Empty;
}

public record CreateUserIfNotExistsResponse
{
  [Required]
  public string UserId { get; set; } = string.Empty;
}
