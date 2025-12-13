using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.ProblemAttempts.Dtos;

public record ListProblemAttemptRequest
{
  [Required]
  public IList<string> UserIds { get; set; } = new List<string>();

  [Required]
  public bool IncludeLeetcode { get; set; }

  [Required]
  public bool IncludeCustom { get; set; }

  public string? SeasonId { get; set; }
}

public record ListProblemAttemptResponse
{
  [Required]
  public IList<ProblemAttemptEntity> ProblemAttempts { get; init; } =
    new List<ProblemAttemptEntity>();
}

public class ListProblemAttemptRequestValidator : AbstractValidator<ListProblemAttemptRequest>
{
  public ListProblemAttemptRequestValidator()
  {
    RuleFor(c => c.UserIds).NotEmpty().WithMessage("At least one user id must be provided.");
  }
}