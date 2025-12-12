using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;
using server.Shared;

namespace RSPWebAPI.Features.MockInterviews.Dtos;

public record ListMockInterviewRequest
{
  [Required]
  public IList<string> UserIds { get; set; } = new List<string>();

  [Required]
  public bool IncludeLeetcode { get; set; }

  [Required]
  public bool IncludeBehavioural { get; set; }

  [Required]
  public bool IncludeCustom { get; set; }

  public string? SeasonId { get; set; }

  /// <summary>
  /// Base64-encoded cursor for pagination. If null, returns the first page.
  /// </summary>
  public string? Cursor { get; set; }

  /// <summary>
  /// Pagination direction. True for forward (next), False for backward (previous).
  /// Defaults to true (forward).
  /// </summary>
  public bool Forward { get; set; } = true;

  /// <summary>
  /// Number of items to return per page. Defaults to 10 if not specified.
  /// Maximum value is 100.
  /// </summary>
  public int PageSize { get; set; } = 10;
}

public record ListMockInterviewResponse
{
  [Required]
  public IList<MockInterviewEntity> MockInterviews { get; init; } = new List<MockInterviewEntity>();
}

/// <summary>
/// Cursor-based paginated response for mock interviews.
/// Use this when Cursor or PageSize is specified in the request.
/// </summary>
public record ListMockInterviewCursorResponse : CursorPagedResponse<MockInterviewEntity>
{
}

public class ListMockInterviewRequestValidator : AbstractValidator<ListMockInterviewRequest>
{
  public ListMockInterviewRequestValidator()
  {
    RuleFor(c => c.UserIds).NotEmpty().WithMessage("At least one user Id must be provided.");

    RuleFor(c => c.PageSize)
      .GreaterThan(0)
      .WithMessage("PageSize must be greater than 0.")
      .LessThanOrEqualTo(100)
      .WithMessage("PageSize cannot exceed 100.");

    RuleFor(c => c.Cursor)
      .Must(BeValidBase64).When(c => !string.IsNullOrEmpty(c.Cursor))
      .WithMessage("Cursor must be a valid base64-encoded string.");
  }

  private bool BeValidBase64(string? cursor)
  {
    if (string.IsNullOrEmpty(cursor))
      return true;

    try
    {
      var buffer = new Span<byte>(new byte[cursor.Length]);
      return Convert.TryFromBase64String(cursor, buffer, out _);
    }
    catch
    {
      return false;
    }
  }
}
