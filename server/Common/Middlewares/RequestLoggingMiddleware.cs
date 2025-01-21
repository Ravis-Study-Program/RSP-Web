using System.Diagnostics;

namespace RSPWebAPI.Common.Middlewares;

public class RequestLoggingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<RequestLoggingMiddleware> _logger;

  public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var stopwatch = Stopwatch.StartNew();

    var request = context.Request;
    _logger.LogInformation($"Incoming Request: {request.Method} {request.Path}");

    var originalResponseBody = context.Response.Body;
    try
    {
      using var responseBodyStream = new MemoryStream();
      context.Response.Body = responseBodyStream;

      await _next(context);

      stopwatch.Stop();

      context.Response.Body.Seek(0, SeekOrigin.Begin);
      var statusCode = context.Response.StatusCode;

      _logger.LogInformation(
        $"Response: {request.Method} {request.Path} responded with {statusCode} in {stopwatch.ElapsedMilliseconds}ms"
      );

      context.Response.Body.Seek(0, SeekOrigin.Begin);
      await responseBodyStream.CopyToAsync(originalResponseBody);
    }
    catch (Exception ex)
    {
      stopwatch.Stop();
      _logger.LogError(
        ex,
        $"An error occurred while processing the request: {request.Method} {request.Path}"
      );
      throw;
    }
    finally
    {
      context.Response.Body = originalResponseBody;
    }
  }
}
