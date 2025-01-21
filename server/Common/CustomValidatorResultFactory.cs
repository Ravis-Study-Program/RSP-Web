using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace RSPWebAPI.Common;

public class CustomValidatorResultFactory : IFluentValidationAutoValidationResultFactory
{
  public IActionResult CreateActionResult(
    ActionExecutingContext context,
    ValidationProblemDetails? validationProblemDetails
  )
  {
    return new BadRequestObjectResult(
      new { Title = "Validation errors", ValidationErrors = validationProblemDetails?.Errors }
    );
  }
};
