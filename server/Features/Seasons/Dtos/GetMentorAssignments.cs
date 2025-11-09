using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Seasons.Dtos;

public class GetMentorAssignmentsRequest
{
  // No additional parameters needed - seasonSlug comes from route
}

public class GetMentorAssignmentsResponse
{
  public required List<MentorAssignmentDto> Assignments { get; set; }
}

public class MentorAssignmentDto
{
  public required string StudentId { get; set; }
  public required string StudentName { get; set; }
  public required string EnrollmentId { get; set; }
  public required SeasonStudentRolePromotion StudentRolePromotion { get; set; }
  public string? MentorId { get; set; }
  public string? MentorName { get; set; }
}