using RSPWebAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Features.Seasons.Dtos;

public class GetMentorAssignmentsRequest
{
  // No additional parameters needed - seasonSlug comes from route
}

public class GetMentorAssignmentsResponse
{
  [Required]
  public required List<MentorAssignmentDto> Assignments { get; set; }
}

public class MentorAssignmentDto
{
  [Required]
  public string StudentId { get; set; } = string.Empty;

  [Required]
  public string StudentName { get; set; } = string.Empty;

  [Required]
  public string StudentSlug { get; set; } = string.Empty;

  [Required]
  public string EnrollmentId { get; set; } = string.Empty;

  [Required]
  public SeasonStudentRolePromotion StudentRolePromotion { get; set; }

  public string? MentorId { get; set; }

  public string? MentorName { get; set; }
  
  public string? MentorSlug { get; set; }
}