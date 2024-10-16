using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class LeetcodeProblemDifficulty
{
  [Required] public Guid LeetcodeProblemDifficultyId { get; set; }
  [Required] public string Name { get; set; }
}
