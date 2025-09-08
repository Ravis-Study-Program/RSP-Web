using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.Mentorships.Dtos;
using RSPWebAPI.Features.Mentorships.Interfaces;

namespace RSPWebAPI.Features.Mentorships;

public class MentorshipService : IMentorshipService
{
  private readonly IEnrollmentService _enrollmentService;
  private readonly ILogger<MentorshipService> _logger;
  private readonly IRepository<MentorshipEntity> _mentorshipRepository;
  private readonly IUnitOfWork _unitOfWork;

  public MentorshipService(
    IRepository<MentorshipEntity> mentorshipRepository,
    IEnrollmentService enrollmentService,
    IUnitOfWork unitOfWork,
    ILogger<MentorshipService> logger
  )
  {
    _mentorshipRepository = mentorshipRepository;
    _enrollmentService = enrollmentService;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  public async Task<IEnumerable<MentorshipEntity>> GetAllMentorshipsAsync(
    Expression<Func<MentorshipEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<MentorshipEntity>, IQueryable<MentorshipEntity>>? include = null
  )
  {
    return await _mentorshipRepository.GetAllAsync(predicate, cancellationToken, include);
  }

  public async Task<MentorshipEntity?> GetMentorshipByIdAsync(
    string mentorshipId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<MentorshipEntity>, IQueryable<MentorshipEntity>>? include = null
  )
  {
    return await _mentorshipRepository.GetByIdAsync(mentorshipId, cancellationToken, include);
  }

  public async Task<AdminCreateMentorshipResponse> CreateAdminMentorship(
    AdminCreateMentorshipRequest request,
    CancellationToken cancellationToken = default
  )
  {
    // Get Mentor
    var mentorEnrollment = await _enrollmentService.GetEnrollmentByIdAsync(
      request.MentorEnrollmentId,
      cancellationToken,
      q => q.Include(e => e.Season)
    );
    if (mentorEnrollment == null)
    {
      throw new KeyNotFoundException(Messages.Enrollment.DoesNotExist);
    }

    if (
      !_enrollmentService.IsSeasonRoleValid(
        mentorEnrollment.Role,
        new List<SeasonRole> { SeasonRole.Mentor },
        out var errorMessage
      )
    )
    {
      throw new ArgumentException(errorMessage);
    }

    // Get Mentee
    var menteeEnrollment = await _enrollmentService.GetEnrollmentByIdAsync(
      request.MenteeEnrollmentId,
      cancellationToken,
      q => q.Include(e => e.Season)
    );
    if (menteeEnrollment == null)
    {
      throw new KeyNotFoundException(Messages.Enrollment.DoesNotExist);
    }

    if (
      !_enrollmentService.IsSeasonRoleValid(
        menteeEnrollment.Role,
        new List<SeasonRole> { SeasonRole.Student },
        out errorMessage
      )
    )
    {
      throw new ArgumentException(errorMessage);
    }

    // Ensure mentor and mentee are part of the same season
    if (mentorEnrollment.SeasonId != menteeEnrollment.SeasonId)
    {
      throw new InvalidOperationException(Messages.Mentorship.NotPermittedDueToDifferentSeason);
    }

    // Ensure that there isn't an existing mentorship
    var existingMentorship = await GetMentorshipByEnrollmentIdsAsync(
      mentorEnrollment.EnrollmentId,
      menteeEnrollment.EnrollmentId,
      cancellationToken
    );
    if (existingMentorship != null)
    {
      throw new InvalidOperationException(Messages.Mentorship.Exists);
    }

    var mentorship = new MentorshipEntity
    {
      MentorshipId = Database.Constants.GeneratePrimaryKeyId(),
      MentorEnrollmentId = request.MentorEnrollmentId,
      MenteeEnrollmentId = request.MenteeEnrollmentId,
    };

    try
    {
      await AddMentorshipAsync(mentorship, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new AdminCreateMentorshipResponse { MentorshipId = mentorship.MentorshipId };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.Mentorship.CreationError);
      throw new InvalidOperationException(Messages.Mentorship.CreationError);
    }
  }

  public async Task<AdminDeleteMentorshipResponse> DeleteAdminMentorship(
    AdminDeleteMentorshipRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingMentorship = await GetMentorshipByIdAsync(request.MentorshipId, cancellationToken);
    if (existingMentorship == null)
    {
      throw new KeyNotFoundException(Messages.Mentorship.DoesNotExist);
    }

    try
    {
      await DeleteMentorshipAsync(existingMentorship.MentorshipId, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new AdminDeleteMentorshipResponse();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.Mentorship.DeletionError);
      throw new InvalidOperationException(Messages.Mentorship.DeletionError);
    }
  }

  public async Task<AdminListMentorshipResponse> ListAdminMentorship(
    AdminListMentorshipRequest request,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      var mentorships = await GetAllMentorshipsAsync(
        null,
        cancellationToken,
        q =>
          q.Include(m => m.MentorEnrollment)
            .ThenInclude(m => m.Season)
            .Include(m => m.MentorEnrollment)
            .ThenInclude(m => m.User)
            .Include(m => m.MenteeEnrollment)
            .ThenInclude(m => m.Season)
            .Include(m => m.MenteeEnrollment)
            .ThenInclude(m => m.User)
      );
      var formattedMentorships = mentorships
        .Select(m => new MentorshipResponse
        {
          SeasonId = m.MentorEnrollment.SeasonId,
          MentorshipId = m.MentorshipId,
          SeasonName = m.MentorEnrollment.Season.Name,
          SeasonSlug = m.MentorEnrollment.Season.Slug,
          MentorEnrollmentId = m.MentorEnrollmentId,
          MentorName = m.MentorEnrollment.User.Name,
          MenteeEnrollmentId = m.MenteeEnrollmentId,
          MenteeName = m.MenteeEnrollment.User.Name,
        })
        .ToList();
      return new AdminListMentorshipResponse { Mentorships = formattedMentorships };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.Mentorship.ListError);
      throw new InvalidOperationException(Messages.Mentorship.ListError);
    }
  }

  public async Task<AdminUpdateMentorshipResponse> UpdateAdminMentorship(
    AdminUpdateMentorshipRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingMentorship = await GetMentorshipByIdAsync(request.MentorshipId, cancellationToken);
    if (existingMentorship == null)
    {
      throw new KeyNotFoundException(Messages.Mentorship.DoesNotExist);
    }

    // Get Mentor
    var mentorEnrollment = await _enrollmentService.GetEnrollmentByIdAsync(
      request.MentorEnrollmentId,
      cancellationToken,
      q => q.Include(e => e.Season)
    );
    if (mentorEnrollment == null)
    {
      throw new KeyNotFoundException(Messages.Enrollment.DoesNotExist);
    }

    if (
      !_enrollmentService.IsSeasonRoleValid(
        mentorEnrollment.Role,
        new List<SeasonRole> { SeasonRole.Mentor },
        out var errorMessage
      )
    )
    {
      throw new ArgumentException(errorMessage);
    }

    // Get Mentee
    var menteeEnrollment = await _enrollmentService.GetEnrollmentByIdAsync(
      request.MenteeEnrollmentId,
      cancellationToken,
      q => q.Include(e => e.Season)
    );
    if (menteeEnrollment == null)
    {
      throw new KeyNotFoundException(Messages.Enrollment.DoesNotExist);
    }

    if (
      !_enrollmentService.IsSeasonRoleValid(
        menteeEnrollment.Role,
        new List<SeasonRole> { SeasonRole.Student },
        out errorMessage
      )
    )
    {
      throw new ArgumentException(errorMessage);
    }

    // Ensure mentor and mentee are part of the same season
    if (mentorEnrollment.SeasonId != menteeEnrollment.SeasonId)
    {
      throw new InvalidOperationException(Messages.Mentorship.NotPermittedDueToDifferentSeason);
    }

    existingMentorship.MentorEnrollmentId = request.MentorEnrollmentId;
    existingMentorship.MenteeEnrollmentId = request.MenteeEnrollmentId;

    try
    {
      await UpdateMentorshipAsync(existingMentorship, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new AdminUpdateMentorshipResponse();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.Mentorship.UpdateError);
      throw new InvalidOperationException(Messages.Mentorship.UpdateError);
    }
  }

  public async Task<GetCurrentUserMenteesListResponse> GetCurrentUserMenteesList(
    GetCurrentUserMenteesListRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var mentorships = await GetAllMentorshipsAsync(
      m =>
        m.MentorEnrollment.User.Email == request.Email
        && m.MentorEnrollment.Season.Slug == request.SeasonSlug,
      cancellationToken,
      q =>
        q.Include(m => m.MentorEnrollment)
          .ThenInclude(m => m.Season)
          .Include(m => m.MentorEnrollment)
          .ThenInclude(m => m.User)
          .Include(m => m.MenteeEnrollment)
          .ThenInclude(m => m.Season)
          .Include(m => m.MenteeEnrollment)
          .ThenInclude(m => m.User)
    );
    var formattedMentorships = mentorships
      .Select(m => new MentorshipResponse
      {
        SeasonId = m.MentorEnrollment.SeasonId,
        MentorshipId = m.MentorshipId,
        SeasonName = m.MentorEnrollment.Season.Name,
        SeasonSlug = m.MentorEnrollment.Season.Slug,
        MentorEnrollmentId = m.MentorEnrollmentId,
        MentorName = m.MentorEnrollment.User.Name,
        MentorEmail = m.MentorEnrollment.User.Email,
        MenteeEnrollmentId = m.MenteeEnrollmentId,
        MenteeName = m.MenteeEnrollment.User.Name,
        StudentRolePromotion = m.MenteeEnrollment.StudentRolePromotion,
        MenteeEmail = m.MenteeEnrollment.User.Email,
      })
      .ToList();
    try
    {
      return new GetCurrentUserMenteesListResponse { Mentorships = formattedMentorships };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.Mentorship.ListError);
      throw new InvalidOperationException(Messages.Mentorship.ListError);
    }
  }

  public async Task<MentorshipEntity?> GetMentorshipByEnrollmentIdsAsync(
    string mentorEnrollmentId,
    string menteeEnrollmentId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<MentorshipEntity>, IQueryable<MentorshipEntity>>? include = null
  )
  {
    var mentorships = await _mentorshipRepository.GetAllAsync(
      m =>
        (m.MentorEnrollmentId == mentorEnrollmentId)
        && (m.MenteeEnrollmentId == menteeEnrollmentId),
      cancellationToken,
      include
    );
    return mentorships.FirstOrDefault();
  }

  #region CRUD Operations

  public async Task AddMentorshipAsync(
    MentorshipEntity mentorship,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(mentorship);

    await _mentorshipRepository.AddAsync(mentorship, cancellationToken);
  }

  public async Task DeleteMentorshipAsync(
    string mentorshipId,
    CancellationToken cancellationToken = default
  )
  {
    var mentorship = await _mentorshipRepository.GetByIdAsync(mentorshipId, cancellationToken);

    if (mentorship == null)
    {
      throw new KeyNotFoundException(Messages.Mentorship.DoesNotExist);
    }

    _mentorshipRepository.Delete(mentorship, cancellationToken);
  }

  public async Task UpdateMentorshipAsync(
    MentorshipEntity mentorship,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(mentorship);

    var existingMentorship = await _mentorshipRepository.GetByIdAsync(
      mentorship.MentorshipId,
      cancellationToken
    );
    if (existingMentorship == null)
    {
      throw new KeyNotFoundException(Messages.Mentorship.DoesNotExist);
    }

    _mentorshipRepository.Update(mentorship, cancellationToken);
  }

  #endregion
}
