using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class Role
{
  [Required]
  public Guid RoleId { get; set; }
  [Required]
  public string Name { get; set; } = string.Empty;
}