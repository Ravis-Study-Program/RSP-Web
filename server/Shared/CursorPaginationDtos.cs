using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace server.Shared;

/// <summary>
/// Constants for cursor-based pagination.
/// </summary>
public static class CursorPaginationConstants
{
    /// <summary>
    /// The name of the timestamp property used for cursor pagination.
    /// All entities using cursor pagination must have this property.
    /// </summary>
    public const string TimestampPropertyName = "CreatedAtUtc";
}

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

    /// <summary>
    /// Parses a base64-encoded cursor string into a Cursor object.
    /// </summary>
    /// <param name="cursorString">The base64-encoded cursor string</param>
    /// <returns>A Cursor object if valid, null otherwise</returns>
    public static Cursor? Parse(string? cursorString)
    {
        if (string.IsNullOrEmpty(cursorString))
            return null;

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursorString));
            return JsonSerializer.Deserialize<Cursor>(json);
        }
        catch
        {
            return null; // Invalid cursor, treat as first page
        }
    }

    /// <summary>
    /// Encodes a timestamp and ID into a base64-encoded cursor string.
    /// </summary>
    /// <param name="timestamp">The timestamp component</param>
    /// <param name="id">The ID component</param>
    /// <returns>A base64-encoded cursor string</returns>
    public static string Encode(DateTime timestamp, string id)
    {
        var cursor = new Cursor
        {
            CreatedAtUtc = timestamp,
            Id = id
        };
        var json = JsonSerializer.Serialize(cursor);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }
}

/// <summary>
/// Options for cursor-based pagination.
/// Encapsulates all parameters needed for cursor pagination queries.
/// Note: This implementation orders by CreatedAtUtc DESC, then by primary key DESC.
/// Entities must have a CreatedAtUtc property and a primary key.
/// </summary>
/// <typeparam name="TEntity">The entity type being paginated</typeparam>
public record CursorPaginationOptions<TEntity> where TEntity : class
{
    /// <summary>
    /// Number of items to return per page.
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Optional filter predicate to apply to the query.
    /// </summary>
    public Expression<Func<TEntity, bool>>? Predicate { get; init; }

    /// <summary>
    /// Optional cursor for pagination (timestamp, id). Null returns first page.
    /// </summary>
    public Cursor? Cursor { get; init; }

    /// <summary>
    /// True for forward pagination, False for backward pagination.
    /// </summary>
    public bool Forward { get; init; } = true;

    /// <summary>
    /// Optional related data to include in the query.
    /// </summary>
    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? Include { get; init; }
}

/// <summary>
/// Response type for repository-level cursor pagination.
/// Contains the raw pagination results before encoding cursors.
/// </summary>
/// <typeparam name="T">The type of items in the page</typeparam>
public record PaginatedResponse<T>
{
    /// <summary>
    /// The items in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// Indicates whether there are more items available after this page.
    /// </summary>
    public bool HasMore { get; init; }

    /// <summary>
    /// Indicates whether there are items available before this page.
    /// </summary>
    public bool HasPrevious { get; init; }
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
