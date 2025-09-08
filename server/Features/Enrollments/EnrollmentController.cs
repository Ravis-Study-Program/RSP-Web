using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RSPWebAPI.Common;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Dtos;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.Enrollments;

[ApiController]
[Route("api/v1/enrollments")]
public class EnrollmentController : BaseController
{
  private readonly IEnrollmentService _enrollmentService;

  public EnrollmentController(IEnrollmentService seasonService)
  {
    _enrollmentService = seasonService;
  }

  #region Routes

  [HttpPost]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/create")]
  [ActionName("AdminCreateEnrollment")]
  public async Task<ActionResult<ApiResponse<AdminCreateEnrollmentResponse>>> AdminCreateEnrollment(
    AdminCreateEnrollmentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _enrollmentService.CreateAdminEnrollment(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpDelete]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/delete")]
  [ActionName("AdminDeleteEnrollment")]
  public async Task<ActionResult<ApiResponse<AdminDeleteEnrollmentResponse>>> AdminDeleteEnrollment(
    AdminDeleteEnrollmentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _enrollmentService.DeleteAdminEnrollment(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpGet]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/get")]
  [ActionName("AdminListEnrollment")]
  public async Task<ActionResult<ApiResponse<AdminListEnrollmentResponse>>> AdminListEnrollment(
    [FromQuery] AdminListEnrollmentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _enrollmentService.ListAdminEnrollment(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpPut]
  [ServiceFilter(typeof(AdminAuthAttribute))]
  [Route("admin/update")]
  [ActionName("AdminUpdateEnrollment")]
  public async Task<ActionResult<ApiResponse<AdminUpdateEnrollmentResponse>>> AdminUpdateEnrollment(
    AdminUpdateEnrollmentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _enrollmentService.UpdateAdminEnrollment(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpGet]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("get-user-enrollments")]
  [ActionName("GetUserEnrollments")]
  public async Task<ActionResult<ApiResponse<GetUserEnrollmentsResponse>>> GetUserEnrollments(
    string? email,
    CancellationToken cancellationToken = default
  )
  {
    if (email.IsNullOrEmpty())
    {
      email = GetCurrentUserEmail();
    }
    if (email == null)
    {
      throw new ArgumentException(Messages.Enrollment.ListError);
    }
    var request = new GetUserEnrollmentsRequest { Email = email };
    var result = await _enrollmentService.GetUserEnrollments(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpGet]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("is-current-user-enrolled")]
  [ActionName("GetIsCurrentUserEnrolled")]
  public async Task<ActionResult<ApiResponse<GetIsUserEnrolledResponse>>> GetIsUserEnrolled(
    string? seasonSlug,
    CancellationToken cancellationToken = default
  )
  {
    var email = GetCurrentUserEmail() ?? "";
    var request = new GetIsUserEnrolledRequest { Email = email, SeasonSlug = seasonSlug ?? "" };
    var result = await _enrollmentService.GetIsUserEnrolled(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpGet]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("get-enrollment-users")]
  [ActionName("GetEnrollmentUsers")]
  public async Task<ActionResult<ApiResponse<GetEnrollmentUsersResponse>>> GetEnrollmentUsers(
    [FromQuery] GetEnrollmentUsersRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _enrollmentService.GetEnrollmentUsers(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpPost]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("kick-student")]
  [ActionName("KickStudent")]
  public async Task<ActionResult<ApiResponse<KickStudentResponse>>> KickStudent(
    KickStudentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _enrollmentService.KickStudent(request, cancellationToken);
    return OkResponse(result);
  }

  [HttpPost]
  [ServiceFilter(typeof(AuthAttribute))]
  [Route("update-student-role-promotion")]
  [ActionName("UpdateStudentRolePromotion")]
  public async Task<
    ActionResult<ApiResponse<UpdateStudentRolePromotionResponse>>
  > UpdateStudentRolePromotion(
    UpdateStudentRolePromotionRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _enrollmentService.UpdateStudentRolePromotion(request, cancellationToken);
    return OkResponse(result);
  }

  #endregion
}
