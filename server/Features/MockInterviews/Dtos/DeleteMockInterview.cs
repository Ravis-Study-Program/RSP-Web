using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.MockInterviews.Dtos;

public record DeleteMockInterviewRequest
{
  [Required]
  public string MockInterviewId { get; set; } = string.Empty;

  [Required]
  public string Email { get; set; } = string.Empty;
}

public record DeleteMockInterviewResponse { }

public class DeleteMockInterviewRequestValidator : AbstractValidator<DeleteMockInterviewRequest>
{
  public DeleteMockInterviewRequestValidator()
  {
    RuleFor(c => c.MockInterviewId).NotEmpty();
    RuleFor(c => c.Email).NotEmpty().EmailAddress();
  }
}
