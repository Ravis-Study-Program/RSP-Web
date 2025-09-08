using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Common;
using RSPWebAPI.Features.Mentorships.Dtos;
using RSPWebAPI.Features.Mentorships.Interfaces;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.Mentorships;

[ApiController]
[Route("api/v1/mentorships")]
public class MentorshipController : BaseController
{
  private readonly IMentorshipService _mentorshipService;

  public MentorshipController(IMentorshipService seasonService)
  {
    _mentorshipService = seasonService;
  }

  #region Routes

  [HttpPost]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/create")]
  [ActionName("AdminCreateMentorship")]
  public async Task<ActionResult<ApiResponse<AdminCreateMentorshipResponse>>> AdminCreateMentorship(
    AdminCreateMentorshipRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _mentorshipService.CreateAdminMentorship(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpDelete]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/delete")]
  [ActionName("AdminDeleteMentorship")]
  public async Task<ActionResult<ApiResponse<AdminDeleteMentorshipResponse>>> AdminDeleteMentorship(
    AdminDeleteMentorshipRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _mentorshipService.DeleteAdminMentorship(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpGet]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/get")]
  [ActionName("AdminListMentorship")]
  public async Task<ActionResult<ApiResponse<AdminListMentorshipResponse>>> AdminListMentorship(
    [FromQuery] AdminListMentorshipRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _mentorshipService.ListAdminMentorship(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpPut]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/update")]
  [ActionName("AdminUpdateMentorship")]
  public async Task<ActionResult<ApiResponse<AdminUpdateMentorshipResponse>>> AdminUpdateMentorship(
    AdminUpdateMentorshipRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _mentorshipService.UpdateAdminMentorship(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpGet]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("get-current-user-mentees")]
  [ActionName("GetCurrentUserMentees")]
  public async Task<
    ActionResult<ApiResponse<GetCurrentUserMenteesListResponse>>
  > GetCurrentUserMenteesList(
    [FromQuery] GetCurrentUserMenteesListRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _mentorshipService.GetCurrentUserMenteesList(request, cancellationToken);
    return OkResponse(result);
  }

  #endregion
}
