using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.MockInterviews.Dtos;

public record UpdateMockInterviewRequest
{
  [Required]
  public string MockInterviewId { get; set; } = string.Empty;

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

public record UpdateMockInterviewResponse { }

public class UpdateMockInterviewRequestValidator : AbstractValidator<UpdateMockInterviewRequest>
{
  public UpdateMockInterviewRequestValidator()
  {
    RuleFor(c => c.MockInterviewId).NotEmpty();
    RuleFor(c => c.InterviewerUserId).NotEmpty();
    RuleFor(c => c.IntervieweeEmail).NotEmpty().EmailAddress();
    RuleFor(c => c.StartDate).NotEmpty();
    RuleFor(c => c.TimeTakenInMinutes).NotEmpty().GreaterThan(0);
    RuleFor(c => c.MockInterviewRounds).NotEmpty();
  }
}
