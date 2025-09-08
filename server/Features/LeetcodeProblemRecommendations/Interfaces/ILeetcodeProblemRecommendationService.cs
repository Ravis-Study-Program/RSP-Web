using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Features.LeetcodeProblemRecommendations.Dtos;

namespace RSPWebAPI.Features.LeetcodeProblemRecommendations.Interfaces;

public interface ILeetcodeProblemRecommendationService
{
  Task<GenerateLeetcodeProblemRecommendationResponse> GenerateLeetcodeProblemRecommendation(
    GenerateLeetcodeProblemRecommendationRequest request,
    CancellationToken cancellationToken = default
  );
}
