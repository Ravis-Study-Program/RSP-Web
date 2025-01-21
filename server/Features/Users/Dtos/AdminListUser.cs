using System.ComponentModel.DataAnnotations;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Users.Dtos;

public record AdminListUserRequest { };

public record AdminListUserResponse
{
  [Required]
  public IList<UserEntity> Users { get; init; } = new List<UserEntity>();
}
