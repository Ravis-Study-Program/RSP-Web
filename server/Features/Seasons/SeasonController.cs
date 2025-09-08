using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Common;
using RSPWebAPI.Features.Seasons.Dtos;
using RSPWebAPI.Features.Seasons.Interfaces;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.Seasons;

[ApiController]
[Route("api/v1/seasons")]
public class SeasonController : BaseController
{
  private readonly ISeasonService _seasonService;

  public SeasonController(ISeasonService seasonService)
  {
    _seasonService = seasonService;
  }

  #region Routes

  [HttpPost]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/create")]
  [ActionName("AdminCreateSeason")]
  public async Task<ActionResult<ApiResponse<AdminCreateSeasonResponse>>> AdminCreateSeason(
    AdminCreateSeasonRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _seasonService.CreateAdminSeason(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpDelete]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/delete")]
  [ActionName("AdminDeleteSeason")]
  public async Task<ActionResult<ApiResponse<AdminDeleteSeasonResponse>>> AdminDeleteSeason(
    AdminDeleteSeasonRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _seasonService.DeleteAdminSeason(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpGet]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/get")]
  [ActionName("AdminListSeason")]
  public async Task<ActionResult<ApiResponse<AdminListSeasonResponse>>> AdminListSeason(
    [FromQuery] AdminListSeasonRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _seasonService.ListAdminSeason(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpPut]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/update")]
  [ActionName("AdminUpdateSeason")]
  public async Task<ActionResult<ApiResponse<AdminUpdateSeasonResponse>>> AdminUpdateSeason(
    AdminUpdateSeasonRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _seasonService.UpdateAdminSeason(request, cancellationToken);
    return OkResponse(result);
  }

  #endregion
}
