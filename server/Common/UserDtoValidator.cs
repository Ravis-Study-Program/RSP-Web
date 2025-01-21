using FluentValidation;

namespace RSPWebAPI.Common;

public record UserTestDto
{
  public string UserId { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
}

public class UserTestDtoValidator : AbstractValidator<UserTestDto>
{
  public UserTestDtoValidator()
  {
    RuleFor(user => user.UserId)
      .NotEmpty()
      .WithMessage("User ID is required.")
      .Length(1, 50)
      .WithMessage("User ID must be between 1 and 50 characters.");

    RuleFor(user => user.Email)
      .NotEmpty()
      .WithMessage("Email is required.")
      .EmailAddress()
      .WithMessage("Invalid email format.");
  }
}
