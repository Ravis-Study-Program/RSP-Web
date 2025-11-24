using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Shared;

/// <summary>
/// Base request DTO for paginated queries
/// </summary>
public record PagedRequest
{
  /// <summary>
  /// Page number (1-based indexing)
  /// </summary>
  [Range(1, int.MaxValue)]
  public int Page { get; set; } = 1;
  /// <summary>
  /// Number of items per page
  /// </summary>
  [Range(1, 100)]
  public int PageSize { get; set; } = 10;
}

/// <summary>
/// Validator for PagedRequest
/// </summary>
public class PagedRequestValidator : AbstractValidator<PagedRequest>
{
  public PagedRequestValidator()
  {
    RuleFor(x => x.Page)
      .GreaterThanOrEqualTo(1)
      .WithMessage("Page must be greater than or equal to 1");

    RuleFor(x => x.PageSize)
      .GreaterThanOrEqualTo(1)
      .WithMessage("PageSize must be greater than or equal to 1")
      .LessThanOrEqualTo(100)
      .WithMessage("PageSize must not exceed 100");
  }
}

/// <summary>
/// Generic paginated response wrapper
/// </summary>
/// <typeparam name="T">Type of items in the response</typeparam>
public record PagedResponse<T>
{
  /// <summary>
  /// Items for the current page
  /// </summary>
  [Required]
  public IList<T> Items { get; init; } = new List<T>();

  /// <summary>
  /// Total number of items across all pages
  /// </summary>
  [Required]
  public int TotalCount { get; init; }

  /// <summary>
  /// Current page number (1-based)
  /// </summary>
  [Required]
  public int Page { get; init; }

  /// <summary>
  /// Number of items per page
  /// </summary>
  [Required]
  public int PageSize { get; init; }

  /// <summary>
  /// Total number of pages
  /// </summary>
  [Required]
  public int TotalPages { get; init; }

  /// <summary>
  /// Whether there is a previous page
  /// </summary>
  [Required]
  public bool HasPreviousPage { get; init; }

  /// <summary>
  /// Whether there is a next page
  /// </summary>
  [Required]
  public bool HasNextPage { get; init; }

  /// <summary>
  /// Creates a new paged response with automatic calculation of pagination metadata
  /// </summary>
  public static PagedResponse<T> Create(
    IList<T> items,
    int totalCount,
    int page,
    int pageSize
  )
  {
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    return new PagedResponse<T>
    {
      Items = items,
      TotalCount = totalCount,
      Page = page,
      PageSize = pageSize,
      TotalPages = totalPages,
      HasPreviousPage = page > 1,
      HasNextPage = page < totalPages
    };
  }
}
