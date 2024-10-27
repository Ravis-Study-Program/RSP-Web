using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class MockInterview
{
  [Required] public Guid MockInterviewId { get; set; }
  [Required] public bool IsPass { get; set; }
  [Required] public Guid InterviewerUserId { get; set; }
  [Required] public User Interviewer { get; set; }
  [Required] public Guid IntervieweeUserId { get; set; }
  [Required] public User Interviewee { get; set; }
  [Required] public DateTime StartDate { get; set; }
  [Required] public int TimeTakenInMinutes { get; set; }
  public Guid? EnrollmentId { get; set; }
  public Enrollment? Enrollment { get; set; }
  public ICollection<MockInterviewRound> MockInterviewRounds { get; set; }
}
