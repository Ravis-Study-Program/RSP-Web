using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RSPWebAPI.Common;

public class AuthAttribute : Attribute, IAuthorizationFilter
{
  public void OnAuthorization(AuthorizationFilterContext context)
  {
    var user = context.HttpContext.User;

    if (!user.Identity?.IsAuthenticated ?? true)
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
    }
  }
}
