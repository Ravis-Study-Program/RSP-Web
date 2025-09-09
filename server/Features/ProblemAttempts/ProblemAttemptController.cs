using Microsoft.AspNetCore.Mvc;
using RSPWebAPI.Common;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.ProblemAttempts.Dtos;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.ProblemAttempts;

[ApiController]
[Route("api/v1/problem-attempts")]
public class ProblemAttemptController : BaseController
{
  private readonly IProblemAttemptService _problemAttemptService;

  public ProblemAttemptController(IProblemAttemptService problemAttemptService)
  {
    _problemAttemptService = problemAttemptService;
  }

  #region Routes

  [HttpPost]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("create")]
  [ActionName("CreateProblemAttempt")]
  public async Task<ActionResult<ApiResponse<CreateProblemAttemptResponse>>> CreateProblemAttempt(
    CreateProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var email = GetCurrentUserEmail() ?? "";
    request.Email = email;
    var result = await _problemAttemptService.CreateProblemAttempt(request, cancellationToken);
    return OkResponse(result, Messages.ProblemAttempt.Created);
  }

  [HttpDelete]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("delete")]
  [ActionName("DeleteProblemAttempt")]
  public async Task<ActionResult<ApiResponse<DeleteProblemAttemptResponse>>> DeleteProblemAttempt(
    DeleteProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _problemAttemptService.DeleteProblemAttempt(request, cancellationToken);
    return OkResponse(result, Messages.ProblemAttempt.Deleted);
  }

  [HttpGet]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("get")]
  [ActionName("ListProblemAttempt")]
  public async Task<ActionResult<ApiResponse<ListProblemAttemptResponse>>> ListProblemAttempt(
    [FromQuery] ListProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _problemAttemptService.ListProblemAttempt(request, cancellationToken);
    return OkResponse(result, Messages.ProblemAttempt.Listed);
  }

  [HttpPut]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("update")]
  [ActionName("UpdateProblemAttempt")]
  public async Task<ActionResult<ApiResponse<UpdateProblemAttemptResponse>>> UpdateProblemAttempt(
    UpdateProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var email = GetCurrentUserEmail() ?? "";
    request.Email = email;
    var result = await _problemAttemptService.UpdateProblemAttempt(request, cancellationToken);
    return OkResponse(result, Messages.ProblemAttempt.Updated);
  }

  #endregion
}
