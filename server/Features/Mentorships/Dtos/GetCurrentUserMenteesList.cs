using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Mentorships.Dtos;

public record GetCurrentUserMenteesListRequest
{
  [Required]
  public string Email { get; set; } = string.Empty;

  [Required]
  public string SeasonSlug { get; set; } = string.Empty;
}

public record GetCurrentUserMenteesListResponse
{
  [Required]
  public IList<MentorshipResponse> Mentorships { get; init; } = new List<MentorshipResponse>();
}

public class GetCurrentUserMenteesListRequestValidator
  : AbstractValidator<GetCurrentUserMenteesListRequest>
{
  public GetCurrentUserMenteesListRequestValidator()
  {
    RuleFor(c => c.Email).NotEmpty().EmailAddress();
    RuleFor(c => c.SeasonSlug).NotEmpty();
  }
}
