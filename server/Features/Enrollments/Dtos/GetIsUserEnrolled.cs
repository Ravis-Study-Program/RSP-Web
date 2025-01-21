using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Enrollments.Dtos;

public record GetIsUserEnrolledRequest
{
  [Required]
  public string Email { get; set; } = string.Empty;

  public string SeasonSlug { get; set; } = string.Empty;
}

public record GetIsUserEnrolledResponse
{
  [Required]
  public bool IsEnrolled { get; set; }

  [Required]
  public SeasonRole? Role { get; set; }

  [Required]
  public string? EnrollmentId { get; set; }

  [Required]
  public string Email { get; set; } = string.Empty;

  [Required]
  public SeasonStudentRolePromotion StudentRolePromotion { get; set; }
}

public class GetIsUserEnrolledRequestValidator : AbstractValidator<GetIsUserEnrolledRequest>
{
  public GetIsUserEnrolledRequestValidator()
  {
    RuleFor(c => c.Email).NotEmpty().EmailAddress();
  }
}
