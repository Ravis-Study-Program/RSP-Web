using System.Net;
using System.Text.Json;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Common.Middlewares;

public class GlobalExceptionHandlingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

  public GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger
  )
  {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
      await HandleExceptionAsync(context, ex);
    }
  }

  private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    var response = context.Response;
    response.ContentType = "application/json";

    var (statusCode, message) = GetStatusCodeAndMessage(exception);
    response.StatusCode = (int)statusCode;

    var apiError = new ApiError(message);
    var errorResponse = new ApiResponse<object> { ResponseBody = null, Error = apiError };

    var options = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    var jsonResponse = JsonSerializer.Serialize(errorResponse, options);
    await response.WriteAsync(jsonResponse);
  }

  private static (HttpStatusCode statusCode, string message) GetStatusCodeAndMessage(
    Exception exception
  )
  {
    return exception switch
    {
      KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
      ArgumentNullException => (HttpStatusCode.BadRequest, exception.Message),
      ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
      InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
      UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
      OperationCanceledException => (
        HttpStatusCode.RequestTimeout,
        "The request was cancelled or timed out"
      ),
      _ => (HttpStatusCode.InternalServerError, "An internal server error occurred"),
    };
  }
}
