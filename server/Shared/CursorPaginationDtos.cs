using System.Text.Json.Serialization;

namespace server.Shared;

/// <summary>
/// Represents a cursor for keyset/cursor-based pagination.
/// Combines a timestamp and ID to create a stable, unique position in the dataset.
/// </summary>
public record Cursor
{
    /// <summary>
    /// The timestamp component of the cursor (typically CreatedAtUtc).
    /// </summary>
    [JsonPropertyName("createdAtUtc")]
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// The ID component of the cursor for tie-breaking.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
}

/// <summary>
/// Generic response type for cursor-based pagination.
/// </summary>
/// <typeparam name="T">The type of items in the page</typeparam>
public record CursorPagedResponse<T>
{
    /// <summary>
    /// The items in the current page.
    /// </summary>
    [JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// Base64-encoded cursor pointing to the next page.
    /// Null if there are no more pages.
    /// </summary>
    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; init; }

    /// <summary>
    /// Base64-encoded cursor pointing to the previous page.
    /// Null if this is the first page.
    /// </summary>
    [JsonPropertyName("previousCursor")]
    public string? PreviousCursor { get; init; }

    /// <summary>
    /// Indicates whether there are more items available after this page.
    /// </summary>
    [JsonPropertyName("hasMore")]
    public bool HasMore { get; init; }

    /// <summary>
    /// Optional total count of items (may be expensive to compute).
    /// Null if not requested or not available.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public int? TotalCount { get; init; }
}
