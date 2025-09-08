using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Common;
using RSPWebAPI.Features.LeetcodeProblemRecommendations.Dtos;
using RSPWebAPI.Features.LeetcodeProblemRecommendations.Interfaces;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.LeetcodeProblemRecommendations;

[ApiController]
[Route("api/v1/leetcode-recommendations")]
public class LeetcodeProblemRecommendationController : BaseController
{
  private readonly ILeetcodeProblemRecommendationService _leetcodeProblemRecommendationService;

  public LeetcodeProblemRecommendationController(
    ILeetcodeProblemRecommendationService leetcodeProblemRecommendationService
  )
  {
    _leetcodeProblemRecommendationService = leetcodeProblemRecommendationService;
  }

  #region Routes

  [HttpPost]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("generate")]
  [ActionName("GenerateLeetcodeProblemRecommendation")]
  public async Task<
    ActionResult<ApiResponse<GenerateLeetcodeProblemRecommendationResponse>>
  > GenerateLeetcodeProblemRecommendation(
    GenerateLeetcodeProblemRecommendationRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _leetcodeProblemRecommendationService.GenerateLeetcodeProblemRecommendation(
      request,
      cancellationToken
    );
    return OkResponse(result);
  }

  #endregion
}
