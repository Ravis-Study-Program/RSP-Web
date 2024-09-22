namespace RSPWebAPI.Entities;

public class Enrollment
{
  public Guid EnrollmentId { get; set; }
  
  public Guid SeasonId { get; set; }
  public Season Season { get; set; }

  public Guid UserId { get; set; }
  public User User { get; set; }

  public Guid RoleId { get; set; }
  public Role Role { get; set; }
}