namespace RSPWebAPI.Contracts;

public class GetUserRequest
{
    public Guid UserId { get; set; }
}

public class GetUserResponse
{
    public string DiscordId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    
    public string ProfileImage { get; set; } = string.Empty;
}