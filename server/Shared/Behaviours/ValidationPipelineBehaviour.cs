using System.Net;
using FluentValidation;
using MediatR;

namespace RSPWebAPI.Shared.Behaviours;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
  where TResponse : ApiResult
{
  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    if (!validators?.Any() ?? true)
    {
      return await next();
    }

    var errors = validators?
                 .Select(validator => validator.Validate(request))
                 .SelectMany(validationResult => validationResult.Errors)
                 .Where(validationFailure => validationFailure is not null)
                 .Select(failure => new ValidationError
                 {
                   Property = failure.PropertyName,
                   Message = failure.ErrorMessage
                 })
                 .Distinct()
                 .ToArray();

    if (errors?.Length > 0)
    {
      return CreateValidationResult<TResponse>(errors);
    }

    return await next();
  }

  private static TResult CreateValidationResult<TResult>(ValidationError[] errors)
    where TResult : ApiResult
  {
    // Handle non-generic ApiResult
    if (typeof(TResult) == typeof(ApiResult))
    {
      return (TResult)(object)ValidationResult.WithErrors(errors);
    }

    // Handle generic ApiResult<T>
    if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(ApiResult<>))
    {
      var resultType = typeof(ValidationResult<>).MakeGenericType(typeof(TResult).GenericTypeArguments[0]);
      var methodInfo = resultType.GetMethod(nameof(ValidationResult.WithErrors));
      if (methodInfo is null)
      {
        throw new InvalidOperationException($"Method 'WithErrors' not found in {resultType.Name}");
      }

      var validationResult = methodInfo.Invoke(null, new object[] { errors });
      if (validationResult is TResult typedResult)
      {
        return typedResult;
      }

      throw new InvalidOperationException($"Failed to create validation result for type: {typeof(TResult)}");
    }

    throw new InvalidOperationException($"Unsupported result type: {typeof(TResult)}");
  }
}

public sealed class ValidationError : ApiError
{
  public string? Property { get; set; }
}

public sealed class ValidationResult : ApiResult
{
  private ValidationResult(ValidationError[] errors)
    : base(HttpStatusCode.BadRequest, new ApiError("There are some validation errors.", errors))
  {
  }

  public static ValidationResult WithErrors(ValidationError[] errors)
  {
    return new ValidationResult(errors);
  }
}

public sealed class ValidationResult<TValue> : ApiResult<TValue>
{
  private ValidationResult(ValidationError[] errors)
    : base(default, HttpStatusCode.BadRequest, new ApiError("There are some validation errors.", errors))
  {
  }

  public static ValidationResult<TValue> WithErrors(ValidationError[] errors)
  {
    return new ValidationResult<TValue>(errors);
  }
}