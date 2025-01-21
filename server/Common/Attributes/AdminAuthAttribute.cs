using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;

namespace RSPWebAPI.Common;

public class AdminAuthAttribute : Attribute, IAsyncAuthorizationFilter
{
  private readonly ApplicationDbContext _dbContext;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public AdminAuthAttribute(
    ApplicationDbContext dbContext,
    IHttpContextAccessor httpContextAccessor
  )
  {
    _dbContext = dbContext;
    _httpContextAccessor = httpContextAccessor;
  }

  public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    // Check if the user is authenticated
    if (httpContext?.User?.Identity?.IsAuthenticated != true)
    {
      context.Result = new JsonResult(
        new
        {
          StatusCode = (int)HttpStatusCode.Unauthorized,
          Error = "You don't have the required authorization to access the resource.",
        }
      )
      {
        StatusCode = (int)HttpStatusCode.Unauthorized,
      };
      return;
    }

    // Get the user's email
    var email = httpContext.User.Identity?.Name;
    if (string.IsNullOrEmpty(email))
    {
      context.Result = new JsonResult(
        new
        {
          StatusCode = (int)HttpStatusCode.Unauthorized,
          Error = "You don't have the required authorization to access the resource.",
        }
      )
      {
        StatusCode = (int)HttpStatusCode.Unauthorized,
      };
      return;
    }

    // Check if the user is an admin
    var isAdmin = await _dbContext
      .Users.AsNoTracking()
      .AnyAsync(u => u.Email == email && u.IsAdmin);

    if (!isAdmin)
    {
      context.Result = new JsonResult(
        new
        {
          StatusCode = (int)HttpStatusCode.Forbidden,
          Error = "Admin privileges are required to access this resource.",
        }
      )
      {
        StatusCode = (int)HttpStatusCode.Forbidden,
      };
    }
  }
}
