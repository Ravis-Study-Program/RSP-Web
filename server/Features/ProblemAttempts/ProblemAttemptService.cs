using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Cache;
using RSPWEBAPI.Common.Cache;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Extensions;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.ProblemAttempts.Dtos;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;

namespace RSPWebAPI.Features.ProblemAttempts;

public class ProblemAttemptService : BaseService, IProblemAttemptService
{
  private readonly IEnrollmentService _enrollmentService;
  private readonly IRepository<ProblemAttemptEntity> _problemAttemptRepository;
  private readonly ISeasonWeekService _seasonWeekService;
  private readonly IRequestCache _cache;
  private readonly IUserService _userService;

  public ProblemAttemptService(
    IRepository<ProblemAttemptEntity> problemAttemptRepository,
    ISeasonWeekService seasonWeekService,
    IUserService userService,
    IEnrollmentService enrollmentService,
    IRequestCache cache,
    IUnitOfWork unitOfWork,
    ILogger<ProblemAttemptService> logger
  )
    : base(unitOfWork, logger)
  {
    _problemAttemptRepository = problemAttemptRepository;
    _seasonWeekService = seasonWeekService;
    _userService = userService;
    _enrollmentService = enrollmentService;
    _cache = cache;
  }

  public async Task<IEnumerable<ProblemAttemptEntity>> GetAllProblemAttemptsAsync(
    Expression<Func<ProblemAttemptEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<ProblemAttemptEntity>, IQueryable<ProblemAttemptEntity>>? include = null
  )
  {
    return await _problemAttemptRepository.GetAllAsync(predicate, cancellationToken, include);
  }

  public async Task<ProblemAttemptEntity?> GetProblemAttemptByIdAsync(
    string problemAttemptId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<ProblemAttemptEntity>, IQueryable<ProblemAttemptEntity>>? include = null
  )
  {
    return await _problemAttemptRepository.GetByIdAsync(
      problemAttemptId,
      cancellationToken,
      include
    );
  }

  public async Task<CreateProblemAttemptResponse> CreateProblemAttempt(
    CreateProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    SeasonWeekEntity? seasonWeek = null;
    var attemptStartDate = DateTime.UtcNow.AddMinutes(-request.TimeTakenInMinutes);
    if (request.EnrollmentId != null)
    {
      var existingEnrollment = await _enrollmentService.GetEnrollmentByIdAsync(
        request.EnrollmentId,
        cancellationToken,
        q => q.Include(e => e.User)
      );
      if (existingEnrollment == null || existingEnrollment.User.UserId != request.UserId)
      {
        throw new KeyNotFoundException(Messages.Enrollment.DoesNotExist);
      }

      var seasonWeeks = await _seasonWeekService.GetAllSeasonWeeksAsync(
        q =>
          q.StartDate <= attemptStartDate
          && q.EndDate >= attemptStartDate
          && q.SeasonId == existingEnrollment.SeasonId,
        cancellationToken
      );
      seasonWeek = seasonWeeks.FirstOrDefault();
      if (seasonWeek == null)
      {
        throw new InvalidOperationException(Messages.ProblemAttempt.OutOfSeasonDateRange);
      }
    }

    // Leetcode should take precedence if CustomProblem is present
    if (request.CustomProblemId != null && request.LeetcodeProblemId != null)
    {
      request.CustomProblemId = null;
    }

    var user = await _userService.GetUserAsync(
      userId: request.UserId,
      cancellationToken: cancellationToken
    );
    if (user == null)
    {
      throw new KeyNotFoundException(Messages.User.EmailDoesNotExist);
    }

    var problemAttempt = new ProblemAttemptEntity
    {
      ProblemAttemptId = Database.Constants.GeneratePrimaryKeyId(),
      AttemptStartDateUtc = attemptStartDate,
      TimeTakenInMinutes = request.TimeTakenInMinutes,
      LeetcodeProblemId = request.LeetcodeProblemId,
      CustomProblemId = request.CustomProblemId,
      Notes = request.Notes,
      EnrollmentId = request.EnrollmentId,
      UserId = user.UserId,
      SeasonWeekId = seasonWeek?.SeasonWeekId,
    };

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await AddProblemAttemptAsync(problemAttempt, cancellationToken);
        _cache.Remove(RouteCacheKeys.ListProblemAttempts, request.UserId);
        return new CreateProblemAttemptResponse
        {
          ProblemAttemptId = problemAttempt.ProblemAttemptId,
        };
      },
      Messages.ProblemAttempt.CreationError,
      cancellationToken
    );
  }

  public async Task<DeleteProblemAttemptResponse> DeleteProblemAttempt(
    DeleteProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingProblemAttempt = await GetProblemAttemptByIdAsync(
      request.ProblemAttemptId,
      cancellationToken,
      q => q.Include(p => p.User)
    );
    if (existingProblemAttempt == null || existingProblemAttempt.User.UserId != request.UserId)
    {
      throw new KeyNotFoundException(Messages.ProblemAttempt.DoesNotExist);
    }

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await DeleteProblemAttemptAsync(existingProblemAttempt.ProblemAttemptId, cancellationToken);
        _cache.Remove(RouteCacheKeys.ListProblemAttempts, request.UserId);
        return new DeleteProblemAttemptResponse();
      },
      Messages.ProblemAttempt.DeletionError,
      cancellationToken
    );
  }

  public async Task<ListProblemAttemptResponse> ListProblemAttempt(
    ListProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    // Build cache key with pagination parameters
    var userIdsKey = string.Join("-", request.UserIds.Distinct().OrderBy(x => x));
    var cacheKey =
      $"p{request.Page}-ps{request.PageSize}-u{userIdsKey}-s{request.SeasonId ?? "all"}-lc{request.IncludeLeetcode}-c{request.IncludeCustom}";

    var response = await _cache.GetOrCreateAsync(
      routeKey: RouteCacheKeys.ListProblemAttempts,
      primaryKey: cacheKey,
      factory: () => _listProblemAttempt(request, cancellationToken),
      ttl: TimeSpan.FromHours(1)
    );

    if (response == null)
    {
      throw new InvalidOperationException("Error listing problem attempts");
    }

    return response;
  }

  private async Task<ListProblemAttemptResponse> _listProblemAttempt(
    ListProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    if (request.SeasonId != null)
    {
      // TODO: Parallelize to speed things up
      foreach (var userId in request.UserIds)
      {
        var existingUser = await _userService.GetUserAsync(
          userId: userId,
          cancellationToken: cancellationToken
        );
        if (existingUser == null)
        {
          throw new KeyNotFoundException(Messages.User.IdDoesNotExist);
        }

        var existingEnrollment = await _enrollmentService.GetEnrollmentBySeasonId(
          request.SeasonId,
          existingUser.UserId,
          null,
          cancellationToken,
          q => q.Include(e => e.User)
        );
        if (existingEnrollment == null || existingEnrollment.User.UserId != userId)
        {
          throw new KeyNotFoundException(Messages.Season.DoesNotExist);
        }
      }
    }

    // Build the query with includes
    Func<IQueryable<ProblemAttemptEntity>, IQueryable<ProblemAttemptEntity>> includeFunc = query =>
    {
      if (request.SeasonId != null)
      {
        query = query.Where(p => p.Enrollment.SeasonId == request.SeasonId);
      }

      if (request.IncludeLeetcode)
      {
        query = query
          .Include(p => p.LeetcodeProblem)
          .ThenInclude(l => l.LeetcodeProblemCategories)
          .Include(p => p.LeetcodeProblem)
          .ThenInclude(l => l.Problem);
      }

      if (request.IncludeCustom)
      {
        query = query.Include(p => p.CustomProblem).ThenInclude(c => c.Problem);
      }

      query = query
        .Include(e => e.User)
        .Include(e => e.Enrollment)
        .ThenInclude(e => e.Season)
        .Include(e => e.SeasonWeek);

      return query;
    };

    // Use GetPagedAsync for pagination
    var (items, totalCount) = await _problemAttemptRepository.GetPagedAsync(
      page: request.Page,
      pageSize: request.PageSize,
      predicate: p => request.UserIds.Contains(p.User.UserId),
      orderBy: q => q.OrderByDescending(p => p.AttemptStartDateUtc),
      include: includeFunc,
      cancellationToken: cancellationToken
    );

    var pagedResponse = RSPWebAPI.Shared.PagedResponse<ProblemAttemptEntity>.Create(
      items.ToList(),
      totalCount,
      request.Page,
      request.PageSize
    );

    return new ListProblemAttemptResponse { Result = pagedResponse };
  }

  public async Task<UpdateProblemAttemptResponse> UpdateProblemAttempt(
    UpdateProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingProblemAttempt = await _problemAttemptRepository.GetByIdAsync(
      request.ProblemAttemptId,
      cancellationToken,
      q => q.Include(p => p.Enrollment)
    );
    if (
      existingProblemAttempt == null
      || request.EnrollmentId != existingProblemAttempt.EnrollmentId
    )
    {
      throw new KeyNotFoundException(Messages.ProblemAttempt.DoesNotExist);
    }

    SeasonWeekEntity? seasonWeek = null;
    var attemptStartDate = existingProblemAttempt.AttemptStartDateUtc;
    if (request.EnrollmentId != null)
    {
      var seasonWeeks = await _seasonWeekService.GetAllSeasonWeeksAsync(
        q =>
          q.StartDate <= attemptStartDate
          && q.EndDate >= attemptStartDate
          && q.SeasonId == existingProblemAttempt.Enrollment.SeasonId,
        cancellationToken
      );
      seasonWeek = seasonWeeks.FirstOrDefault();
      if (seasonWeek == null)
      {
        throw new InvalidOperationException(Messages.ProblemAttempt.OutOfSeasonDateRange);
      }
    }

    // Leetcode should take precedence if CustomProblem is present for some reason
    if (request.CustomProblemId != null && request.LeetcodeProblemId != null)
    {
      request.CustomProblemId = null;
    }

    existingProblemAttempt.LeetcodeProblemId = request.LeetcodeProblemId;
    existingProblemAttempt.CustomProblemId = request.CustomProblemId;
    existingProblemAttempt.Notes = request.Notes;
    existingProblemAttempt.SeasonWeekId = seasonWeek?.SeasonWeekId;

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await UpdateProblemAttemptAsync(existingProblemAttempt, cancellationToken);
        _cache.Remove(RouteCacheKeys.ListProblemAttempts, request.UserId);
        return new UpdateProblemAttemptResponse();
      },
      Messages.ProblemAttempt.UpdateError,
      cancellationToken
    );
  }

  #region CRUD Operations

  public async Task AddProblemAttemptAsync(
    ProblemAttemptEntity problemAttempt,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(problemAttempt);

    await _problemAttemptRepository.AddAsync(problemAttempt, cancellationToken);
  }

  public async Task DeleteProblemAttemptAsync(
    string problemAttemptId,
    CancellationToken cancellationToken = default
  )
  {
    var problemAttempt = await _problemAttemptRepository.GetByIdAsync(
      problemAttemptId,
      cancellationToken
    );

    if (problemAttempt == null)
    {
      throw new KeyNotFoundException(Messages.ProblemAttempt.DoesNotExist);
    }

    _problemAttemptRepository.Delete(problemAttempt, cancellationToken);
  }

  public async Task UpdateProblemAttemptAsync(
    ProblemAttemptEntity problemAttempt,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(problemAttempt);

    var existingProblemAttempt = await _problemAttemptRepository.GetByIdAsync(
      problemAttempt.ProblemAttemptId,
      cancellationToken
    );
    if (existingProblemAttempt == null)
    {
      throw new KeyNotFoundException(Messages.ProblemAttempt.DoesNotExist);
    }

    _problemAttemptRepository.Update(problemAttempt, cancellationToken);
  }

  #endregion
}
