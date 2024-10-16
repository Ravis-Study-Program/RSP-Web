using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class LeetcodeProblemCategory
{
  [Required] public Guid LeetcodeProblemCategoryId { get; set; }
  [Required] public string Name { get; set; }

  public ICollection<LeetcodeProblem> LeetcodeProblems { get; set; }
}
