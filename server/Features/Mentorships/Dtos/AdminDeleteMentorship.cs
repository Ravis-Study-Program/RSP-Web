using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.Mentorships.Dtos;

public record AdminDeleteMentorshipRequest
{
  [Required]
  public string MentorshipId { get; set; } = string.Empty;
}

public record AdminDeleteMentorshipResponse { }

public class AdminDeleteMentorshipRequestValidator : AbstractValidator<AdminDeleteMentorshipRequest>
{
  public AdminDeleteMentorshipRequestValidator()
  {
    RuleFor(c => c.MentorshipId).NotEmpty();
  }
}
