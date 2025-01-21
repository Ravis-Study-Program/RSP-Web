using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Shared;

public class ApiError
{
  protected ApiError() { }

  public ApiError(string message)
  {
    Message = message;
  }

  public ApiError(string message, ICollection<ValidationError> validationErrors)
  {
    Message = message;
    ValidationErrors = validationErrors;
  }

  public string Message { get; set; } = string.Empty;

  public ICollection<ValidationError> ValidationErrors { get; set; } = new List<ValidationError>();
}
