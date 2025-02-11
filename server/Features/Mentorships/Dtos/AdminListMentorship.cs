using System.ComponentModel.DataAnnotations;
using RSPWebAPI.Entities;
using RSPWebAPI.Migrations;

namespace RSPWebAPI.Features.Mentorships.Dtos;

public record AdminListMentorshipRequest { };

public record AdminListMentorshipResponse
{
  [Required]
  public IList<MentorshipResponse> Mentorships { get; init; } = new List<MentorshipResponse>();
}

public record MentorshipResponse
{
  [Required]
  public string SeasonId { get; set; } = string.Empty;

  [Required]
  public string MentorshipId { get; set; } = string.Empty;

  [Required]
  public string SeasonName { get; set; } = string.Empty;

  [Required]
  public string SeasonSlug { get; set; } = string.Empty;

  [Required]
  public string MentorEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string MentorName { get; set; } = string.Empty;

  [Required]
  public string MentorEmail { get; set; } = string.Empty;

  [Required]
  public string MenteeEnrollmentId { get; set; } = string.Empty;

  [Required]
  public string MenteeName { get; set; } = string.Empty;

  [Required]
  public string MenteeEmail { get; set; } = string.Empty;

  [Required]
  public SeasonStudentRolePromotion StudentRolePromotion { get; set; }
}
