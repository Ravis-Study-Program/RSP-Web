using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.MockInterviews.Dtos;

public record CreateMockInterviewRequest
{
  [Required]
  public string InterviewerUserId { get; set; } = string.Empty;

  [Required]
  public string IntervieweeUserId { get; set; } = string.Empty;

  [Required]
  public int TimeTakenInMinutes { get; set; }

  [Required]
  public List<MockInterviewRoundDto> MockInterviewRounds { get; set; } = new();

  public string? Notes { get; set; }

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
    RuleFor(c => c.InterviewerUserId).NotEmpty();
    RuleFor(c => c.TimeTakenInMinutes).NotEmpty().GreaterThan(0);
    RuleFor(c => c.MockInterviewRounds).NotEmpty();
  }
}
