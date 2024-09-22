namespace RSPWebAPI.Shared;

public class ApiError
{
  protected ApiError()
  {
  }

  public ApiError(string message)
  {
    Message = message;
  }

  public string Message { get; set; }
}