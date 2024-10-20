using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RSPWebAPI.Entities;

public class LeetcodeProblemCategory
{
  [Required] public Guid LeetcodeProblemCategoryId { get; set; }
  [Required] public string Name { get; set; }

  [JsonIgnore] public ICollection<LeetcodeProblem> LeetcodeProblems { get; set; }
}
