using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.LeetcodeProblemRecommendations.Dtos;

public record GenerateLeetcodeProblemRecommendationRequest
{
  [Required]
  public string UserId { get; set; } = string.Empty;
}

public record GenerateLeetcodeProblemRecommendationResponse
{
  [Required]
  public string LeetcodeProblemRecommendationId { get; set; } = string.Empty;
}

public class GenerateLeetcodeProblemRecommendationRequestValidator
  : AbstractValidator<GenerateLeetcodeProblemRecommendationRequest>
{
  public GenerateLeetcodeProblemRecommendationRequestValidator()
  {
    RuleFor(c => c.UserId).NotEmpty();
  }
}
