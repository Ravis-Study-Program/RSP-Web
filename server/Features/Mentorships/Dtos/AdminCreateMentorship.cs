using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.Mentorships.Dtos;

public record AdminCreateMentorshipRequest
{
  [Required]
  public string MentorEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string MenteeEnrollmentId { get; set; } = string.Empty;
}

public record AdminCreateMentorshipResponse
{
  [Required]
  public string MentorshipId { get; set; } = string.Empty;
}

public class AdminCreateMentorshipRequestValidator : AbstractValidator<AdminCreateMentorshipRequest>
{
  public AdminCreateMentorshipRequestValidator()
  {
    RuleFor(c => c.MentorEnrollmentId).NotEmpty();
    RuleFor(c => c.MenteeEnrollmentId).NotEmpty();
  }
}
