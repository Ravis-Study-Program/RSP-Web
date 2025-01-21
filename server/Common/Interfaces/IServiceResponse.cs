namespace RSPWebAPI.Common.Interfaces;

public interface IServiceResponse<T>
{
  T? Data { get; set; }
  string Message { get; set; }
  bool IsSuccess { get; }
}
