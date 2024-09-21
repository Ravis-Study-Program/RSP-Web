namespace RSPWebAPI.Contracts;

public class CreateUserRequest
{
    public string DiscordId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    
    public string ProfileImage { get; set; } = string.Empty;
}

public class CreateUserResponse
{
    public Guid UserId { get; set; }
    
    public string DiscordId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    
    public string ProfileImage { get; set; } = string.Empty;
}
