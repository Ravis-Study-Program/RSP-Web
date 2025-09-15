using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Features.Users.Dtos;

public record AdminListUserRequest { };

public record AdminListUserResponse
{
  [Required]
  public IList<AdminUserDto> Users { get; init; } = new List<AdminUserDto>();
}
