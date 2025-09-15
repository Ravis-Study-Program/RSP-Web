using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.ProblemAttempts.Dtos;

public record UpdateProblemAttemptRequest
{
  [Required]
  public string ProblemAttemptId { get; set; } = string.Empty;

  [Required]
  public DateTime AttemptStartDateUtc { get; set; }

  [Required]
  public int TimeTakenInMinutes { get; set; }

  [Required]
  public string UserId { get; set; } = string.Empty;

  public string Notes { get; set; } = string.Empty;
  public string? LeetcodeProblemId { get; set; }
  public string? CustomProblemId { get; set; }
  public string? EnrollmentId { get; set; }
}

public record UpdateProblemAttemptResponse { }

public class UpdateProblemAttemptRequestValidator : AbstractValidator<UpdateProblemAttemptRequest>
{
  public UpdateProblemAttemptRequestValidator()
  {
    RuleFor(c => c.ProblemAttemptId).NotEmpty();
    RuleFor(c => c.AttemptStartDateUtc).NotEmpty();
    RuleFor(c => c.TimeTakenInMinutes).NotEmpty().GreaterThanOrEqualTo(1);
    RuleFor(c => c.UserId).NotEmpty();
  }
}
