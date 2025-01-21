using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.Enrollments.Dtos;

public record AdminDeleteEnrollmentRequest
{
  [Required]
  public string EnrollmentId { get; set; } = string.Empty;
}

public record AdminDeleteEnrollmentResponse { }

public class AdminDeleteEnrollmentRequestValidator : AbstractValidator<AdminDeleteEnrollmentRequest>
{
  public AdminDeleteEnrollmentRequestValidator()
  {
    RuleFor(c => c.EnrollmentId).NotEmpty();
  }
}
