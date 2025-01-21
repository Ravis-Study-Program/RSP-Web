using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Features.Constants;

namespace RSPWebAPI.Features.SeasonWeeks.Dtos;

public record AdminCreateSeasonWeekRequest
{
  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public int WeekNumber { get; set; }

  [Required]
  public DateTime StartDate { get; set; }

  [Required]
  public DateTime EndDate { get; set; }
}

public record AdminCreateSeasonWeekResponse
{
  [Required]
  public string SeasonWeekId { get; set; } = string.Empty;
}

public class AdminCreateSeasonWeekRequestValidator : AbstractValidator<AdminCreateSeasonWeekRequest>
{
  public AdminCreateSeasonWeekRequestValidator()
  {
    RuleFor(c => c.SeasonId).NotEmpty();
    RuleFor(c => c.WeekNumber).GreaterThan(0);
    RuleFor(c => c.StartDate).NotEmpty();
    RuleFor(c => c.EndDate)
      .NotEmpty()
      .GreaterThan(c => c.StartDate)
      .WithMessage(Message.EndDateMustBeGreaterThanStartDate);
  }
}
