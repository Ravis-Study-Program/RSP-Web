using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.Users.Dtos;

public record AdminDeleteUserRequest
{
  [Required]
  public string UserId { get; set; } = string.Empty;
}

public record AdminDeleteUserResponse { }

public class AdminDeleteUserRequestValidator : AbstractValidator<AdminDeleteUserRequest>
{
  public AdminDeleteUserRequestValidator()
  {
    RuleFor(c => c.UserId).NotEmpty();
  }
}
