using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Common;
using RSPWebAPI.Features.MockInterviews.Dtos;
using RSPWebAPI.Features.MockInterviews.Interfaces;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.MockInterviews;

[ApiController]
[Route("api/v1/mock-interviews")]
public class MockInterviewController : BaseController
{
  private readonly IMockInterviewService _mockInterviewService;

  public MockInterviewController(IMockInterviewService mockInterviewService)
  {
    _mockInterviewService = mockInterviewService;
  }

  #region Routes

  [HttpPost]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("create")]
  [ActionName("CreateMockInterview")]
  public async Task<ActionResult<ApiResponse<CreateMockInterviewResponse>>> CreateMockInterview(
    CreateMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _mockInterviewService.CreateMockInterview(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpDelete]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("delete")]
  [ActionName("DeleteMockInterview")]
  public async Task<ActionResult<ApiResponse<DeleteMockInterviewResponse>>> DeleteMockInterview(
    DeleteMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _mockInterviewService.DeleteMockInterview(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpGet]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("get")]
  [ActionName("ListMockInterview")]
  public async Task<ActionResult<ApiResponse<ListMockInterviewResponse>>> ListMockInterview(
    [FromQuery] ListMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _mockInterviewService.ListMockInterview(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpPut]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("update")]
  [ActionName("UpdateMockInterview")]
  public async Task<ActionResult<ApiResponse<UpdateMockInterviewResponse>>> UpdateMockInterview(
    UpdateMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _mockInterviewService.UpdateMockInterview(request, cancellationToken);
    return OkResponse(result);
  }

  #endregion
}
