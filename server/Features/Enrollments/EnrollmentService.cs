using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Dtos;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.Mentorships.Dtos;
using RSPWebAPI.Features.Mentorships.Interfaces;

namespace RSPWebAPI.Features.Enrollments;

public class EnrollmentService : BaseService, IEnrollmentService
{
  private readonly IRepository<EnrollmentEntity> _enrollmentRepository;
  private readonly IRepository<KickStudentEventEntity> _kickStudentEventRepository;
  private readonly Lazy<IMentorshipService> _mentorshipService;

  public EnrollmentService(
    IRepository<EnrollmentEntity> seasonRepository,
    IRepository<KickStudentEventEntity> kickStudentEventRepository,
    IUnitOfWork unitOfWork,
    ILogger<EnrollmentService> logger,
    Lazy<IMentorshipService> mentorshipService
  )
    : base(unitOfWork, logger)
  {
    _enrollmentRepository = seasonRepository;
    _kickStudentEventRepository = kickStudentEventRepository;
    _mentorshipService = mentorshipService;
  }

  public async Task<AdminCreateEnrollmentResponse> CreateAdminEnrollment(
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
      throw new InvalidOperationException(Messages.Enrollment.Exists);
    }

    if (
      !IsSeasonRoleAndRolePromotionValid(
        request.Role,
        request.StudentRolePromotion,
        out var errorMessage
      )
    )
    {
      throw new ArgumentException(errorMessage);
    }

    var enrollment = new EnrollmentEntity
    {
      EnrollmentId = Database.Constants.GeneratePrimaryKeyId(),
      SeasonId = request.SeasonId,
      UserId = request.UserId,
      Role = request.Role,
      StudentRolePromotion = request.StudentRolePromotion,
    };

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await AddEnrollmentAsync(enrollment, cancellationToken);
        return new AdminCreateEnrollmentResponse { EnrollmentId = enrollment.EnrollmentId };
      },
      Messages.Enrollment.CreationError,
      cancellationToken
    );
  }

  public async Task<AdminDeleteEnrollmentResponse> DeleteAdminEnrollment(
    AdminDeleteEnrollmentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingEnrollment = await GetEnrollmentByIdAsync(request.EnrollmentId, cancellationToken);
    if (existingEnrollment == null)
    {
      throw new KeyNotFoundException(Messages.Enrollment.DoesNotExist);
    }

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await DeleteEnrollmentAsync(existingEnrollment.EnrollmentId, cancellationToken);
        return new AdminDeleteEnrollmentResponse();
      },
      Messages.Enrollment.DeletionError,
      cancellationToken
    );
  }

  public async Task<AdminListEnrollmentResponse> ListAdminEnrollment(
    AdminListEnrollmentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    return await ExecuteWithSaveAsync(
      async () =>
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
            SeasonResourcesUrl = e.Season.ResourcesUrl,
            UserId = e.UserId,
            UserName = e.User.Name,
            UserSlug=e.User.Slug,
            StudentRolePromotion = e.StudentRolePromotion,
          })
          .OrderBy(e => e.SeasonName)
          .ToList();
        return new AdminListEnrollmentResponse { Enrollments = formattedEnrollments };
      },
      Messages.Enrollment.ListError,
      cancellationToken
    );
  }

  public async Task<AdminUpdateEnrollmentResponse> UpdateAdminEnrollment(
    AdminUpdateEnrollmentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingEnrollment = await GetEnrollmentByIdAsync(request.EnrollmentId, cancellationToken);
    if (existingEnrollment == null)
    {
      throw new KeyNotFoundException(Messages.Enrollment.DoesNotExist);
    }

    if (
      !IsSeasonRoleAndRolePromotionValid(
        request.Role,
        request.StudentRolePromotion,
        out var errorMessage
      )
    )
    {
      throw new ArgumentException(errorMessage);
    }

    existingEnrollment.SeasonId = request.SeasonId;
    existingEnrollment.UserId = request.UserId;
    existingEnrollment.Role = request.Role;
    existingEnrollment.StudentRolePromotion = request.StudentRolePromotion;

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await UpdateEnrollmentAsync(existingEnrollment, cancellationToken);
        return new AdminUpdateEnrollmentResponse();
      },
      Messages.Enrollment.UpdateError,
      cancellationToken
    );
  }

  public async Task<GetUserEnrollmentsResponse> GetUserEnrollments(
    GetUserEnrollmentsRequest request,
    CancellationToken cancellationToken = default
  )
  {
    return await ExecuteWithSaveAsync(
      async () =>
      {
        var rawEnrollments = await _enrollmentRepository
          .Table.Where(e => e.User.UserId == request.UserId)
          .Include(e => e.Season)
          .Include(e => e.User)
          .AsNoTracking()
          .ToListAsync(cancellationToken);

        var seasonIds = rawEnrollments.Select(e => e.SeasonId).Distinct().ToList();

        var usersInSeason = await _enrollmentRepository
          .Table.Where(e => seasonIds.Contains(e.SeasonId))
          .GroupBy(e => e.SeasonId)
          .Select(g => new
          {
            SeasonId = g.Key,
            NumStudents = g.Count(e => e.Role == SeasonRole.Student),
            NumMentors = g.Count(e => e.Role == SeasonRole.Mentor),
          })
          .ToListAsync(cancellationToken);

        var menteesBySeason = new Dictionary<string, int>(); // SeasonId -> MenteeCount
        foreach (var enrollment in rawEnrollments.Where(e => e.Role == SeasonRole.Mentor))
        {
          var menteesRequest = new GetCurrentUserMenteesListRequest
          {
            UserId = request.UserId,
            SeasonSlug = enrollment.Season.Slug,
          };

          var menteesResponse = await _mentorshipService.Value.GetCurrentUserMenteesList(
            menteesRequest,
            cancellationToken
          );
          menteesBySeason[enrollment.SeasonId] = menteesResponse?.Mentorships.Count ?? 0;
        }

        var enrollments = rawEnrollments
          .Select(e =>
          {
            var userCount =
              usersInSeason.FirstOrDefault(x => x.SeasonId == e.SeasonId)?.NumStudents ?? 0;
            var mentorsCount =
              usersInSeason.FirstOrDefault(x => x.SeasonId == e.SeasonId)?.NumMentors ?? 0;
            var menteeCount = menteesBySeason.TryGetValue(e.SeasonId, out var count) ? count : 0;

            return new EnrollmentResponseDto
            {
              EnrollmentId = e.EnrollmentId,
              SeasonStartDate = e.Season.StartDateInclusiveUtc,
              SeasonEndDate = e.Season.EndDateInclusiveUtc,
              SeasonId = e.SeasonId,
              SeasonSlug = e.Season.Slug,
              SeasonName = e.Season.Name,
              SeasonImageUrl = e.Season.ImageUrl,
              SeasonResourcesUrl = e.Season.ResourcesUrl,
              UserId = e.User.UserId,
              UserName = e.User.Name,
              UserSlug=e.User.Slug,
              Role = e.Role,
              StudentRolePromotion = e.StudentRolePromotion,
              NumStudentsInSeason = userCount,
              NumMentorsInSeason = mentorsCount,
              NumMenteesInSeason = e.Role == SeasonRole.Mentor ? menteeCount : 0,
            };
          })
          .OrderBy(e => e.SeasonName)
          .ToList();

        return new GetUserEnrollmentsResponse { Enrollments = enrollments };
      },
      Messages.Enrollment.UsersListError,
      cancellationToken
    );
  }

  public async Task<GetEnrollmentUsersResponse> GetEnrollmentUsers(
    GetEnrollmentUsersRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var query = _enrollmentRepository
      .Table.AsNoTracking()
      .Include(e => e.User)
      .Include(e => e.Season)
      .AsQueryable();

    var filterBySeason = !string.IsNullOrEmpty(request.SeasonSlug);
    var filterByGraduates = request.OnlyGraduates == true;

    if (filterBySeason)
    {
      query = query.Where(e => e.Season.Slug == request.SeasonSlug);
    }
    
    if (filterByGraduates)
    {
      query = query.Where(e => e.User.IsGraduate);
    }

    var enrollmentUsers = await query.ToListAsync(cancellationToken);

    var distinctUsers = enrollmentUsers
      .GroupBy(e => e.User.Slug)
      .Select(g =>
      {
        var e = g.First();

        return new EnrollmentUserDto
        {
          UserId = e.User.UserId,
          Name = e.User.Name,
          Slug = e.User.Slug,
          ProfileImage = e.User.ProfileImage,
          Role = filterBySeason ? e.Role : null,
          StudentRolePromotion = filterBySeason ? e.StudentRolePromotion : null,
          IsGraduate = e.User.IsGraduate,
        };
      })
      .OrderBy(e => e.Name)
      .ToList();

    return new GetEnrollmentUsersResponse { EnrollmentUsers = distinctUsers };
  }

  public async Task<GetIsUserEnrolledResponse> GetIsUserEnrolled(
    GetIsUserEnrolledRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingEnrollment = await GetEnrollmentBySeasonSlug(
      request.SeasonSlug,
      request.UserId,
      null,
      cancellationToken
    );
    if (existingEnrollment == null)
    {
      return new GetIsUserEnrolledResponse
      {
        IsEnrolled = false,
        Role = null,
        EnrollmentId = null,
        SeasonId = null,
        StudentRolePromotion = SeasonStudentRolePromotion.NotApplicable,
        UserId = request.UserId,
      };
    }

    return new GetIsUserEnrolledResponse
    {
      IsEnrolled = true,
      Role = existingEnrollment.Role,
      EnrollmentId = existingEnrollment.EnrollmentId,
      SeasonId = existingEnrollment.SeasonId,
      StudentRolePromotion = existingEnrollment.StudentRolePromotion,
      UserId = request.UserId,
    };
  }

  public async Task<KickStudentResponse> KickStudent(
    KickStudentRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingEnrollment = await GetEnrollmentBySeasonSlug(
      request.SeasonSlug,
      request.UserId,
      null,
      cancellationToken
    );
    if (existingEnrollment == null)
    {
      throw new KeyNotFoundException(Messages.Enrollment.DoesNotExist);
    }

    if (
      !IsSeasonRoleValid(
        existingEnrollment.Role,
        new List<SeasonRole> { SeasonRole.Mentor, SeasonRole.Coordinator },
        out var errorMessage
      )
    )
    {
      throw new ArgumentException(errorMessage);
    }

    var studentEnrollment = await GetEnrollmentByIdAsync(
      request.MenteeEnrollmentId,
      cancellationToken
    );
    if (studentEnrollment == null)
    {
      throw new KeyNotFoundException(Messages.Student.KickMenteeDoesntExist);
    }

    if (
      !IsSeasonRoleValid(
        studentEnrollment.Role,
        new List<SeasonRole> { SeasonRole.Student },
        out errorMessage
      )
    )
    {
      throw new ArgumentException(errorMessage);
    }

    return await ExecuteWithSaveAsync(
      async () =>
      {
        var kickStudentEvent = new KickStudentEventEntity
        {
          KickStudentEventId = Database.Constants.GeneratePrimaryKeyId(),
          KickedAtUtc = DateTime.UtcNow,
          MentorId = existingEnrollment.UserId,
          StudentId = studentEnrollment.UserId,
          SeasonId = studentEnrollment.SeasonId,
          KickReason = request.KickReason,
        };

        await _kickStudentEventRepository.AddAsync(kickStudentEvent, cancellationToken);

        _enrollmentRepository.Delete(studentEnrollment, cancellationToken);
        
        return new KickStudentResponse();
      },
      Messages.Student.KickError,
      cancellationToken
    );
  }

  public async Task<UpdateStudentRolePromotionResponse> UpdateStudentRolePromotion(
    UpdateStudentRolePromotionRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingEnrollment = await GetEnrollmentBySeasonSlug(
      request.SeasonSlug,
      request.UserId,
      null,
      cancellationToken
    );
    if (existingEnrollment == null)
    {
      throw new KeyNotFoundException(Messages.Enrollment.DoesNotExist);
    }

    if (
      !IsSeasonRoleValid(
        existingEnrollment.Role,
        new List<SeasonRole> { SeasonRole.Mentor, SeasonRole.Coordinator },
        out var errorMessage
      )
    )
    {
      throw new ArgumentException(errorMessage);
    }

    var studentEnrollment = await GetEnrollmentByIdAsync(
      request.MenteeEnrollmentId,
      cancellationToken
    );
    if (studentEnrollment == null)
    {
      throw new KeyNotFoundException(Messages.Student.UpdateRolePromotionMenteeDoesntExist);
    }

    if (
      !IsSeasonRoleValid(
        studentEnrollment.Role,
        new List<SeasonRole> { SeasonRole.Student },
        out errorMessage
      )
    )
    {
      throw new ArgumentException(errorMessage);
    }

    if (
      !IsSeasonRoleAndRolePromotionValid(
        studentEnrollment.Role,
        request.StudentRolePromotion,
        out errorMessage
      )
    )
    {
      throw new ArgumentException(errorMessage);
    }

    // TODO: ensure mentorship integrity if the mentor is kicking the student out

    studentEnrollment.StudentRolePromotion = request.StudentRolePromotion;

    return await ExecuteWithSaveAsync(
      () =>
      {
        return Task.FromResult(new UpdateStudentRolePromotionResponse());
      },
      Messages.Student.UpdateRolePromotionError,
      cancellationToken
    );
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
          errorMessage = Messages.User.NotInSeasonRole("student");
          break;
        case SeasonRole.Mentor:
          errorMessage = Messages.User.NotInSeasonRole("mentor");
          break;
        case SeasonRole.Coordinator:
          errorMessage = Messages.User.NotInSeasonRole("coordinator");
          break;
        default:
          errorMessage = Messages.Common.UnexpectedError;
          break;
      }

      return false;
    }

    errorMessage = Messages.Common.UnexpectedError;
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
          errorMessage = Messages.Enrollment.StudentMustHaveRolePromotion;
          return false;
        }

        break;

      case SeasonRole.Mentor:
      case SeasonRole.Coordinator:
        if (rolePromotion != SeasonStudentRolePromotion.NotApplicable)
        {
          errorMessage = Messages.Enrollment.MentorOrCoordinatorMustNotHaveRolePromotion;
          return false;
        }

        break;
    }

    errorMessage = Messages.Common.UnexpectedError;
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
      throw new KeyNotFoundException(Messages.Enrollment.DoesNotExist);
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
      throw new KeyNotFoundException(Messages.Enrollment.DoesNotExist);
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
        && (email == null || q.User.UserId == email)
        && (role == null || q.Role == role),
      cancellationToken,
      include
    );
  }

  #endregion
}
