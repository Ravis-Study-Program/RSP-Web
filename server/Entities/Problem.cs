using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class Problem
{
  [Required] public Guid ProblemId { get; set; }
  [Required] public string Title { get; set; } = string.Empty;
  [Required] public string? Link { get; set; }

  public ICollection<LeetcodeProblemCategory> LeetcodeProblemCategories { get; set; }
}
