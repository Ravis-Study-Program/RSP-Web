using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class User
{
    public Guid UserId { get; set; }

    [Required]
    public string DiscordId { get; set; } = string.Empty;
    
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public bool IsAdmin { get; set; } = false;
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required] 
    public string ProfileImage { get; set; } = string.Empty;
    
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}