using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Enrollments.Dtos;

public record AdminUpdateEnrollmentRequest
{
  [Required]
  public string EnrollmentId { get; set; } = string.Empty;

  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public string UserId { get; set; } = string.Empty;

  [Required]
  public SeasonRole Role { get; set; }

  [Required]
  public SeasonStudentRolePromotion StudentRolePromotion { get; set; }
}

public record AdminUpdateEnrollmentResponse
{
  [Required]
  public string EnrollmentId { get; set; } = string.Empty;
}

public class AdminUpdateEnrollmentRequestValidator : AbstractValidator<AdminUpdateEnrollmentRequest>
{
  public AdminUpdateEnrollmentRequestValidator()
  {
    RuleFor(c => c.EnrollmentId).NotEmpty();
    RuleFor(c => c.SeasonId).NotEmpty();
    RuleFor(c => c.UserId).NotEmpty();
    RuleFor(c => c.Role).IsInEnum();
    RuleFor(c => c.StudentRolePromotion).IsInEnum();
  }
}
