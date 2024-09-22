using System.Net;

namespace RSPWebAPI.Shared;

public abstract class ApiResult
{
  protected ApiResult()
  {
  }

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
  public ApiResult()
  {
  }

  protected ApiResult(TValue? responseBody, HttpStatusCode statusCode, ApiError? error = null)
    : base(statusCode, error)
  {
    ResponseBody = responseBody;
    StatusCode = statusCode;
    Error = error;
  }

  public TValue? ResponseBody { get; set; }
}