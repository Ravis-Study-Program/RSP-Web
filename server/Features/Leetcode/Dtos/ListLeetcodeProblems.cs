using System.ComponentModel.DataAnnotations;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Leetcode.Dtos;

public record ListLeetcodeProblemsRequest { }

public class ListLeetcodeProblemsResponse
{
  [Required]
  public IList<LeetcodeProblemDto> LeetcodeProblems { get; set; } = new List<LeetcodeProblemDto>();
}

public record LeetcodeProblemDto
{
  [Required]
  public List<LeetcodeProblemCategoryDto> LeetcodeProblemCategories = new();

  [Required]
  public string LeetcodeProblemId { get; set; } = string.Empty;

  [Required]
  public LeetcodeProblemDifficulty Difficulty { get; set; }

  [Required]
  public bool IsPremium { get; set; }

  [Required]
  public string Title { get; set; } = string.Empty;

  [Required]
  public string Link { get; set; } = string.Empty;
}

public record LeetcodeProblemCategoryDto
{
  [Required]
  public string Name { get; set; } = string.Empty;
}
