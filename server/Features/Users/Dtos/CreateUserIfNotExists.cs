using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.Users.Dtos;

public record CreateUserIfNotExistsRequest
{
  public string Name { get; set; } = string.Empty;
}

public record CreateUserIfNotExistsResponse
{
  [Required]
  public string UserId { get; set; } = string.Empty;
}

public class CreateUserIfNotExistsRequestValidator : AbstractValidator<CreateUserIfNotExistsRequest>
{
  public CreateUserIfNotExistsRequestValidator()
  {
    RuleFor(c => c.Name).NotEmpty();
  }
}
