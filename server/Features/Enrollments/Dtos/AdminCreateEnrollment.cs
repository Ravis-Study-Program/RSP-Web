using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Enrollments.Dtos;

public record AdminCreateEnrollmentRequest
{
  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public string UserId { get; set; } = string.Empty;

  [Required]
  public SeasonRole Role { get; set; }

  [Required]
  public SeasonStudentRolePromotion StudentRolePromotion { get; set; }
}

public record AdminCreateEnrollmentResponse
{
  [Required]
  public string EnrollmentId { get; set; } = string.Empty;
}

public class AdminCreateEnrollmentRequestValidator : AbstractValidator<AdminCreateEnrollmentRequest>
{
  public AdminCreateEnrollmentRequestValidator()
  {
    RuleFor(c => c.SeasonId).NotEmpty();
    RuleFor(c => c.UserId).NotEmpty();
    RuleFor(c => c.Role).IsInEnum();
    RuleFor(c => c.StudentRolePromotion).IsInEnum();
  }
}
