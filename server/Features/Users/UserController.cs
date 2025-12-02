using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Common;
using RSPWebAPI.Features.Constants;
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
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/create")]
  [ActionName("AdminCreateUser")]
  public async Task<ActionResult<ApiResponse<AdminCreateUserResponse>>> AdminCreateUser(
    AdminCreateUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _userService.CreateAdminUser(request, cancellationToken);
    return OkResponse(result, Messages.User.Created);
  }

  [HttpDelete]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/delete")]
  [ActionName("AdminDeleteUser")]
  public async Task<ActionResult<ApiResponse<AdminDeleteUserResponse>>> AdminDeleteUser(
    AdminDeleteUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _userService.DeleteAdminUser(request, cancellationToken);
    return OkResponse(result, Messages.User.Deleted);
  }

  [HttpGet]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/get")]
  [ActionName("AdminListUser")]
  public async Task<ActionResult<ApiResponse<AdminListUserResponse>>> AdminListUser(
    [FromQuery] AdminListUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _userService.ListAdminUser(request, cancellationToken);
    return OkResponse(result, Messages.User.Listed);
  }

  [HttpPut]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/update")]
  [ActionName("AdminUpdateUser")]
  public async Task<ActionResult<ApiResponse<AdminUpdateUserResponse>>> AdminUpdateUser(
    AdminUpdateUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _userService.UpdateAdminUser(request, cancellationToken);
    return OkResponse(result, Messages.User.Updated);
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
    var email = GetCurrentUserEmail();
    if (string.IsNullOrEmpty(email))
    {
      return ErrorResponse<CreateUserIfNotExistsResponse>("Email not found in token");
    }

    var result = await _userService.CreateUserIfNotExists(request, email, cancellationToken);
    return OkResponse(result, Messages.User.Created);
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
    var userId = GetCurrentUserId();
    var result = await _userService.GetCurrentUser(userId, cancellationToken);
    return OkResponse(result, Messages.User.Listed);
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
    if (string.IsNullOrEmpty(request.UserId) && string.IsNullOrEmpty(request.Slug))
    {
      return ErrorResponse<GetUserResponse>("Either UserId or Slug must be provided");
    }

    var result = await _userService.GetUser(request, cancellationToken);
    return OkResponse(result, Messages.User.Listed);
  }

  [HttpPut]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("update-slug")]
  [ActionName("UpdateUserSlug")]
  public async Task<ActionResult<ApiResponse<UpdateUserSlugResponse>>> UpdateUserSlug(
    UpdateUserSlugRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var userId = GetCurrentUserId();
    if (string.IsNullOrEmpty(userId))
    {
      return ErrorResponse<UpdateUserSlugResponse>(UserMessages.UserIdNotFoundInToken);
    }

    var result = await _userService.UpdateUserSlug(request, userId, cancellationToken);
    return OkResponse(result, Messages.User.Updated);
  }

  [HttpPost]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("generate-random-slug")]
  [ActionName("GenerateRandomSlug")]
  public async Task<ActionResult<ApiResponse<GenerateRandomSlugResponse>>> GenerateRandomSlug(
    GenerateRandomSlugRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _userService.GenerateRandomSlug(request, cancellationToken);
    return OkResponse(result, UserMessages.RandomSlugGeneratedSuccessfully);
  }

  #endregion
}
