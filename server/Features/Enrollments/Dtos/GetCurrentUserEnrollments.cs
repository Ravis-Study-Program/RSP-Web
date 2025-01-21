using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Enrollments.Dtos;

public record GetCurrentUserEnrollmentsRequest
{
  [Required]
  public string? Email { get; set; } = string.Empty;
}

public record EnrollmentResponseDto
{
  [Required]
  public string EnrollmentId { get; set; } = string.Empty;

  [Required]
  public SeasonRole Role { get; set; }

  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public string SeasonName { get; set; } = string.Empty;

  [Required]
  public string SeasonImageUrl { get; set; } = string.Empty;

  [Required]
  public string SeasonSlug { get; set; } = string.Empty;

  [Required]
  public string UserId { get; set; } = string.Empty;

  [Required]
  public string UserName { get; set; } = string.Empty;

  [Required]
  public SeasonStudentRolePromotion StudentRolePromotion { get; set; }
}

public record GetCurrentUserEnrollmentsResponse
{
  [Required]
  public IList<EnrollmentResponseDto> Enrollments { get; set; } = new List<EnrollmentResponseDto>();
}

public class GetCurrentUserEnrollmentsRequestValidator
  : AbstractValidator<GetCurrentUserEnrollmentsRequest>
{
  public GetCurrentUserEnrollmentsRequestValidator()
  {
    RuleFor(c => c.Email).NotEmpty().EmailAddress();
  }
}
