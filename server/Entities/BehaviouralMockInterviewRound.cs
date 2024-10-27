using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class BehaviouralMockInterviewRound
{
  [Required] public Guid BehaviouralMockInterviewRoundId { get; set; }
  [Required] public Guid MockInterviewId { get; set; }
  [Required] public int BehavioralScore { get; set; }
}
