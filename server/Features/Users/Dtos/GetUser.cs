using System.ComponentModel.DataAnnotations;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Users.Dtos;

public record GetUserRequest
{
  [Required]
  public string Email { get; set; } = string.Empty;
};

public record GetUserResponse
{
  [Required]
  public UserEntity? User { get; set; }
}
