using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.Users.Dtos;

public record AdminUpdateUserRequest
{
  [Required]
  public string UserId { get; set; } = string.Empty;

  [Required]
  public string DiscordId { get; set; } = string.Empty;

  [Required]
  public string Email { get; set; } = string.Empty;

  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string ProfileImage { get; set; } = string.Empty;

  [Required]
  public bool IsAdmin { get; set; }
}

public record AdminUpdateUserResponse { }

public class AdminUpdateUserRequestValidator : AbstractValidator<AdminUpdateUserRequest>
{
  public AdminUpdateUserRequestValidator()
  {
    RuleFor(c => c.UserId).NotEmpty();
    RuleFor(c => c.Email).NotEmpty().EmailAddress();
  }
}
