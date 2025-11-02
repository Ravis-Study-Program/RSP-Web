using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.Users.Dtos;

public record AdminCreateUserRequest
{
  [Required]
  public string Email { get; set; } = string.Empty;

  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public bool IsAdmin { get; set; } = false;

  [Required]
  public bool IsTestUser { get; set; } = false;

  public string? ProfileImage { get; set; }
  public string? DiscordId { get; set; }
}

public record AdminCreateUserResponse
{
  [Required]
  public string UserId { get; set; } = string.Empty;
}

public class AdminCreateUserRequestValidator : AbstractValidator<AdminCreateUserRequest>
{
  public AdminCreateUserRequestValidator()
  {
    RuleFor(c => c.Email).NotEmpty().EmailAddress();
  }
}
