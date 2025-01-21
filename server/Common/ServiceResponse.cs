using RSPWebAPI.Common.Interfaces;

namespace RSPWebAPI.Common;

public abstract class ServiceResponse<T> : IServiceResponse<T>
{
  public T? Data { get; set; }
  public string Message { get; set; } = string.Empty;
  public abstract bool IsSuccess { get; }
}

public class SuccessServiceResponse<T> : ServiceResponse<T>
{
  public override bool IsSuccess => true;

  public SuccessServiceResponse(string message, T? data = default)
  {
    Data = data;
    Message = message;
  }
}

public class ErrorServiceResponse<T> : ServiceResponse<T>
{
  public override bool IsSuccess => false;

  public ErrorServiceResponse(string message, T? data = default)
  {
    Data = data;
    Message = message;
  }
}
