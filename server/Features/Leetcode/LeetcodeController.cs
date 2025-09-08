using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Common;
using RSPWebAPI.Features.Leetcode.Dtos;
using RSPWebAPI.Features.Leetcodes.Interfaces;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.Leetcodes;

[ApiController]
[Route("api/v1/leetcode")]
public class LeetcodeController : BaseController
{
  private readonly ILeetcodeService _leetcodeService;

  public LeetcodeController(ILeetcodeService leetcodeService)
  {
    _leetcodeService = leetcodeService;
  }

  #region Routes

  [HttpGet]
  // [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/populate-leetcode-questions")]
  [ActionName("AdminPopulateLeetcodeQuestions")]
  public async Task<
    ActionResult<ApiResponse<AdminPopulateLeetcodeQuestionsResponse>>
  > AdminPopulateLeetcodeQuestions(
    [FromQuery] AdminPopulateLeetcodeQuestionsRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _leetcodeService.AdminPopulateLeetcodeQuestions(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpGet]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("list-leetcode-problems")]
  [ActionName("ListLeetcodeProblems")]
  public async Task<ActionResult<ApiResponse<ListLeetcodeProblemsResponse>>> ListLeetcodeProblems(
    [FromQuery] ListLeetcodeProblemsRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _leetcodeService.ListLeetcodeProblems(request, cancellationToken);
    return OkResponse(result);
  }

  #endregion
}
