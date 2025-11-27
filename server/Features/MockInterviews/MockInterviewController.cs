using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Common;
using RSPWebAPI.Features.Constants;
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
    return OkResponse(result, Messages.MockInterview.Created);
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
    return OkResponse(result, Messages.MockInterview.Deleted);
  }

  [HttpGet]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("get")]
  [ActionName("ListMockInterview")]
  public async Task<IActionResult> ListMockInterview(
    [FromQuery] ListMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    // Use cursor pagination if Cursor or PageSize is specified
    if (!string.IsNullOrEmpty(request.Cursor) || request.PageSize.HasValue)
    {
      var cursorResult = await _mockInterviewService.ListMockInterviewWithCursor(request, cancellationToken);
      return OkResponse(cursorResult, Messages.MockInterview.Listed);
    }

    var result = await _mockInterviewService.ListMockInterview(request, cancellationToken);
    return OkResponse(result, Messages.MockInterview.Listed);
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
    return OkResponse(result, Messages.MockInterview.Updated);
  }

  [HttpPut]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("update-custom-round-review")]
  [ActionName("UpdateCustomMockInterviewRoundReview")]
  public async Task<ActionResult<ApiResponse<UpdateMockInterviewRoundReviewResponse>>> UpdateCustomMockInterviewRoundReview(
    UpdateCustomMockInterviewRoundReviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    request.UserId = GetCurrentUserId();
    var result = await _mockInterviewService.UpdateCustomMockInterviewRoundReview(request, cancellationToken);
    return OkResponse(result, Messages.MockInterview.CustomRoundReviewUpdated);
  }

  [HttpPut]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("update-leetcode-round-review")]
  [ActionName("UpdateLeetcodeMockInterviewRoundReview")]
  public async Task<ActionResult<ApiResponse<UpdateMockInterviewRoundReviewResponse>>> UpdateLeetcodeMockInterviewRoundReview(
    UpdateLeetcodeMockInterviewRoundReviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    request.UserId = GetCurrentUserId();
    var result = await _mockInterviewService.UpdateLeetcodeMockInterviewRoundReview(request, cancellationToken);
    return OkResponse(result, Messages.MockInterview.LeetcodeRoundReviewUpdated);
  }

  #endregion
}
