using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.Users.Dtos;

public record UpdateUserSlugRequest
{
  [Required]
  public string Slug { get; set; } = string.Empty;
}

public record UpdateUserSlugResponse { }

public class UpdateUserSlugRequestValidator : AbstractValidator<UpdateUserSlugRequest>
{
  public UpdateUserSlugRequestValidator()
  {
    RuleFor(c => c.Slug)
      .NotEmpty()
      .Matches(@"^[a-z0-9-]+$")
      .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens")
      .Length(3, 50)
      .WithMessage("Slug must be between 3 and 50 characters long");
  }
}