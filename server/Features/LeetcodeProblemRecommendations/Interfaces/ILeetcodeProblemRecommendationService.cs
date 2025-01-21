using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Features.LeetcodeProblemRecommendations.Dtos;

namespace RSPWebAPI.Features.LeetcodeProblemRecommendations.Interfaces;

public interface ILeetcodeProblemRecommendationService
{
  Task<
    IServiceResponse<GenerateLeetcodeProblemRecommendationResponse>
  > GenerateLeetcodeProblemRecommendation(
    GenerateLeetcodeProblemRecommendationRequest request,
    CancellationToken cancellationToken = default
  );
}
