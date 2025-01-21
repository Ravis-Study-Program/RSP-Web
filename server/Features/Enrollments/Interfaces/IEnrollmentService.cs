using System.Linq.Expressions;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Enrollments.Dtos;

namespace RSPWebAPI.Features.Enrollments.Interfaces;

public interface IEnrollmentService
{
  Task AddEnrollmentAsync(
    EnrollmentEntity enrollment,
    CancellationToken cancellationToken = default
  );

  Task DeleteEnrollmentAsync(string enrollmentId, CancellationToken cancellationToken = default);

  Task UpdateEnrollmentAsync(
    EnrollmentEntity enrollment,
    CancellationToken cancellationToken = default
  );

  Task<IEnumerable<EnrollmentEntity>> GetAllEnrollmentsAsync(
    Expression<Func<EnrollmentEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<EnrollmentEntity>, IQueryable<EnrollmentEntity>>? include = null
  );

  Task<EnrollmentEntity?> GetEnrollmentByIdAsync(
    string enrollmentId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<EnrollmentEntity>, IQueryable<EnrollmentEntity>>? include = null
  );

  Task<EnrollmentEntity?> GetEnrollmentBySeasonId(
    string seasonId,
    string? userId = null,
    SeasonRole? role = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<EnrollmentEntity>, IQueryable<EnrollmentEntity>>? include = null
  );

  Task<EnrollmentEntity?> GetEnrollmentBySeasonSlug(
    string seasonSlug,
    string? email = null,
    SeasonRole? role = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<EnrollmentEntity>, IQueryable<EnrollmentEntity>>? include = null
  );

  Task<IServiceResponse<AdminCreateEnrollmentResponse>> CreateAdminEnrollment(
    AdminCreateEnrollmentRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<AdminDeleteEnrollmentResponse>> DeleteAdminEnrollment(
    AdminDeleteEnrollmentRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<AdminListEnrollmentResponse>> ListAdminEnrollment(
    AdminListEnrollmentRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<AdminUpdateEnrollmentResponse>> UpdateAdminEnrollment(
    AdminUpdateEnrollmentRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<GetCurrentUserEnrollmentsResponse>> GetCurrentUserEnrollments(
    GetCurrentUserEnrollmentsRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<GetEnrollmentUsersResponse>> GetEnrollmentUsers(
    GetEnrollmentUsersRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<GetIsUserEnrolledResponse>> GetIsUserEnrolled(
    GetIsUserEnrolledRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<KickStudentResponse>> KickStudent(
    KickStudentRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<UpdateStudentRolePromotionResponse>> UpdateStudentRolePromotion(
    UpdateStudentRolePromotionRequest request,
    CancellationToken cancellationToken = default
  );

  bool IsSeasonRoleValid(SeasonRole role, List<SeasonRole> allowedRoles, out string errorMessage);

  bool IsSeasonRoleAndRolePromotionValid(
    SeasonRole role,
    SeasonStudentRolePromotion rolePromotion,
    out string errorMessage
  );
}
