using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Features.MockInterviews.Dtos;

public record UpdateCustomMockInterviewRoundReviewRequest
{
  [Required]
  public string CustomMockInterviewRoundId { get; set; } = string.Empty;
  
  [Required]
  public bool IsReviewed { get; set; }
  
  public string UserId { get; set; } = string.Empty;
}

public record UpdateLeetcodeMockInterviewRoundReviewRequest
{
  [Required]
  public string LeetcodeMockInterviewRoundId { get; set; } = string.Empty;
  
  [Required]
  public bool IsReviewed { get; set; }
  
  public string UserId { get; set; } = string.Empty;
}

public record UpdateMockInterviewRoundReviewResponse
{
  public string Message { get; set; } = string.Empty;
}