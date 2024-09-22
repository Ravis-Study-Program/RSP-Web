namespace RSPWebAPI.Entities;

public class Role
{
  public Guid RoleId { get; set; }

  public string Name { get; set; } = string.Empty;

  public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}