using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.MockInterviews.Dtos;

public record CreateMockInterviewRequest
{
  [Required]
  public string IntervieweeEmail { get; set; } = string.Empty;

  [Required]
  public string InterviewerUserId { get; set; } = string.Empty;

  [Required]
  public DateTime StartDate { get; set; }

  [Required]
  public int TimeTakenInMinutes { get; set; }

  [Required]
  public List<MockInterviewRoundDto> MockInterviewRounds { get; set; } = new();

  public string? EnrollmentId { get; set; }
}

public record CreateMockInterviewResponse
{
  [Required]
  public string MockInterviewId { get; set; } = string.Empty;
}

public class CreateMockInterviewRequestValidator : AbstractValidator<CreateMockInterviewRequest>
{
  public CreateMockInterviewRequestValidator()
  {
    RuleFor(c => c.InterviewerUserId).NotEmpty();
    RuleFor(c => c.IntervieweeEmail).NotEmpty().EmailAddress();
    RuleFor(c => c.StartDate).NotEmpty();
    RuleFor(c => c.TimeTakenInMinutes).NotEmpty().GreaterThan(0);
    RuleFor(c => c.MockInterviewRounds).NotEmpty();
  }
}
