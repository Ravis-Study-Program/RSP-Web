using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class MockInterviewRound
{
  [Required] public Guid MockInterviewRoundId { get; set; }
  [Required] public Guid MockInterviewId { get; set; }
  public bool IsReviewedByInterviewee { get; set; } = false;
  public string IntervieweeComment { get; set; } = string.Empty;
  
  public Guid? BehaviouralMockInterviewRoundId { get; set; }
  public BehaviouralMockInterviewRound? BehaviouralMockInterviewRound { get; set; }
  
  public Guid? LeetcodeMockInterviewRoundId { get; set; }
  public LeetcodeMockInterviewRound? LeetcodeMockInterviewRound { get; set; }
  
  public Guid? CustomMockInterviewRoundId { get; set; }
  public CustomMockInterviewRound? CustomMockInterviewRound { get; set; }
}
