using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class Enrollment
{
  [Required] public Guid EnrollmentId { get; set; }
  [Required] public Guid SeasonId { get; set; }
  [Required] public Season Season { get; set; }

  [Required] public Guid UserId { get; set; }
  [Required] public User User { get; set; }

  [Required] public Guid RoleId { get; set; }
  [Required] public Role Role { get; set; }
}