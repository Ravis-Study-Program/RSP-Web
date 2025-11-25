using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.ProblemAttempts.Dtos;

public record ListProblemAttemptRequest : PagedRequest
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
  public PagedResponse<ProblemAttemptEntity> Result { get; init; } = PagedResponse<ProblemAttemptEntity>.Create(
    new List<ProblemAttemptEntity>(),
    0,
    1,
    10
  );
}

public class ListProblemAttemptRequestValidator : AbstractValidator<ListProblemAttemptRequest>
{
  public ListProblemAttemptRequestValidator()
  {
    Include(new PagedRequestValidator());
    RuleFor(c => c.UserIds).NotEmpty().WithMessage("At least one user id must be provided.");
  }
}
