using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.Mentorships.Dtos;

public record AdminUpdateMentorshipRequest
{
  [Required]
  public string MentorshipId { get; set; } = string.Empty;

  [Required]
  public string MentorEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string MenteeEnrollmentId { get; set; } = string.Empty;
}

public record AdminUpdateMentorshipResponse { }

public class AdminUpdateMentorshipRequestValidator : AbstractValidator<AdminUpdateMentorshipRequest>
{
  public AdminUpdateMentorshipRequestValidator()
  {
    RuleFor(c => c.MentorshipId).NotEmpty();
    RuleFor(c => c.MentorEnrollmentId).NotEmpty();
    RuleFor(c => c.MenteeEnrollmentId).NotEmpty();
  }
}
