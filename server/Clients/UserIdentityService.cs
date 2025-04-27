using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Newtonsoft.Json;
using RSPWebAPI.Clients.Interfaces;

public class UserIdentityService : IUserIdentityService
{
  private readonly AppConfiguration _config;
  private ManagementApiClient? _client;
  private DateTime _tokenExpiry = DateTime.MinValue;
  private string? _accessToken;

  public UserIdentityService(AppConfiguration config)
  {
    _config = config;
  }

  private async Task<ManagementApiClient> GetClientAsync()
  {
    if (_client != null && DateTime.UtcNow < _tokenExpiry)
      return _client;

    var token = await GetManagementTokenAsync();

    _client = new ManagementApiClient(token, new Uri($"{_config.Auth0Domain}/api/v2"));
    return _client;
  }

  private async Task<string> GetManagementTokenAsync()
  {
    var authClient = new AuthenticationApiClient(new Uri(_config.Auth0Domain));

    var tokenRequest = new ClientCredentialsTokenRequest
    {
      ClientId = _config.Auth0ClientId,
      ClientSecret = _config.Auth0ClientSecret,
      Audience = $"{_config.Auth0Domain}/api/v2/",
    };

    var tokenResponse = await authClient.GetTokenAsync(tokenRequest);

    _accessToken = tokenResponse.AccessToken;
    _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);
    return _accessToken;
  }

  public async Task<IList<User>?> GetUserByEmailAsync(
    string email,
    CancellationToken cancellationToken
  )
  {
    var client = await GetClientAsync();
    var users = await client.Users.GetUsersByEmailAsync(
      email,
      cancellationToken: cancellationToken
    );
    return users;
  }

  public async Task SendVerificationEmailAsync(string userId)
  {
    if (string.IsNullOrWhiteSpace(userId))
    {
      throw new ArgumentException("User ID must be provided", nameof(userId));
    }

    var client = await GetClientAsync();
    var request = new VerifyEmailJobRequest { UserId = userId };
    await client.Jobs.SendVerificationEmailAsync(request);
  }

  public async Task LinkAccountAsync(string userId, User user)
  {
    var client = await GetClientAsync();
    await client.Users.LinkAccountAsync(
      userId,
      new UserAccountLinkRequest
      {
        Provider = user.Identities.First().Provider,
        UserId = user.Identities.First().UserId,
      }
    );
  }
}
