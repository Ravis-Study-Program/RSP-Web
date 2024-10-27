using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class CustomMockInterviewRound
{
  [Required] public Guid CustomMockInterviewRoundId { get; set; }
  [Required] public Guid MockInterviewId { get; set; }
  [Required] public string Content { get; set; }
  [Required] public string Link { get; set; }
  [Required] public int Score { get; set; }
}
