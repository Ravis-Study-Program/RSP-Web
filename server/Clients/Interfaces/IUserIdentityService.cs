using Auth0.ManagementApi.Models;

namespace RSPWebAPI.Clients.Interfaces
{
  public interface IUserIdentityService
  {
    Task<IList<User>?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task SendVerificationEmailAsync(string auth0UserId);
    Task AddMetadata(string auth0UserId, dynamic metadata);
    Task LinkAccountAsync(string userId, User user);
  }
}
