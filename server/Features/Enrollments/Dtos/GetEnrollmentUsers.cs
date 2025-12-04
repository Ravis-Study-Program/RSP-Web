using System.ComponentModel.DataAnnotations;
using RSPWebAPI.Entities;

namespace RSPWebAPI.Features.Enrollments.Dtos;

public record GetEnrollmentUsersRequest
{
  public string? SeasonSlug { get; set; }
  public bool? OnlyGraduates { get; set; }
}

public record EnrollmentUserDto
{
  [Required]
  public string UserId { get; set; } = string.Empty;

  [Required]
  public string Name { get; set; } = string.Empty;

  [Required]
  public string Slug { get; set; } = string.Empty;

  public string? ProfileImage { get; set; }

  public SeasonRole? Role { get; set; }

  public SeasonStudentRolePromotion? StudentRolePromotion { get; set; }
  
  [Required]
  public bool IsGraduate { get; set; }
}

public record GetEnrollmentUsersResponse
{
  [Required]
  public IList<EnrollmentUserDto> EnrollmentUsers { get; set; } = new List<EnrollmentUserDto>();
}
