using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Dtos;
using RSPWebAPI.Features.Enrollments.Interfaces;

namespace RSPWebAPI.Features.Enrollments;

public class EnrollmentService : IEnrollmentService
{
  private readonly IRepository<EnrollmentEntity> _enrollmentRepository;
  private readonly ILogger<EnrollmentService> _logger;
  private readonly IUnitOfWork _unitOfWork;

  public EnrollmentService(
    IRepository<EnrollmentEntity> seasonRepository,
    IUnitOfWork unitOfWork,
    ILogger<EnrollmentService> logger
  )
  {
    _enrollmentRepository = seasonRepository;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  public async Task<IServiceResponse<AdminCreateEnrollmentResponse>> CreateAdminEnrollment(
    AdminCreateEnrollmentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingEnrollment = await GetEnrollmentBySeasonId(
      request.SeasonId,
      request.UserId,
      request.Role,
      cancellationToken
    );
    if (existingEnrollment != null)
    {
      return new ErrorServiceResponse<AdminCreateEnrollmentResponse>(Message.EnrollmentExists);
    }

    if (
      !IsSeasonRoleAndRolePromotionValid(
        request.Role,
        request.StudentRolePromotion,
        out var errorMessage
      )
    )
    {
      return new ErrorServiceResponse<AdminCreateEnrollmentResponse>(errorMessage);
    }

    var enrollment = new EnrollmentEntity
    {
      EnrollmentId = Database.Constants.GeneratePrimaryKeyId(),
      SeasonId = request.SeasonId,
      UserId = request.UserId,
      Role = request.Role,
      StudentRolePromotion = request.StudentRolePromotion,
    };

    try
    {
      await AddEnrollmentAsync(enrollment, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminCreateEnrollmentResponse>(
        Message.EnrollmentCreatedSuccessfully,
        new AdminCreateEnrollmentResponse { EnrollmentId = enrollment.EnrollmentId }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.EnrollmentCreationUnexpectedError);
      return new ErrorServiceResponse<AdminCreateEnrollmentResponse>(
        Message.EnrollmentCreationUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<AdminDeleteEnrollmentResponse>> DeleteAdminEnrollment(
    AdminDeleteEnrollmentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingEnrollment = await GetEnrollmentByIdAsync(request.EnrollmentId, cancellationToken);
    if (existingEnrollment == null)
    {
      return new ErrorServiceResponse<AdminDeleteEnrollmentResponse>(
        Message.EnrollmentDoesNotExists
      );
    }

    try
    {
      await DeleteEnrollmentAsync(existingEnrollment.EnrollmentId, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminDeleteEnrollmentResponse>(
        Message.EnrollmentDeletedSuccessfully
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.EnrollmentDeletionUnexpectedError);
      return new ErrorServiceResponse<AdminDeleteEnrollmentResponse>(
        Message.EnrollmentDeletionUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<AdminListEnrollmentResponse>> ListAdminEnrollment(
    AdminListEnrollmentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      var enrollments = await GetAllEnrollmentsAsync(
        null,
        cancellationToken,
        q => q.Include(e => e.User).Include(e => e.Season)
      );
      var formattedEnrollments = enrollments
        .Select(e => new EnrollmentResponseDto
        {
          EnrollmentId = e.EnrollmentId,
          Role = e.Role,
          SeasonId = e.SeasonId,
          SeasonName = e.Season.Name,
          SeasonImageUrl = e.Season.ImageUrl,
          SeasonSlug = e.Season.Slug,
          UserId = e.UserId,
          UserName = e.User.Name,
          StudentRolePromotion = e.StudentRolePromotion,
        })
        .ToList();
      return new SuccessServiceResponse<AdminListEnrollmentResponse>(
        Message.EnrollmentListSuccessfully,
        new AdminListEnrollmentResponse { Enrollments = formattedEnrollments }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.EnrollmentListUnexpectedError);
      return new ErrorServiceResponse<AdminListEnrollmentResponse>(
        Message.EnrollmentListUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<AdminUpdateEnrollmentResponse>> UpdateAdminEnrollment(
    AdminUpdateEnrollmentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingEnrollment = await GetEnrollmentByIdAsync(request.EnrollmentId, cancellationToken);
    if (existingEnrollment == null)
    {
      return new ErrorServiceResponse<AdminUpdateEnrollmentResponse>(
        Message.EnrollmentDoesNotExists
      );
    }

    if (
      !IsSeasonRoleAndRolePromotionValid(
        request.Role,
        request.StudentRolePromotion,
        out var errorMessage
      )
    )
    {
      return new ErrorServiceResponse<AdminUpdateEnrollmentResponse>(errorMessage);
    }

    existingEnrollment.SeasonId = request.SeasonId;
    existingEnrollment.UserId = request.UserId;
    existingEnrollment.Role = request.Role;
    existingEnrollment.StudentRolePromotion = request.StudentRolePromotion;

    try
    {
      await UpdateEnrollmentAsync(existingEnrollment, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminUpdateEnrollmentResponse>(
        Message.EnrollmentUpdatedSuccessfully
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.EnrollmentUpdateUnexpectedError);
      return new ErrorServiceResponse<AdminUpdateEnrollmentResponse>(
        Message.EnrollmentUpdateUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<GetCurrentUserEnrollmentsResponse>> GetCurrentUserEnrollments(
    GetCurrentUserEnrollmentsRequest request,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      var enrollments = await _enrollmentRepository
        .Table.Where(e => e.User.Email == request.Email)
        .Select(e => new EnrollmentResponseDto
        {
          EnrollmentId = e.EnrollmentId,
          SeasonId = e.SeasonId,
          SeasonSlug = e.Season.Slug,
          SeasonName = e.Season.Name,
          SeasonImageUrl = e.Season.ImageUrl,
          UserId = e.User.UserId,
          UserName = e.User.Name,
          Role = e.Role,
          StudentRolePromotion = e.StudentRolePromotion,
        })
        .AsNoTracking()
        .ToListAsync(cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<GetCurrentUserEnrollmentsResponse>(
        Message.EnrollmentUpdatedSuccessfully,
        new GetCurrentUserEnrollmentsResponse { Enrollments = enrollments }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.EnrollmentUpdateUnexpectedError);
      return new ErrorServiceResponse<GetCurrentUserEnrollmentsResponse>(
        Message.EnrollmentUpdateUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<GetEnrollmentUsersResponse>> GetEnrollmentUsers(
    GetEnrollmentUsersRequest request,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      var enrollmentUsers = await _enrollmentRepository
        .Table.Where(e => e.Season.Slug == request.SeasonSlug)
        .Select(e => new EnrollmentUserDto
        {
          Name = e.User.Name,
          Role = e.Role,
          DiscordId = e.User.DiscordId,
          ProfileImage = e.User.ProfileImage,
          Email = e.User.Email,
          StudentRolePromotion = e.StudentRolePromotion,
        })
        .AsNoTracking()
        .ToListAsync(cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<GetEnrollmentUsersResponse>(
        Message.EnrollmentUpdatedSuccessfully,
        new GetEnrollmentUsersResponse { EnrollmentUsers = enrollmentUsers }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.EnrollmentUpdateUnexpectedError);
      return new ErrorServiceResponse<GetEnrollmentUsersResponse>(
        Message.EnrollmentUpdateUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<GetIsUserEnrolledResponse>> GetIsUserEnrolled(
    GetIsUserEnrolledRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingEnrollment = await GetEnrollmentBySeasonSlug(
      request.SeasonSlug,
      request.Email,
      null,
      cancellationToken
    );
    if (existingEnrollment == null)
    {
      return new SuccessServiceResponse<GetIsUserEnrolledResponse>(
        Message.EnrollmentDoesNotExists,
        new GetIsUserEnrolledResponse
        {
          IsEnrolled = false,
          Role = null,
          EnrollmentId = null,
          StudentRolePromotion = SeasonStudentRolePromotion.NotApplicable,
          Email = request.Email,
        }
      );
    }

    return new SuccessServiceResponse<GetIsUserEnrolledResponse>(
      Message.EnrollmentExists,
      new GetIsUserEnrolledResponse
      {
        IsEnrolled = true,
        Role = existingEnrollment.Role,
        EnrollmentId = existingEnrollment.EnrollmentId,
        StudentRolePromotion = existingEnrollment.StudentRolePromotion,
        Email = request.Email,
      }
    );
  }

  public async Task<IServiceResponse<KickStudentResponse>> KickStudent(
    KickStudentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingEnrollment = await GetEnrollmentBySeasonSlug(
      request.SeasonSlug,
      request.Email,
      null,
      cancellationToken
    );
    if (existingEnrollment == null)
    {
      return new ErrorServiceResponse<KickStudentResponse>(Message.EnrollmentDoesNotExists);
    }

    if (
      !IsSeasonRoleValid(
        existingEnrollment.Role,
        new List<SeasonRole> { SeasonRole.Mentor, SeasonRole.Coordinator },
        out var errorMessage
      )
    )
    {
      return new ErrorServiceResponse<KickStudentResponse>(errorMessage);
    }

    var studentEnrollment = await GetEnrollmentByIdAsync(
      request.MenteeEnrollmentId,
      cancellationToken
    );
    if (studentEnrollment == null)
    {
      return new ErrorServiceResponse<KickStudentResponse>(Message.KickStudentMenteeDoesntExist);
    }

    if (
      !IsSeasonRoleValid(
        existingEnrollment.Role,
        new List<SeasonRole> { SeasonRole.Student },
        out errorMessage
      )
    )
    {
      return new ErrorServiceResponse<KickStudentResponse>(errorMessage);
    }

    // TODO: ensure mentorship integrity if the mentor is kicking the student out

    try
    {
      _enrollmentRepository.Delete(studentEnrollment, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<KickStudentResponse>(Message.KickStudentSuccessfully);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.EnrollmentUpdateUnexpectedError);
      return new ErrorServiceResponse<KickStudentResponse>(Message.KickStudentUnexpectedError);
    }
  }

  public async Task<
    IServiceResponse<UpdateStudentRolePromotionResponse>
  > UpdateStudentRolePromotion(
    UpdateStudentRolePromotionRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingEnrollment = await GetEnrollmentBySeasonSlug(
      request.SeasonSlug,
      request.Email,
      null,
      cancellationToken
    );
    if (existingEnrollment == null)
    {
      return new ErrorServiceResponse<UpdateStudentRolePromotionResponse>(
        Message.EnrollmentDoesNotExists
      );
    }

    if (
      !IsSeasonRoleValid(
        existingEnrollment.Role,
        new List<SeasonRole> { SeasonRole.Mentor, SeasonRole.Coordinator },
        out var errorMessage
      )
    )
    {
      return new ErrorServiceResponse<UpdateStudentRolePromotionResponse>(errorMessage);
    }

    var studentEnrollment = await GetEnrollmentByIdAsync(
      request.MenteeEnrollmentId,
      cancellationToken
    );
    if (studentEnrollment == null)
    {
      return new ErrorServiceResponse<UpdateStudentRolePromotionResponse>(
        Message.UpdateStudentRolePromotionMenteeDoesntExist
      );
    }

    if (
      !IsSeasonRoleValid(
        studentEnrollment.Role,
        new List<SeasonRole> { SeasonRole.Student },
        out errorMessage
      )
    )
    {
      return new ErrorServiceResponse<UpdateStudentRolePromotionResponse>(errorMessage);
    }

    if (
      !IsSeasonRoleAndRolePromotionValid(
        studentEnrollment.Role,
        request.StudentRolePromotion,
        out errorMessage
      )
    )
    {
      return new ErrorServiceResponse<UpdateStudentRolePromotionResponse>(errorMessage);
    }

    // TODO: ensure mentorship integrity if the mentor is kicking the student out

    studentEnrollment.StudentRolePromotion = request.StudentRolePromotion;

    try
    {
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<UpdateStudentRolePromotionResponse>(
        Message.UpdateStudentRolePromotionSuccessfully
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.EnrollmentUpdateUnexpectedError);
      return new ErrorServiceResponse<UpdateStudentRolePromotionResponse>(
        Message.KickStudentUnexpectedError
      );
    }
  }

  /// <summary>
  ///   Validate role based on list of allowed roles
  /// </summary>
  public bool IsSeasonRoleValid(
    SeasonRole role,
    List<SeasonRole> allowedRoles,
    out string errorMessage
  )
  {
    if (!allowedRoles.Contains(role))
    {
      switch (role)
      {
        case SeasonRole.Student:
          errorMessage = Message.UserIsNotStudentInSeason;
          break;
        case SeasonRole.Mentor:
          errorMessage = Message.UserIsNotMentorInSeason;
          break;
        case SeasonRole.Coordinator:
          errorMessage = Message.UserIsNotCoordinatorInSeason;
          break;
        default:
          errorMessage = Message.UnexpectedError;
          break;
      }

      return false;
    }

    errorMessage = Message.UnexpectedError;
    return true;
  }

  /// <summary>
  ///   Validate student role promotion and season role correctness
  /// </summary>
  public bool IsSeasonRoleAndRolePromotionValid(
    SeasonRole role,
    SeasonStudentRolePromotion rolePromotion,
    out string errorMessage
  )
  {
    switch (role)
    {
      case SeasonRole.Student:
        if (rolePromotion == SeasonStudentRolePromotion.NotApplicable)
        {
          errorMessage = Message.EnrollmentStudentMustHaveAppropriateRolePromotion;
          return false;
        }

        break;

      case SeasonRole.Mentor:
      case SeasonRole.Coordinator:
        if (rolePromotion != SeasonStudentRolePromotion.NotApplicable)
        {
          errorMessage = Message.EnrollmentMentorOrCoordinatorMustNotHaveRolePromotion;
          return false;
        }

        break;
    }

    errorMessage = Message.UnexpectedError;
    return true;
  }

  #region CRUD Operations

  public async Task AddEnrollmentAsync(
    EnrollmentEntity enrollment,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(enrollment);

    await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
  }

  public async Task DeleteEnrollmentAsync(
    string enrollmentId,
    CancellationToken cancellationToken = default
  )
  {
    var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken);

    if (enrollment == null)
    {
      throw new KeyNotFoundException(Message.EnrollmentDoesNotExists);
    }

    _enrollmentRepository.Delete(enrollment, cancellationToken);
  }

  public async Task UpdateEnrollmentAsync(
    EnrollmentEntity enrollment,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(enrollment);

    var existingEnrollment = await _enrollmentRepository.GetByIdAsync(
      enrollment.EnrollmentId,
      cancellationToken
    );
    if (existingEnrollment == null)
    {
      throw new KeyNotFoundException(Message.EnrollmentDoesNotExists);
    }

    _enrollmentRepository.Update(enrollment, cancellationToken);
  }

  public async Task<IEnumerable<EnrollmentEntity>> GetAllEnrollmentsAsync(
    Expression<Func<EnrollmentEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<EnrollmentEntity>, IQueryable<EnrollmentEntity>>? include = null
  )
  {
    return await _enrollmentRepository.GetAllAsync(predicate, cancellationToken, include);
  }

  public async Task<EnrollmentEntity?> GetEnrollmentByIdAsync(
    string enrollmentId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<EnrollmentEntity>, IQueryable<EnrollmentEntity>>? include = null
  )
  {
    return await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken, include);
  }

  public async Task<EnrollmentEntity?> GetEnrollmentBySeasonId(
    string seasonId,
    string? userId = null,
    SeasonRole? role = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<EnrollmentEntity>, IQueryable<EnrollmentEntity>>? include = null
  )
  {
    return await _enrollmentRepository.FirstOrDefaultAsync(
      q =>
        q.SeasonId == seasonId
        && (userId == null || q.UserId == userId)
        && (role == null || q.Role == role),
      cancellationToken,
      include
    );
  }

  public async Task<EnrollmentEntity?> GetEnrollmentBySeasonSlug(
    string seasonSlug,
    string? email = null,
    SeasonRole? role = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<EnrollmentEntity>, IQueryable<EnrollmentEntity>>? include = null
  )
  {
    return await _enrollmentRepository.FirstOrDefaultAsync(
      q =>
        q.Season.Slug == seasonSlug
        && (email == null || q.User.Email == email)
        && (role == null || q.Role == role),
      cancellationToken,
      include
    );
  }

  #endregion
}
