using System.ComponentModel.DataAnnotations;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Users.Dtos;

public record GetUserRequest
{
  public string? UserId { get; set; }

  public string? Slug { get; set; }
};

public record GetUserResponse
{
  [Required]
  public UserEntity? User { get; set; }
}
