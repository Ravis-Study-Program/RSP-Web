using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Common;
using RSPWebAPI.Features.DummyData.Dtos;
using RSPWebAPI.Features.DummyData.Interfaces;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.DummyData;

[ApiController]
[Route("api/v1/dummy-data")]
public class DummyDataController : BaseController
{
  private readonly IDummyDataService _dummyDataService;

  public DummyDataController(IDummyDataService dummyDataService)
  {
    _dummyDataService = dummyDataService;
  }

  #region Routes

  [HttpPost]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/generate")]
  [ActionName("AdminGenerateDummyData")]
  public async Task<
    ActionResult<ApiResponse<AdminGenerateDummyDataResponse>>
  > AdminGenerateDummyData(
    AdminGenerateDummyDataRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var response = await _dummyDataService.AdminGenerateDummyData(request, cancellationToken);
    return HandleResponse(response);
  }

  #endregion
}
