using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class LeetcodeProblem
{
  [Required] public Guid LeetcodeProblemId { get; set; }
  [Required] public LeetcodeProblemDifficulty LeetcodeProblemDifficulty { get; set; }
  [Required] public bool IsPremium { get; set; }

  [Required] public Guid ProblemId { get; set; }
  [Required] public Problem Problem { get; set; }

  public ICollection<LeetcodeProblemCategory> LeetcodeProblemCategories { get; set; }
}