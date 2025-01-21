using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Features.Constants;

namespace RSPWebAPI.Features.SeasonWeeks.Dtos;

public record AdminUpdateSeasonWeekRequest
{
  [Required]
  public string SeasonWeekId { get; set; } = string.Empty;

  [Required]
  public int WeekNumber { get; set; }

  [Required]
  public DateTime StartDate { get; set; }

  [Required]
  public DateTime EndDate { get; set; }
}

public record AdminUpdateSeasonWeekResponse { }

public class AdminUpdateSeasonWeekRequestValidator : AbstractValidator<AdminUpdateSeasonWeekRequest>
{
  public AdminUpdateSeasonWeekRequestValidator()
  {
    RuleFor(c => c.SeasonWeekId).NotEmpty();
    RuleFor(c => c.WeekNumber).GreaterThan(0);
    RuleFor(c => c.StartDate).NotEmpty();
    RuleFor(c => c.EndDate)
      .NotEmpty()
      .GreaterThan(c => c.StartDate)
      .WithMessage(Message.EndDateMustBeGreaterThanStartDate);
  }
}
