using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.ProblemAttempts.Dtos;

public record CreateProblemAttemptRequest
{
  [Required]
  public DateTime AttemptStartDateUtc { get; set; }

  [Required]
  public int TimeTakenInMinutes { get; set; }

  [Required]
  public string UserId { get; set; } = string.Empty;

  public string? Notes { get; set; }
  public string? LeetcodeProblemId { get; set; }
  public string? CustomProblemId { get; set; }
  public string? EnrollmentId { get; set; }
}

public record CreateProblemAttemptResponse
{
  [Required]
  public string ProblemAttemptId { get; set; } = string.Empty;
}

public class CreateProblemAttemptRequestValidator : AbstractValidator<CreateProblemAttemptRequest>
{
  public CreateProblemAttemptRequestValidator()
  {
    RuleFor(c => c.AttemptStartDateUtc).NotEmpty();
    RuleFor(c => c.TimeTakenInMinutes).NotEmpty().GreaterThanOrEqualTo(1);
    RuleFor(c => c.UserId).NotEmpty();
  }
}
