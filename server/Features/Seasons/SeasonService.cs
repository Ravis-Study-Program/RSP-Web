using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Seasons.Dtos;
using RSPWebAPI.Features.Seasons.Interfaces;

namespace RSPWebAPI.Features.Seasons;

public class SeasonService : BaseService, ISeasonService
{
  private readonly IRepository<SeasonEntity> _seasonRepository;
  private readonly IRepository<EnrollmentEntity> _enrollmentRepository;
  private readonly IRepository<MentorshipEntity> _mentorshipRepository;

  public SeasonService(
    IRepository<SeasonEntity> seasonRepository,
    IRepository<EnrollmentEntity> enrollmentRepository,
    IRepository<MentorshipEntity> mentorshipRepository,
    IUnitOfWork unitOfWork,
    ILogger<SeasonService> logger
  )
    : base(unitOfWork, logger)
  {
    _seasonRepository = seasonRepository;
    _enrollmentRepository = enrollmentRepository;
    _mentorshipRepository = mentorshipRepository;
  }

  public async Task<AdminCreateSeasonResponse> CreateAdminSeason(
    AdminCreateSeasonRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingSeason = await GetSeasonBySlugAsync(request.Slug, cancellationToken);
    if (existingSeason != null)
    {
      throw new InvalidOperationException(Messages.Season.Exists);
    }

    var season = new SeasonEntity
    {
      SeasonId = Database.Constants.GeneratePrimaryKeyId(),
      Name = request.Name,
      Slug = request.Slug,
      StartDateInclusiveUtc = request.StartDateInclusiveUtc,
      EndDateInclusiveUtc = request.EndDateInclusiveUtc,
      Location = request.Location,
      ImageUrl = request.ImageUrl,
      ResourcesUrl = request.ResourcesUrl,
      IsDataBackFilled = request.IsDataBackFilled,
    };

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await AddSeasonAsync(season, cancellationToken);
        return new AdminCreateSeasonResponse { SeasonId = season.SeasonId };
      },
      Messages.Season.CreationError,
      cancellationToken
    );
  }

  public async Task<AdminDeleteSeasonResponse> DeleteAdminSeason(
    AdminDeleteSeasonRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingSeason = await GetSeasonByIdAsync(request.SeasonId, cancellationToken);
    if (existingSeason == null)
    {
      throw new KeyNotFoundException(Messages.Season.DoesNotExist);
    }

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await DeleteSeasonAsync(existingSeason.SeasonId, cancellationToken);
        return new AdminDeleteSeasonResponse();
      },
      Messages.Season.DeletionError,
      cancellationToken
    );
  }

  public async Task<AdminListSeasonResponse> ListAdminSeason(
    AdminListSeasonRequest request,
    CancellationToken cancellationToken = default
  )
  {
    return await ExecuteWithSaveAsync(
      async () =>
      {
        var seasons = await GetAllSeasonsAsync(null, cancellationToken);
        return new AdminListSeasonResponse() { Seasons = seasons.ToList() };
      },
      Messages.Season.ListError,
      cancellationToken
    );
  }

  public async Task<AdminUpdateSeasonResponse> UpdateAdminSeason(
    AdminUpdateSeasonRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingSeason = await GetSeasonByIdAsync(request.SeasonId, cancellationToken);
    if (existingSeason == null)
    {
      throw new KeyNotFoundException(Messages.Season.DoesNotExist);
    }

    existingSeason.Name = request.Name;
    existingSeason.Slug = request.Slug;
    existingSeason.StartDateInclusiveUtc = request.StartDateInclusiveUtc;
    existingSeason.EndDateInclusiveUtc = request.EndDateInclusiveUtc;
    existingSeason.Location = request.Location;
    existingSeason.ImageUrl = request.ImageUrl;
    existingSeason.ResourcesUrl = request.ResourcesUrl;
    existingSeason.IsDataBackFilled = request.IsDataBackFilled;

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await UpdateSeasonAsync(existingSeason, cancellationToken);
        return new AdminUpdateSeasonResponse();
      },
      Messages.Season.UpdateError,
      cancellationToken
    );
  }

  #region CRUD Operations

  public async Task AddSeasonAsync(
    SeasonEntity season,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(season);

    await _seasonRepository.AddAsync(season, cancellationToken);
  }
 
  public async Task DeleteSeasonAsync(
    string seasonId,
    CancellationToken cancellationToken = default
  )
  {
    var season = await _seasonRepository.GetByIdAsync(seasonId, cancellationToken);

    if (season == null)
    {
      throw new KeyNotFoundException(Messages.Season.DoesNotExist);
    }

    _seasonRepository.Delete(season, cancellationToken);
  }

  public async Task UpdateSeasonAsync(
    SeasonEntity season,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(season);

    var existingSeason = await _seasonRepository.GetByIdAsync(season.SeasonId, cancellationToken);
    if (existingSeason == null)
    {
      throw new KeyNotFoundException(Messages.Season.DoesNotExist);
    }

    _seasonRepository.Update(season, cancellationToken);
  }

  public async Task<IEnumerable<SeasonEntity>> GetAllSeasonsAsync(
    Expression<Func<SeasonEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonEntity>, IQueryable<SeasonEntity>>? include = null
  )
  {
    return await _seasonRepository.GetAllAsync(predicate, cancellationToken, include);
  }

  public async Task<SeasonEntity?> GetSeasonByIdAsync(
    string seasonId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonEntity>, IQueryable<SeasonEntity>>? include = null
  )
  {
    return await _seasonRepository.GetByIdAsync(seasonId, cancellationToken, include);
  }

  public async Task<SeasonEntity?> GetSeasonBySlugAsync(
    string? seasonSlug,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonEntity>, IQueryable<SeasonEntity>>? include = null
  )
  {
    return await _seasonRepository.FirstOrDefaultAsync(
      q => q.Slug == seasonSlug,
      cancellationToken,
      include
    );
  }

  public async Task<GetMentorAssignmentsResponse> GetMentorAssignments(
    string seasonSlug,
    GetMentorAssignmentsRequest request,
    CancellationToken cancellationToken = default
  )
  {
    // Get the season by slug
    var season = await GetSeasonBySlugAsync(seasonSlug, cancellationToken);
    if (season == null)
    {
      throw new KeyNotFoundException(Messages.Season.DoesNotExist);
    }

    // Get all student enrollments for this season
    var studentEnrollments = await _enrollmentRepository.GetAllAsync(
      e => e.SeasonId == season.SeasonId && e.Role == SeasonRole.Student,
      cancellationToken,
      q => q.Include(e => e.User)
    );

    // Get all mentorships for this season
    var mentorships = await _mentorshipRepository.GetAllAsync(
      m => m.MentorEnrollment.SeasonId == season.SeasonId,
      cancellationToken,
      q => q.Include(m => m.MentorEnrollment)
           .ThenInclude(e => e.User)
           .Include(m => m.MenteeEnrollment)
    );

    // Create mentor assignments DTO
    var assignments = studentEnrollments.Select(studentEnrollment =>
    {
      // Find mentorship for this student
      var mentorship = mentorships.FirstOrDefault(m => 
        m.MenteeEnrollmentId == studentEnrollment.EnrollmentId);

      return new MentorAssignmentDto
      {
        StudentId = studentEnrollment.UserId,
        StudentName = studentEnrollment.User.Name,
        StudentSlug = studentEnrollment.User.Slug,
        EnrollmentId = studentEnrollment.EnrollmentId,
        StudentRolePromotion = studentEnrollment.StudentRolePromotion,
        MentorId = mentorship?.MentorEnrollment.UserId,
        MentorName = mentorship?.MentorEnrollment.User.Name,
MentorSlug=mentorship?.MentorEnrollment.User.Slug
      };
    }).ToList();

    return new GetMentorAssignmentsResponse
    {
      Assignments = assignments
    };
  }

  #endregion
}
