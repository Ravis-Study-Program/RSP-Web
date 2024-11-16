using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;

namespace RSPWebAPI.Shared;

public abstract class ApiResult
{
  protected ApiResult() { }

  protected ApiResult(HttpStatusCode statusCode, ApiError? error = null)
  {
    StatusCode = statusCode;
    Error = error;
  }

  public HttpStatusCode StatusCode { get; set; }

  public bool IsSuccess => (int)StatusCode >= 200 && (int)StatusCode < 300;

  public ApiError? Error { get; set; }

  public string? SuccessMessage { get; set; }
}

public class ApiResult<TValue> : ApiResult
{
  public ApiResult() { }

  protected ApiResult(TValue? responseBody, HttpStatusCode statusCode, ApiError? error = null)
    : base(statusCode, error)
  {
    ResponseBody = responseBody;
    StatusCode = statusCode;
    Error = error;
  }

  public TValue? ResponseBody { get; set; }
}

public static class ApiResultHelper
{
  public static Results<
    Ok<ApiResult<T>>,
    NotFound<ApiResult<T>>,
    BadRequest<ApiResult<T>>
  > FormatResponse<T>(ApiResult<T> response)
  {
    return response.StatusCode switch
    {
      HttpStatusCode.OK => TypedResults.Ok(response),
      HttpStatusCode.NotFound => TypedResults.NotFound(response),
      _ => TypedResults.BadRequest(response),
    };
  }
}
