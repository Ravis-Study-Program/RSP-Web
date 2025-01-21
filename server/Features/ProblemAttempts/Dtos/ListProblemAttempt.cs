using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.ProblemAttempts.Dtos;

public record ListProblemAttemptRequest
{
  [Required]
  public string Email { get; set; } = string.Empty;

  [Required]
  public bool IncludeLeetcode { get; set; }

  [Required]
  public bool IncludeCustom { get; set; }

  public string? EnrollmentId { get; set; }
}

public record ListProblemAttemptResponse
{
  [Required]
  public IList<ProblemAttemptEntity> ProblemAttempts { get; init; } =
    new List<ProblemAttemptEntity>();
}

public class ListProblemAttemptRequestValidator : AbstractValidator<ListProblemAttemptRequest>
{
  public ListProblemAttemptRequestValidator()
  {
    RuleFor(c => c.Email).NotEmpty().EmailAddress();
  }
}
