using System.Net;
using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Database;
using RSPWebAPI.Shared;
using static RSPWebAPI.Database.Constants;

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

  protected string GetCurrentUserId()
  {
    return HttpContext.User.FindFirst($"{Domain}userId")?.Value ?? "invalid user id";
  }

  protected bool GetCurrentUserIsAdmin()
  {
    return HttpContext.User.FindFirst($"{Domain}isAdmin")?.Value == "true";
  }

  protected string? GetCurrentUserEmail()
  {
    // Try custom claim first, then fallback to Identity.Name
    return HttpContext.User.FindFirst("email")?.Value ?? HttpContext.User.Identity?.Name;
  }
}
