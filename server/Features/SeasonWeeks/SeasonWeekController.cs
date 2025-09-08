using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Common;
using RSPWebAPI.Features.SeasonWeeks.Dtos;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.SeasonWeeks;

[ApiController]
[Route("api/v1/season-weeks")]
public class SeasonWeekController : BaseController
{
  private readonly ISeasonWeekService _seasonWeekService;

  public SeasonWeekController(ISeasonWeekService seasonWeekService)
  {
    _seasonWeekService = seasonWeekService;
  }

  #region Routes

  [HttpPost]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/create")]
  [ActionName("AdminCreateSeasonWeek")]
  public async Task<ActionResult<ApiResponse<AdminCreateSeasonWeekResponse>>> AdminCreateSeasonWeek(
    AdminCreateSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _seasonWeekService.CreateAdminSeasonWeek(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpDelete]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/delete")]
  [ActionName("AdminDeleteSeasonWeek")]
  public async Task<ActionResult<ApiResponse<AdminDeleteSeasonWeekResponse>>> AdminDeleteSeasonWeek(
    AdminDeleteSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _seasonWeekService.DeleteAdminSeasonWeek(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpGet]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/get")]
  [ActionName("AdminListSeasonWeek")]
  public async Task<ActionResult<ApiResponse<AdminListSeasonWeekResponse>>> AdminListSeasonWeek(
    [FromQuery] AdminListSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _seasonWeekService.ListAdminSeasonWeek(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpPut]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/update")]
  [ActionName("AdminUpdateSeasonWeek")]
  public async Task<ActionResult<ApiResponse<AdminUpdateSeasonWeekResponse>>> AdminUpdateSeasonWeek(
    AdminUpdateSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _seasonWeekService.UpdateAdminSeasonWeek(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpGet]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("get")]
  [ActionName("GetSeasonWeeksBySeasonSlug")]
  public async Task<
    ActionResult<ApiResponse<GetSeasonWeeksBySeasonSlugResponse>>
  > GetSeasonWeeksBySeasonSlug(
    [FromQuery] GetSeasonWeeksBySeasonSlugRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _seasonWeekService.GetSeasonWeeksBySeasonSlug(request, cancellationToken);
    return OkResponse(result);
  }

  #endregion
}
