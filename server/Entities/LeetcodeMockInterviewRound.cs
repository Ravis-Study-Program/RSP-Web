using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class LeetcodeMockInterviewRound
{
  [Required] public Guid LeetcodeMockInterviewRoundId { get; set; }
  [Required] public Guid MockInterviewId { get; set; }
  [Required] public int ConfirmQuestionScore { get; set; }
  [Required] public int AlgorithmDesignScore { get; set; }
  [Required] public int ComplexityAnalysisScore { get; set; }
  [Required] public int CodingScore { get; set; }
  [Required] public int TestingScore { get; set; }
  [Required] public Guid LeetcodeProblemId { get; set; }
  [Required] public LeetcodeProblem LeetcodeProblem { get; set; }
}
