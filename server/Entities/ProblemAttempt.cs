using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class ProblemAttempt
{
  [Required] public Guid ProblemAttemptId { get; set; }
  [Required] public DateTime AttemptStartDate { get; set; }
  [Required] public int TimeTakenInMinutes { get; set; }
  [Required] public string Notes { get; set; }
  
  // User field allows ProblemAttempts to be potentially outside a particular season
  [Required] public Guid UserId { get; set; }
  [Required] public User User { get; set; }
  public Guid? LeetcodeProblemId { get; set; }
  public LeetcodeProblem? LeetcodeProblem { get; set; }
  
  public Guid? CustomProblemId { get; set; }
  public CustomProblem? CustomProblem { get; set; }
  
  public Guid? EnrollmentId { get; set; }
  public Enrollment? Enrollment { get; set; }
}
