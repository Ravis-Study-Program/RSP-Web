namespace RSPWebAPI.Shared;

public abstract class ApiResponse
{
  protected ApiResponse() { }

  protected ApiResponse(ApiError? error = null)
  {
    Error = error;
  }

  public ApiError? Error { get; set; }

  public string? SuccessMessage { get; set; }
}

public class ApiResponse<TValue> : ApiResponse
{
  public ApiResponse() { }

  protected ApiResponse(TValue? responseBody, ApiError? error = null)
    : base(error)
  {
    ResponseBody = responseBody;
    Error = error;
  }

  public TValue? ResponseBody { get; set; }
}
