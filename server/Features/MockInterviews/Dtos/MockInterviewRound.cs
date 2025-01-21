using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Features.MockInterviews.Dtos;

public record MockInterviewRoundDto
{
  public LeetcodeMockInterviewRoundDto? LeetcodeMockInterviewRound { get; set; }
  public BehaviouralMockInterviewRoundDto? BehaviouralMockInterviewRound { get; set; }
  public CustomMockInterviewRoundDto? CustomMockInterviewRound { get; set; }

  // For update
  public string? MockInterviewRoundId { get; set; }
}

public record LeetcodeMockInterviewRoundDto
{
  [Required]
  public int ConfirmQuestionScore { get; set; }

  [Required]
  public int AlgorithmDesignScore { get; set; }

  [Required]
  public int ComplexityAnalysisScore { get; set; }

  [Required]
  public int CodingScore { get; set; }

  [Required]
  public int TestingScore { get; set; }

  [Required]
  public string LeetcodeProblemId { get; set; } = string.Empty;
}

public record BehaviouralMockInterviewRoundDto
{
  [Required]
  public int BehavioralScore { get; set; }
}

public record CustomMockInterviewRoundDto
{
  [Required]
  public string Content { get; set; } = string.Empty;

  [Required]
  public string Link { get; set; } = string.Empty;

  [Required]
  public int Score { get; set; }
}
