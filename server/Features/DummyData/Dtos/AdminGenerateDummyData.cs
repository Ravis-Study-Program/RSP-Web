using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace RSPWebAPI.Features.DummyData.Dtos;

public record AdminGenerateDummyDataRequest
{
  [Required]
  public int NumberOfSeasons { get; set; }

  [Required]
  public int NumberOfUsers { get; set; }
}

public record AdminGenerateDummyDataResponse { }

public class AdminGenerateDummyDataRequestValidator
  : AbstractValidator<AdminGenerateDummyDataRequest>
{
  public AdminGenerateDummyDataRequestValidator()
  {
    RuleFor(c => c.NumberOfSeasons).GreaterThan(0);
    RuleFor(c => c.NumberOfUsers).GreaterThan(0);
  }
}
