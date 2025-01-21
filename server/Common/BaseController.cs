using System.Net;
using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Common.Interfaces;
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

  protected ActionResult<ApiResponse<T>> HandleResponse<T>(
    IServiceResponse<T> response,
    HttpStatusCode errorHttpStatusCode = HttpStatusCode.BadRequest,
    HttpStatusCode successHttpStatusCode = HttpStatusCode.OK
  )
  {
    return response.IsSuccess
      ? SuccessResponse(response.Message, successHttpStatusCode, response.Data)
      : ErrorResponse<T>(response.Message, errorHttpStatusCode);
  }

  protected string? GetCurrentUserEmail()
  {
    return HttpContext.User.Identity?.Name;
  }
}
