using System.Net;
using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Common;

public class BaseController : ControllerBase
{
  protected ActionResult<ApiResponse<T>> ErrorResponse<T>(
    string errorMessage,
    HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest
  )
  {
    return StatusCode(
      (int)httpStatusCode,
      new ApiResponse<T> { Error = new ApiError(errorMessage) }
    );
  }

  protected ActionResult<ApiResponse<T>> SuccessResponse<T>(
    string successMessage,
    HttpStatusCode httpStatusCode = HttpStatusCode.OK,
    T? responseBody = default
  )
  {
    return StatusCode(
      (int)httpStatusCode,
      new ApiResponse<T> { ResponseBody = responseBody, SuccessMessage = successMessage }
    );
  }

  protected ActionResult<ApiResponse<T>> OkResponse<T>(T responseBody, string successMessage)
  {
    return Ok(new ApiResponse<T> { ResponseBody = responseBody, SuccessMessage = successMessage });
  }

  protected string? GetCurrentUserEmail()
  {
    return HttpContext.User.Identity?.Name;
  }
}
