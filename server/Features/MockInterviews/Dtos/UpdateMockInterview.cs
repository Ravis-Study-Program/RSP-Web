using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.MockInterviews.Dtos;

public record UpdateMockInterviewRequest
{
  [Required]
  public string MockInterviewId { get; set; } = string.Empty;

  [Required]
  public string InterviewerUserId { get; set; } = string.Empty;

  [Required]
  public string IntervieweeUserId { get; set; } = string.Empty;


  [Required]
  public List<MockInterviewRoundDto> MockInterviewRounds { get; set; } = new();

  public string? Notes { get; set; }

  public string? SeasonId { get; set; }
}

public record UpdateMockInterviewResponse { }

public class UpdateMockInterviewRequestValidator : AbstractValidator<UpdateMockInterviewRequest>
{
  public UpdateMockInterviewRequestValidator()
  {
    RuleFor(c => c.MockInterviewId).NotEmpty();
    RuleFor(c => c.IntervieweeUserId).NotEmpty();
    RuleFor(c => c.InterviewerUserId).NotEmpty();
    RuleFor(c => c.MockInterviewRounds).NotEmpty();
  }
}
