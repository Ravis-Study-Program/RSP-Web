using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Enrollments.Dtos;

public record GetUserEnrollmentsRequest
{
  public string Email { get; set; } = string.Empty;
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
  public DateTime SeasonStartDate { get; set; }

  [Required]
  public DateTime SeasonEndDate { get; set; }

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

  public int NumStudentsInSeason { get; set; }

  public int NumMentorsInSeason { get; set; }

  public int NumMenteesInSeason { get; set; }
}

public record GetUserEnrollmentsResponse
{
  [Required]
  public IList<EnrollmentResponseDto> Enrollments { get; set; } = new List<EnrollmentResponseDto>();
}
