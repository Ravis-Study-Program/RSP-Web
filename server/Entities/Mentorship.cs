using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class Mentorship
{
  [Required] public Guid MentorshipId { get; set; }

  [Required] public Guid MentorEnrollmentId { get; set; }
  [Required] public Enrollment MentorEnrollment { get; set; }

  [Required] public Guid MenteeEnrollmentId { get; set; }
  [Required] public Enrollment MenteeEnrollment { get; set; }
}
