using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.ProblemAttempts.Dtos;

public record ListProblemAttemptRequest
{
  [Required]
  public IList<string> UserIds { get; set; } = new List<string>();

  [Required]
  public bool IncludeLeetcode { get; set; }

  [Required]
  public bool IncludeCustom { get; set; }

  public string? SeasonId { get; set; }

  /// <summary>
  /// Optional page number (1-based indexing). If not provided, all results are returned.
  /// </summary>
  [Range(1, int.MaxValue)]
  public int? Page { get; set; }

  /// <summary>
  /// Optional number of items per page. If not provided, all results are returned.
  /// </summary>
  [Range(1, 100)]
  public int? PageSize { get; set; }
}

public record ListProblemAttemptResponse
{
  /// <summary>
  /// Paginated result (populated when pagination params are provided)
  /// </summary>
  public PagedResponse<ProblemAttemptEntity>? Result { get; init; }

  /// <summary>
  /// All problem attempts (populated when pagination params are NOT provided)
  /// </summary>
  public IList<ProblemAttemptEntity>? ProblemAttempts { get; init; }
}

public class ListProblemAttemptRequestValidator : AbstractValidator<ListProblemAttemptRequest>
{
  public ListProblemAttemptRequestValidator()
  {
    RuleFor(c => c.UserIds).NotEmpty().WithMessage("At least one user id must be provided.");

    // Validate pagination parameters when provided
    When(
      x => x.Page.HasValue || x.PageSize.HasValue,
      () =>
      {
        RuleFor(x => x.Page)
          .NotNull()
          .WithMessage("Page must be provided when PageSize is specified")
          .GreaterThanOrEqualTo(1)
          .WithMessage("Page must be greater than or equal to 1")
          .When(x => x.Page.HasValue);

        RuleFor(x => x.PageSize)
          .NotNull()
          .WithMessage("PageSize must be provided when Page is specified")
          .GreaterThanOrEqualTo(1)
          .WithMessage("PageSize must be greater than or equal to 1")
          .LessThanOrEqualTo(100)
          .WithMessage("PageSize must not exceed 100")
          .When(x => x.PageSize.HasValue);
      }
    );
  }
}
