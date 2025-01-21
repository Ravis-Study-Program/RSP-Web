using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.Enrollments.Dtos;

public record KickStudentRequest
{
  [Required]
  public string MenteeEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string SeasonSlug { get; set; } = string.Empty;

  [Required]
  public string Email { get; set; } = string.Empty;
}

public record KickStudentResponse { }

public class KickStudentRequestValidator : AbstractValidator<KickStudentRequest>
{
  public KickStudentRequestValidator()
  {
    RuleFor(c => c.MenteeEnrollmentId).NotEmpty();
    RuleFor(c => c.SeasonSlug).NotEmpty();
    RuleFor(c => c.Email).NotEmpty().EmailAddress();
  }
}
