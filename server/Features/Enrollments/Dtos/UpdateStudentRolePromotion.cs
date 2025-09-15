using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Enrollments.Dtos;

public record UpdateStudentRolePromotionRequest
{
  [Required]
  public string MenteeEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string SeasonSlug { get; set; } = string.Empty;

  [Required]
  public string UserId { get; set; } = string.Empty;

  [Required]
  public SeasonStudentRolePromotion StudentRolePromotion { get; set; }
}

public record UpdateStudentRolePromotionResponse { }

public class UpdateStudentRolePromotionRequestValidator
  : AbstractValidator<UpdateStudentRolePromotionRequest>
{
  public UpdateStudentRolePromotionRequestValidator()
  {
    RuleFor(c => c.MenteeEnrollmentId).NotEmpty();
    RuleFor(c => c.SeasonSlug).NotEmpty();
    RuleFor(c => c.UserId).NotEmpty();
    RuleFor(c => c.StudentRolePromotion).IsInEnum();
  }
}
