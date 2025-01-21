using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.MockInterviews.Dtos;

public record ListMockInterviewRequest
{
  [Required]
  public string Email { get; set; } = string.Empty;

  [Required]
  public bool IncludeLeetcode { get; set; }

  [Required]
  public bool IncludeBehavioural { get; set; }

  [Required]
  public bool IncludeCustom { get; set; }

  public string? EnrollmentId { get; set; }
}

public record ListMockInterviewResponse
{
  [Required]
  public IList<MockInterviewEntity> MockInterviews { get; init; } = new List<MockInterviewEntity>();
}

public class ListMockInterviewRequestValidator : AbstractValidator<ListMockInterviewRequest>
{
  public ListMockInterviewRequestValidator()
  {
    RuleFor(c => c.Email).NotEmpty().EmailAddress();
  }
}
