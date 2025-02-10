using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.MockInterviews.Dtos;

public record CreateMockInterviewRequest
{
  [Required]
  public string InterviewerEmail { get; set; } = string.Empty;

  [Required]
  public string IntervieweeUserId { get; set; } = string.Empty;

  [Required]
  public DateTime StartDate { get; set; }

  [Required]
  public int TimeTakenInMinutes { get; set; }

  [Required]
  public List<MockInterviewRoundDto> MockInterviewRounds { get; set; } = new();

  public string? SeasonId { get; set; }
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
    RuleFor(c => c.IntervieweeUserId).NotEmpty();
    RuleFor(c => c.InterviewerEmail).NotEmpty().EmailAddress();
    RuleFor(c => c.StartDate).NotEmpty();
    RuleFor(c => c.TimeTakenInMinutes).NotEmpty().GreaterThan(0);
    RuleFor(c => c.MockInterviewRounds).NotEmpty();
  }
}
