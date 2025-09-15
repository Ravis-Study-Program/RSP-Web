using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.ProblemAttempts.Dtos;

public record DeleteProblemAttemptRequest
{
  [Required]
  public string ProblemAttemptId { get; set; } = string.Empty;

  [Required]
  public string UserId { get; set; } = string.Empty;
}

public record DeleteProblemAttemptResponse { }

public class DeleteProblemAttemptRequestValidator : AbstractValidator<DeleteProblemAttemptRequest>
{
  public DeleteProblemAttemptRequestValidator()
  {
    RuleFor(c => c.ProblemAttemptId).NotEmpty();
    RuleFor(c => c.UserId).NotEmpty();
  }
}
