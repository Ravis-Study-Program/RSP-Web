using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.Leetcode.Dtos;

public record ListLeetcodeProblemsRequest : PagedRequest
{
  /// <summary>
  /// Filter by difficulty level (Easy, Medium, Hard)
  /// </summary>
  public string? Difficulty { get; set; }

  /// <summary>
  /// Filter by category name
  /// </summary>
  public string? Category { get; set; }

  /// <summary>
  /// Search term to filter by title
  /// </summary>
  public string? SearchTerm { get; set; }

  /// <summary>
  /// Field to sort by (e.g., "title", "difficulty")
  /// </summary>
  public string? SortBy { get; set; } = "title";

  /// <summary>
  /// Sort order: "asc" or "desc"
  /// </summary>
  public string? SortOrder { get; set; } = "asc";
}

public class ListLeetcodeProblemsRequestValidator : AbstractValidator<ListLeetcodeProblemsRequest>
{
  public ListLeetcodeProblemsRequestValidator()
  {
    Include(new PagedRequestValidator());

    RuleFor(x => x.Difficulty)
      .Must(d => d == null || new[] { "Easy", "Medium", "Hard" }.Contains(d))
      .When(x => !string.IsNullOrEmpty(x.Difficulty))
      .WithMessage("Difficulty must be Easy, Medium, or Hard");

    RuleFor(x => x.SortBy)
      .Must(s => s == null || new[] { "title", "difficulty" }.Contains(s))
      .When(x => !string.IsNullOrEmpty(x.SortBy))
      .WithMessage("SortBy must be either 'title' or 'difficulty'");

    RuleFor(x => x.SortOrder)
      .Must(o => o == null || new[] { "asc", "desc" }.Contains(o))
      .When(x => !string.IsNullOrEmpty(x.SortOrder))
      .WithMessage("SortOrder must be either 'asc' or 'desc'");
  }
}

public class ListLeetcodeProblemsResponse
{
  [Required]
  public PagedResponse<LeetcodeProblemDto> Result { get; set; } = PagedResponse<LeetcodeProblemDto>.Create(
    new List<LeetcodeProblemDto>(),
    0,
    1,
    10
  );
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
