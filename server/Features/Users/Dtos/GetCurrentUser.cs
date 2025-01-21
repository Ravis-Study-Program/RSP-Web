using System.ComponentModel.DataAnnotations;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Users.Dtos;

public record GetCurrentUserRequest { };

public record GetCurrentUserResponse
{
  [Required]
  public UserEntity? User { get; set; }
}
