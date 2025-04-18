using System.ComponentModel.DataAnnotations;
using FluentValidation;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Enrollments.Dtos;

public record GetEnrollmentUsersRequest
{
  [Required]
  public string SeasonSlug { get; set; } = string.Empty;
}

public record EnrollmentUserDto
{
  [Required]
  public string DiscordId { get; set; } = string.Empty;

  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string Slug { get; set; } = string.Empty;

  [Required]
  public string Email { get; set; } = string.Empty;

  [Required]
  public SeasonRole Role { get; set; }

  [Required]
  public string ProfileImage { get; set; } = string.Empty;

  [Required]
  public SeasonStudentRolePromotion StudentRolePromotion { get; set; }
}

public record GetEnrollmentUsersResponse
{
  [Required]
  public IList<EnrollmentUserDto> EnrollmentUsers { get; set; } = new List<EnrollmentUserDto>();
}

public class GetEnrollmentUsersRequestValidator : AbstractValidator<GetEnrollmentUsersRequest>
{
  public GetEnrollmentUsersRequestValidator()
  {
    RuleFor(c => c.SeasonSlug).NotEmpty();
  }
}
