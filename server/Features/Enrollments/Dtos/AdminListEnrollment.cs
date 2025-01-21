using System.ComponentModel.DataAnnotations;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Enrollments.Dtos;

public record AdminListEnrollmentRequest { };

public record AdminListEnrollmentResponse
{
  [Required]
  public IList<EnrollmentResponseDto> Enrollments { get; init; } =
    new List<EnrollmentResponseDto>();
}
