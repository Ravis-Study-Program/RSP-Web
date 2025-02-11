using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.MockInterviews.Dtos;

public record ListMockInterviewRequest
{
  [Required]
  public IList<string> Emails { get; set; } = new List<string>();

  [Required]
  public bool IncludeLeetcode { get; set; }

  [Required]
  public bool IncludeBehavioural { get; set; }

  [Required]
  public bool IncludeCustom { get; set; }

  public string? SeasonId { get; set; }
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
    RuleFor(c => c.Emails).NotEmpty().WithMessage("At least one email address must be provided.");
    RuleForEach(c => c.Emails).EmailAddress();
  }
}
