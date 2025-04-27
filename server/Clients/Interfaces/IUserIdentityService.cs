using Auth0.ManagementApi.Models;

namespace RSPWebAPI.Clients.Interfaces
{
  public interface IUserIdentityService
  {
    Task<IList<User>?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task SendVerificationEmailAsync(string userId);
    Task LinkAccountAsync(string userId, User user);
  }
}
