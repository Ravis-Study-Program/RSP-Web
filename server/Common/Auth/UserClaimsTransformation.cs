using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using RSPWebAPI.Features.Users.Interfaces;

namespace RSPWebAPI.Common.Auth;

public class UserClaimsTransformation : IClaimsTransformation
{
  private readonly IServiceProvider _serviceProvider;

  public UserClaimsTransformation(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
  {
    if (!principal.Identity?.IsAuthenticated == true)
    {
      return principal;
    }

    // Check if we already have the custom claims
    if (principal.FindFirst($"{Database.Constants.Domain}userId") != null)
    {
      return principal;
    }

    var email = principal.FindFirst("email")?.Value ?? principal.Identity?.Name;
    if (string.IsNullOrEmpty(email))
    {
      return principal;
    }

    using var scope = _serviceProvider.CreateScope();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

    try
    {
      var user = await userService.GetUserAsync(email: email);
      if (user != null)
      {
        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim($"{Database.Constants.Domain}userId", user.UserId));
        identity.AddClaim(
          new Claim($"{Database.Constants.Domain}isAdmin", user.IsAdmin.ToString().ToLower())
        );

        principal.AddIdentity(identity);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error in ClaimsTransformation: {ex.Message}");
    }

    return principal;
  }
}
