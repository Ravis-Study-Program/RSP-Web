using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Common;
using RSPWebAPI.Features.Users.Dtos;
using RSPWebAPI.Features.Users.Interfaces;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.Users;

[ApiController]
[Route("api/v1/users")]
public class UserController : BaseController
{
  private readonly IUserService _userService;

  public UserController(IUserService userService)
  {
    _userService = userService;
  }

  #region Routes

  [HttpPost]
  [Route("admin/create")]
  [ActionName("AdminCreateUser")]
  public async Task<ActionResult<ApiResponse<AdminCreateUserResponse>>> AdminCreateUser(
    AdminCreateUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var response = await _userService.CreateAdminUser(request, cancellationToken);
    return HandleResponse(response);
  }

  [HttpDelete]
  [Route("admin/delete")]
  [ActionName("AdminDeleteUser")]
  public async Task<ActionResult<ApiResponse<AdminDeleteUserResponse>>> AdminDeleteUser(
    AdminDeleteUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var response = await _userService.DeleteAdminUser(request, cancellationToken);
    return HandleResponse(response);
  }

  [HttpGet]
  [Route("admin/get")]
  [ActionName("AdminListUser")]
  public async Task<ActionResult<ApiResponse<AdminListUserResponse>>> AdminListUser(
    [FromQuery] AdminListUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var response = await _userService.ListAdminUser(request, cancellationToken);
    return HandleResponse(response);
  }

  [HttpPut]
  [Route("admin/update")]
  [ActionName("AdminUpdateUser")]
  public async Task<ActionResult<ApiResponse<AdminUpdateUserResponse>>> AdminUpdateUser(
    AdminUpdateUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var response = await _userService.UpdateAdminUser(request, cancellationToken);
    return HandleResponse(response);
  }

  [HttpPost]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("create-user-if-not-exists")]
  [ActionName("CreateUserIfNotExists")]
  public async Task<ActionResult<ApiResponse<CreateUserIfNotExistsResponse>>> CreateUserIfNotExists(
    CreateUserIfNotExistsRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var currentUserEmail = GetCurrentUserEmail();
    var response = await _userService.CreateUserIfNotExists(
      request,
      currentUserEmail,
      cancellationToken
    );
    return HandleResponse(response);
  }

  [HttpGet]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("get-current-user")]
  [ActionName("GetCurrentUser")]
  public async Task<ActionResult<ApiResponse<GetCurrentUserResponse>>> GetCurrentUser(
    [FromQuery] GetCurrentUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var currentUserEmail = GetCurrentUserEmail();
    var response = await _userService.GetCurrentUser(currentUserEmail, cancellationToken);
    return HandleResponse(response);
  }

  [HttpGet]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("get-user")]
  [ActionName("GetUser")]
  public async Task<ActionResult<ApiResponse<GetUserResponse>>> GetUser(
    [FromQuery] GetUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var response = await _userService.GetUser(request, cancellationToken);
    return HandleResponse(response);
  }

  #endregion
}
