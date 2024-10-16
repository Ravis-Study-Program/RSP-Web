using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Entities;

public class CustomProblem
{
  [Required] public Guid CustomProblemId { get; set; }
  [Required] public string Difficulty { get; set; }
  [Required] public string Question { get; set; }

  [Required] public Guid ProblemId { get; set; }
  [Required] public Problem Problem { get; set; }
}