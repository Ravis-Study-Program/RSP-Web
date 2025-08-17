using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Cache;
using RSPWEBAPI.Common.Cache;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.ProblemAttempts.Dtos;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;

namespace RSPWebAPI.Features.ProblemAttempts;

public class ProblemAttemptService : IProblemAttemptService
{
  private readonly IEnrollmentService _enrollmentService;
  private readonly ILogger<ProblemAttemptService> _logger;
  private readonly IRepository<ProblemAttemptEntity> _problemAttemptRepository;
  private readonly ISeasonWeekService _seasonWeekService;
  private readonly IRequestCache _cache;
  private readonly IUnitOfWork _unitOfWork;
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
  {
    _problemAttemptRepository = problemAttemptRepository;
    _seasonWeekService = seasonWeekService;
    _userService = userService;
    _enrollmentService = enrollmentService;
    _cache = cache;
    _unitOfWork = unitOfWork;
    _logger = logger;
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

  public async Task<IServiceResponse<CreateProblemAttemptResponse>> CreateProblemAttempt(
    CreateProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    SeasonWeekEntity? seasonWeek = null;
    if (request.EnrollmentId != null)
    {
      var existingEnrollment = await _enrollmentService.GetEnrollmentByIdAsync(
        request.EnrollmentId,
        cancellationToken,
        q => q.Include(e => e.User)
      );
      if (existingEnrollment == null || existingEnrollment.User.Email != request.Email)
      {
        return new ErrorServiceResponse<CreateProblemAttemptResponse>(
          Messages.Enrollment.DoesNotExist
        );
      }

      var currentDate = request.AttemptStartDateUtc;
      var seasonWeeks = await _seasonWeekService.GetAllSeasonWeeksAsync(
        q =>
          q.StartDate <= currentDate
          && q.EndDate >= currentDate
          && q.SeasonId == existingEnrollment.SeasonId,
        cancellationToken
      );
      seasonWeek = seasonWeeks.FirstOrDefault();
      if (seasonWeek == null)
      {
        return new ErrorServiceResponse<CreateProblemAttemptResponse>(
          Messages.ProblemAttempt.OutOfSeasonDateRange
        );
      }
    }

    // Leetcode should take precedence if CustomProblem is present
    if (request.CustomProblemId != null && request.LeetcodeProblemId != null)
    {
      request.CustomProblemId = null;
    }

    var user = await _userService.GetUserByEmailAsync(request.Email, cancellationToken);
    if (user == null)
    {
      return new ErrorServiceResponse<CreateProblemAttemptResponse>(
        Messages.User.EmailDoesNotExist
      );
    }

    var problemAttempt = new ProblemAttemptEntity
    {
      ProblemAttemptId = Database.Constants.GeneratePrimaryKeyId(),
      AttemptStartDateUtc = request.AttemptStartDateUtc,
      TimeTakenInMinutes = request.TimeTakenInMinutes,
      LeetcodeProblemId = request.LeetcodeProblemId,
      CustomProblemId = request.CustomProblemId,
      Notes = request.Notes,
      EnrollmentId = request.EnrollmentId,
      UserId = user.UserId,
      SeasonWeekId = seasonWeek?.SeasonWeekId,
    };

    try
    {
      await AddProblemAttemptAsync(problemAttempt, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      _cache.Remove(RouteCacheKeys.ListProblemAttempts, request.Email);
      return new SuccessServiceResponse<CreateProblemAttemptResponse>(
        Messages.ProblemAttempt.Created,
        new CreateProblemAttemptResponse { ProblemAttemptId = problemAttempt.ProblemAttemptId }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.ProblemAttempt.CreationError);
      return new ErrorServiceResponse<CreateProblemAttemptResponse>(
        Messages.ProblemAttempt.CreationError
      );
    }
  }

  public async Task<IServiceResponse<DeleteProblemAttemptResponse>> DeleteProblemAttempt(
    DeleteProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingProblemAttempt = await GetProblemAttemptByIdAsync(
      request.ProblemAttemptId,
      cancellationToken,
      q => q.Include(p => p.User)
    );
    if (existingProblemAttempt == null || existingProblemAttempt.User.Email != request.Email)
    {
      return new ErrorServiceResponse<DeleteProblemAttemptResponse>(
        Messages.ProblemAttempt.DoesNotExist
      );
    }

    try
    {
      await DeleteProblemAttemptAsync(existingProblemAttempt.ProblemAttemptId, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      _cache.Remove(RouteCacheKeys.ListProblemAttempts, request.Email);
      return new SuccessServiceResponse<DeleteProblemAttemptResponse>(
        Messages.ProblemAttempt.Deleted
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.ProblemAttempt.DeletionError);
      return new ErrorServiceResponse<DeleteProblemAttemptResponse>(
        Messages.ProblemAttempt.DeletionError
      );
    }
  }

  public async Task<IServiceResponse<ListProblemAttemptResponse>> ListProblemAttempt(
    ListProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var allProblemAttempts = new List<ProblemAttemptEntity>();

    foreach (var email in request.Emails.Distinct())
    {
      var modifiedRequest = new ListProblemAttemptRequest
      {
        Emails = new List<string> { email },
        SeasonId = request.SeasonId,
        IncludeLeetcode = request.IncludeLeetcode,
        IncludeCustom = request.IncludeCustom,
      };

      var response = await _cache.GetOrCreateAsync(
        routeKey: RouteCacheKeys.ListProblemAttempts,
        primaryKey: email,
        factory: () => _listProblemAttempt(modifiedRequest, cancellationToken),
        ttl: TimeSpan.FromHours(1)
      );
      if (response == null)
      {
        throw new Exception("Error listing problem attempts");
      }

      // Early return if any failure occurs
      if (!response.IsSuccess || response.Data == null)
      {
        return response;
      }

      allProblemAttempts.AddRange(response.Data.ProblemAttempts);
    }

    return new SuccessServiceResponse<ListProblemAttemptResponse>(
      Messages.ProblemAttempt.Listed,
      new ListProblemAttemptResponse { ProblemAttempts = allProblemAttempts }
    );
  }

  private async Task<IServiceResponse<ListProblemAttemptResponse>> _listProblemAttempt(
    ListProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var query = _problemAttemptRepository.Table;

    if (request.SeasonId != null)
    {
      // TODO: Parallelize to speed things up
      foreach (var email in request.Emails)
      {
        var existingUser = await _userService.GetUserByEmailAsync(email, cancellationToken);
        if (existingUser == null)
        {
          return new ErrorServiceResponse<ListProblemAttemptResponse>(Messages.User.IdDoesNotExist);
        }

        var existingEnrollment = await _enrollmentService.GetEnrollmentBySeasonId(
          request.SeasonId,
          existingUser.UserId,
          null,
          cancellationToken,
          q => q.Include(e => e.User)
        );
        if (existingEnrollment == null || existingEnrollment.User.Email != email)
        {
          return new ErrorServiceResponse<ListProblemAttemptResponse>(Messages.Season.DoesNotExist);
        }
      }

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

    var problemAttempts = await _problemAttemptRepository.GetAllAsync(
      p => request.Emails.Contains(p.User.Email),
      cancellationToken,
      _ => query
    );

    try
    {
      return new SuccessServiceResponse<ListProblemAttemptResponse>(
        Messages.ProblemAttempt.Listed,
        new ListProblemAttemptResponse { ProblemAttempts = problemAttempts.ToList() }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.ProblemAttempt.ListError);
      return new ErrorServiceResponse<ListProblemAttemptResponse>(
        Messages.ProblemAttempt.ListError
      );
    }
  }

  public async Task<IServiceResponse<UpdateProblemAttemptResponse>> UpdateProblemAttempt(
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
      return new ErrorServiceResponse<UpdateProblemAttemptResponse>(
        Messages.ProblemAttempt.DoesNotExist
      );
    }

    SeasonWeekEntity? seasonWeek = null;
    if (request.EnrollmentId != null)
    {
      var currentDate = request.AttemptStartDateUtc;
      var seasonWeeks = await _seasonWeekService.GetAllSeasonWeeksAsync(
        q =>
          q.StartDate <= currentDate
          && q.EndDate >= currentDate
          && q.SeasonId == existingProblemAttempt.Enrollment.SeasonId,
        cancellationToken
      );
      seasonWeek = seasonWeeks.FirstOrDefault();
      if (seasonWeek == null)
      {
        return new ErrorServiceResponse<UpdateProblemAttemptResponse>(
          Messages.ProblemAttempt.OutOfSeasonDateRange
        );
      }
    }

    // Leetcode should take precedence if CustomProblem is present for some reason
    if (request.CustomProblemId != null && request.LeetcodeProblemId != null)
    {
      request.CustomProblemId = null;
    }

    existingProblemAttempt.AttemptStartDateUtc = request.AttemptStartDateUtc;
    existingProblemAttempt.TimeTakenInMinutes = request.TimeTakenInMinutes;
    existingProblemAttempt.LeetcodeProblemId = request.LeetcodeProblemId;
    existingProblemAttempt.CustomProblemId = request.CustomProblemId;
    existingProblemAttempt.Notes = request.Notes;
    existingProblemAttempt.SeasonWeekId = seasonWeek?.SeasonWeekId;

    try
    {
      await UpdateProblemAttemptAsync(existingProblemAttempt, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      _cache.Remove(RouteCacheKeys.ListProblemAttempts, request.Email);
      return new SuccessServiceResponse<UpdateProblemAttemptResponse>(
        Messages.ProblemAttempt.Updated
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.ProblemAttempt.UpdateError);
      return new ErrorServiceResponse<UpdateProblemAttemptResponse>(
        Messages.ProblemAttempt.UpdateError
      );
    }
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
