using System.ComponentModel.DataAnnotations;

namespace RSPWebAPI.Features.Users.Dtos;

public record GenerateRandomSlugRequest { }

public record GenerateRandomSlugResponse
{
  [Required]
  public string Slug { get; set; } = string.Empty;
}
