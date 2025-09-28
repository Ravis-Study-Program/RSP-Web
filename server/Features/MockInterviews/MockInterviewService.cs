using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common;
using RSPWEBAPI.Common.Cache;
using RSPWebAPI.Common.Cache;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Extensions;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.MockInterviews.Dtos;
using RSPWebAPI.Features.MockInterviews.Interfaces;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;

namespace RSPWebAPI.Features.MockInterviews;

public class MockInterviewService : BaseService, IMockInterviewService
{
  private readonly IEnrollmentService _enrollmentService;
  private readonly IRepository<MockInterviewEntity> _mockInterviewRepository;
  private readonly ISeasonWeekService _seasonWeekService;
  private readonly IRequestCache _cache;
  private readonly IUserService _userService;

  public MockInterviewService(
    IRepository<MockInterviewEntity> mockInterviewRepository,
    ISeasonWeekService seasonWeekService,
    IUserService userService,
    IEnrollmentService enrollmentService,
    IRequestCache cache,
    IUnitOfWork unitOfWork,
    ILogger<MockInterviewService> logger
  )
    : base(unitOfWork, logger)
  {
    _mockInterviewRepository = mockInterviewRepository;
    _seasonWeekService = seasonWeekService;
    _userService = userService;
    _enrollmentService = enrollmentService;
    _cache = cache;
  }

  public async Task<IEnumerable<MockInterviewEntity>> GetAllMockInterviewsAsync(
    Expression<Func<MockInterviewEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<MockInterviewEntity>, IQueryable<MockInterviewEntity>>? include = null
  )
  {
    return await _mockInterviewRepository.GetAllAsync(predicate, cancellationToken, include);
  }

  public async Task<MockInterviewEntity?> GetMockInterviewByIdAsync(
    string mockInterviewId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<MockInterviewEntity>, IQueryable<MockInterviewEntity>>? include = null
  )
  {
    return await _mockInterviewRepository.GetByIdAsync(mockInterviewId, cancellationToken, include);
  }

  public async Task<CreateMockInterviewResponse> CreateMockInterview(
    CreateMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    SeasonWeekEntity? seasonWeek = null;
    string? seasonId = null;
    if (request.SeasonId != null)
    {
      var existingEnrollment = await _enrollmentService.GetEnrollmentBySeasonId(
        request.SeasonId,
        userId: request.IntervieweeUserId,
        null,
        cancellationToken,
        q => q.Include(e => e.User)
      );
      if (existingEnrollment == null || existingEnrollment.User.UserId != request.IntervieweeUserId)
      {
        throw new KeyNotFoundException(Messages.Enrollment.DoesNotExist);
      }

      seasonId = existingEnrollment.SeasonId;

      var currentDate = request.StartDate;
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
        throw new InvalidOperationException(Messages.MockInterview.OutOfSeasonDateRange);
      }
    }

    var interviewer = await _userService.GetUserAsync(
      userId: request.InterviewerUserId,
      cancellationToken: cancellationToken
    );
    var interviewee = await _userService.GetUserAsync(
      userId: request.IntervieweeUserId,
      cancellationToken: cancellationToken
    );
    if (interviewee == null || interviewer == null)
    {
      throw new KeyNotFoundException(Messages.MockInterview.InterviewerOrIntervieweeCannotBeFound);
    }

    var (mockInterviewId, mockInterviewRounds) = GenerateMockInterviewRounds(
      request.MockInterviewRounds
    );

    var mockInterview = new MockInterviewEntity
    {
      MockInterviewId = mockInterviewId,
      IsPass = IsAllScoresAboveThreshold(request.MockInterviewRounds),
      InterviewerUserId = interviewer.UserId,
      IntervieweeUserId = interviewee.UserId,
      SeasonId = seasonId,
      MockInterviewRounds = mockInterviewRounds,
      StartDate = request.StartDate,
      TimeTakenInMinutes = request.TimeTakenInMinutes,
      Notes = request.Notes,
      SeasonWeekId = seasonWeek?.SeasonWeekId,
    };

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await AddMockInterviewAsync(mockInterview, cancellationToken);
        _cache.Remove(RouteCacheKeys.ListMockInterviews, interviewee.UserId);
        _cache.Remove(RouteCacheKeys.ListMockInterviews, interviewer.UserId);
        return new CreateMockInterviewResponse { MockInterviewId = mockInterview.MockInterviewId };
      },
      Messages.MockInterview.CreationError,
      cancellationToken: cancellationToken
    );
  }

  public async Task<DeleteMockInterviewResponse> DeleteMockInterview(
    DeleteMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingMockInterview = await GetMockInterviewByIdAsync(
      request.MockInterviewId,
      cancellationToken,
      q => q.Include(m => m.Interviewer).Include(m => m.Interviewee)
    );
    if (existingMockInterview == null)
    {
      throw new KeyNotFoundException(Messages.MockInterview.DoesNotExist);
    }
    if (existingMockInterview.Interviewer.UserId != request.UserId)
    {
      throw new ArgumentException(Messages.MockInterview.DeletionOnlyInterviewerAllowed);
    }

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await DeleteMockInterviewAsync(existingMockInterview.MockInterviewId, cancellationToken);
        _cache.Remove(RouteCacheKeys.ListMockInterviews, existingMockInterview.Interviewer.UserId);
        _cache.Remove(RouteCacheKeys.ListMockInterviews, existingMockInterview.Interviewee.UserId);
        return new DeleteMockInterviewResponse();
      },
      Messages.MockInterview.DeletionError,
      cancellationToken: cancellationToken
    );
  }

  public async Task<ListMockInterviewResponse> ListMockInterview(
    ListMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var allMockInterviews = new List<MockInterviewEntity>();

    foreach (var userId in request.UserIds.Distinct())
    {
      var modifiedRequest = new ListMockInterviewRequest
      {
        UserIds = new List<string> { userId },
        SeasonId = request.SeasonId,
        IncludeLeetcode = request.IncludeLeetcode,
        IncludeCustom = request.IncludeCustom,
        IncludeBehavioural = request.IncludeBehavioural,
      };

      var response = await _cache.GetOrCreateAsync(
        routeKey: RouteCacheKeys.ListMockInterviews,
        primaryKey: userId,
        factory: () => _listMockInterview(modifiedRequest, cancellationToken),
        ttl: TimeSpan.FromHours(1)
      );
      if (response == null)
      {
        throw new InvalidOperationException("Error listing mock interviews");
      }

      allMockInterviews.AddRange(response.MockInterviews);
    }

    return new ListMockInterviewResponse { MockInterviews = allMockInterviews };
  }

  private async Task<ListMockInterviewResponse> _listMockInterview(
    ListMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var query = _mockInterviewRepository.Table;
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

      query = query.Where(m => m.SeasonId == request.SeasonId);
    }

    if (request.IncludeBehavioural)
    {
      query = query
        .Include(m => m.MockInterviewRounds)
        .ThenInclude(mr => mr.BehaviouralMockInterviewRound);
    }

    if (request.IncludeLeetcode)
    {
      query = query
        .Include(m => m.MockInterviewRounds)
        .ThenInclude(mr => mr.LeetcodeMockInterviewRound)
        .ThenInclude(l => l.LeetcodeProblem)
        .ThenInclude(l => l.Problem);
    }

    if (request.IncludeCustom)
    {
      query = query
        .Include(m => m.MockInterviewRounds)
        .ThenInclude(mr => mr.CustomMockInterviewRound);
    }

    query = query
      .Include(e => e.Interviewer)
      .Include(e => e.Interviewee)
      .Include(e => e.SeasonWeek)
      .Include(e => e.Season);

    var mockInterviews = await _mockInterviewRepository.GetAllAsync(
      m =>
        request.UserIds.Contains(m.Interviewer.UserId)
        || request.UserIds.Contains(m.Interviewee.UserId),
      cancellationToken,
      _ => query
    );

    return new ListMockInterviewResponse { MockInterviews = mockInterviews.ToList() };
  }

  public async Task<UpdateMockInterviewResponse> UpdateMockInterview(
    UpdateMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingMockInterview = await _mockInterviewRepository.GetByIdAsync(
      request.MockInterviewId,
      cancellationToken,
      q =>
        q.Include(m => m.Season)
          .Include(m => m.Interviewer)
          .Include(m => m.MockInterviewRounds)
          .ThenInclude(r => r.BehaviouralMockInterviewRound)
          .Include(m => m.MockInterviewRounds)
          .ThenInclude(r => r.CustomMockInterviewRound)
          .Include(m => m.MockInterviewRounds)
          .ThenInclude(r => r.LeetcodeMockInterviewRound)
    );
    if (existingMockInterview == null || request.SeasonId != existingMockInterview.SeasonId)
    {
      throw new KeyNotFoundException(Messages.MockInterview.DoesNotExist);
    }

    if (request.InterviewerUserId != existingMockInterview.Interviewer.UserId)
    {
      throw new ArgumentException(Messages.MockInterview.UpdateOnlyInterviewerAllowed);
    }

    SeasonWeekEntity? seasonWeek = null;
    if (request.SeasonId != null)
    {
      var currentDate = request.StartDate;
      var seasonWeeks = await _seasonWeekService.GetAllSeasonWeeksAsync(
        q =>
          q.StartDate <= currentDate
          && q.EndDate >= currentDate
          && q.SeasonId == existingMockInterview.SeasonId,
        cancellationToken
      );
      seasonWeek = seasonWeeks.FirstOrDefault();
      if (seasonWeek == null)
      {
        throw new InvalidOperationException(Messages.MockInterview.OutOfSeasonDateRange);
      }
    }

    var interviewer = await _userService.GetUserAsync(
      userId: request.InterviewerUserId,
      cancellationToken: cancellationToken
    );
    var interviewee = await _userService.GetUserAsync(
      userId: request.IntervieweeUserId,
      cancellationToken: cancellationToken
    );
    if (interviewee == null || interviewer == null)
    {
      throw new KeyNotFoundException(Messages.MockInterview.InterviewerOrIntervieweeCannotBeFound);
    }

    UpdateMockInterviewRounds(
      request.MockInterviewRounds,
      existingMockInterview.MockInterviewRounds
    );

    existingMockInterview.IsPass = IsAllScoresAboveThreshold(request.MockInterviewRounds);
    existingMockInterview.InterviewerUserId = interviewer.UserId;
    existingMockInterview.IntervieweeUserId = interviewee.UserId;
    existingMockInterview.SeasonId = request.SeasonId;
    existingMockInterview.StartDate = request.StartDate;
    existingMockInterview.TimeTakenInMinutes = request.TimeTakenInMinutes;
    existingMockInterview.Notes = request.Notes;
    existingMockInterview.SeasonWeekId = seasonWeek?.SeasonWeekId;

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await UpdateMockInterviewAsync(existingMockInterview, cancellationToken);
        _cache.Remove(RouteCacheKeys.ListMockInterviews, interviewee.UserId);
        _cache.Remove(RouteCacheKeys.ListMockInterviews, interviewer.UserId);
        return new UpdateMockInterviewResponse();
      },
      Messages.MockInterview.UpdateError,
      cancellationToken: cancellationToken
    );
  }

  public void UpdateMockInterviewRounds(
    List<MockInterviewRoundDto> newMockInterviewRounds,
    ICollection<MockInterviewRoundEntity> existingRounds
  )
  {
    foreach (var newRound in newMockInterviewRounds)
    {
      // Find the corresponding tracked entity based on the ID.
      var trackedRound = existingRounds.FirstOrDefault(r =>
        r.MockInterviewRoundId == newRound.MockInterviewRoundId
      );

      if (trackedRound == null)
      {
        continue;
      }

      // Update Behavioral Round values
      if (newRound.BehaviouralMockInterviewRound != null)
      {
        trackedRound.BehaviouralMockInterviewRound.BehavioralScore = newRound
          .BehaviouralMockInterviewRound
          .BehavioralScore;
      }

      // Update Leetcode Round values
      if (newRound.LeetcodeMockInterviewRound != null)
      {
        trackedRound.LeetcodeMockInterviewRound.LeetcodeProblemId = newRound
          .LeetcodeMockInterviewRound
          .LeetcodeProblemId;
        trackedRound.LeetcodeMockInterviewRound.ConfirmQuestionScore = newRound
          .LeetcodeMockInterviewRound
          .ConfirmQuestionScore;
        trackedRound.LeetcodeMockInterviewRound.AlgorithmDesignScore = newRound
          .LeetcodeMockInterviewRound
          .AlgorithmDesignScore;
        trackedRound.LeetcodeMockInterviewRound.ComplexityAnalysisScore = newRound
          .LeetcodeMockInterviewRound
          .ComplexityAnalysisScore;
        trackedRound.LeetcodeMockInterviewRound.CodingScore = newRound
          .LeetcodeMockInterviewRound
          .CodingScore;
        trackedRound.LeetcodeMockInterviewRound.TestingScore = newRound
          .LeetcodeMockInterviewRound
          .TestingScore;
      }

      // Update Custom Round values
      if (newRound.CustomMockInterviewRound != null)
      {
        trackedRound.CustomMockInterviewRound.Score = newRound.CustomMockInterviewRound.Score;
        trackedRound.CustomMockInterviewRound.Link = newRound.CustomMockInterviewRound.Link;
        trackedRound.CustomMockInterviewRound.Content = newRound.CustomMockInterviewRound.Content;
      }
    }
  }

  public (string, List<MockInterviewRoundEntity>) GenerateMockInterviewRounds(
    List<MockInterviewRoundDto> mockInterviewRoundDtos
  )
  {
    // Prepare list to store mock interview rounds and related entities
    var mockInterviewRounds = new List<MockInterviewRoundEntity>();
    var mockInterviewId = Database.Constants.GeneratePrimaryKeyId();

    foreach (var m in mockInterviewRoundDtos)
    {
      var mockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId();
      var mockInterviewRound = new MockInterviewRoundEntity
      {
        MockInterviewRoundId = mockInterviewRoundId,
        MockInterviewId = mockInterviewId,
        IsReviewedByInterviewee = false,
        IntervieweeComment = "",
      };

      // Add BehaviouralMockInterviewRound if present, and avoid other types in this round
      if (m.BehaviouralMockInterviewRound != null)
      {
        mockInterviewRound.BehaviouralMockInterviewRound = new BehaviouralMockInterviewRoundEntity
        {
          BehaviouralMockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
          BehavioralScore = m.BehaviouralMockInterviewRound.BehavioralScore,
        };
      }

      // Add LeetcodeMockInterviewRound if present
      if (m.LeetcodeMockInterviewRound != null)
      {
        mockInterviewRound.LeetcodeMockInterviewRound = new LeetcodeMockInterviewRoundEntity
        {
          LeetcodeMockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
          ConfirmQuestionScore = m.LeetcodeMockInterviewRound.ConfirmQuestionScore,
          AlgorithmDesignScore = m.LeetcodeMockInterviewRound.AlgorithmDesignScore,
          ComplexityAnalysisScore = m.LeetcodeMockInterviewRound.ComplexityAnalysisScore,
          CodingScore = m.LeetcodeMockInterviewRound.CodingScore,
          TestingScore = m.LeetcodeMockInterviewRound.TestingScore,
          LeetcodeProblemId = m.LeetcodeMockInterviewRound.LeetcodeProblemId,
        };
      }

      // Add CustomMockInterviewRound if present
      if (m.CustomMockInterviewRound != null)
      {
        mockInterviewRound.CustomMockInterviewRound = new CustomMockInterviewRoundEntity
        {
          CustomMockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
          Content = m.CustomMockInterviewRound.Content,
          Score = m.CustomMockInterviewRound.Score,
          Link = m.CustomMockInterviewRound.Link,
        };
      }

      // Add the constructed mock interview round to the list
      mockInterviewRounds.Add(mockInterviewRound);
    }

    return (mockInterviewId, mockInterviewRounds);
  }

  private static bool IsAllScoresAboveThreshold(List<MockInterviewRoundDto> mockInterviewRounds)
  {
    const int threshold = 5;

    var numList = new List<int>();
    foreach (var round in mockInterviewRounds)
    {
      if (round.LeetcodeMockInterviewRound != null)
      {
        var r = round.LeetcodeMockInterviewRound;
        numList.AddRange(
          new List<int>
          {
            r.ConfirmQuestionScore,
            r.AlgorithmDesignScore,
            r.ComplexityAnalysisScore,
            r.CodingScore,
            r.TestingScore,
          }
        );
      }

      if (round.BehaviouralMockInterviewRound != null)
      {
        var r = round.BehaviouralMockInterviewRound;
        numList.AddRange(new List<int> { r.BehavioralScore });
      }

      if (round.CustomMockInterviewRound != null)
      {
        var r = round.CustomMockInterviewRound;
        numList.AddRange(new List<int> { r.Score });
      }
    }

    foreach (var num in numList)
    {
      if (num < threshold)
      {
        return false;
      }
    }

    return true;
  }

  #region CRUD Operations

  public async Task AddMockInterviewAsync(
    MockInterviewEntity mockInterview,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(mockInterview);

    await _mockInterviewRepository.AddAsync(mockInterview, cancellationToken);
  }

  public async Task DeleteMockInterviewAsync(
    string mockInterviewId,
    CancellationToken cancellationToken = default
  )
  {
    var mockInterview = await _mockInterviewRepository.GetByIdAsync(
      mockInterviewId,
      cancellationToken: cancellationToken
    );

    if (mockInterview == null)
    {
      throw new KeyNotFoundException(Messages.MockInterview.DoesNotExist);
    }

    _mockInterviewRepository.Delete(mockInterview, cancellationToken);
  }

  public async Task UpdateMockInterviewAsync(
    MockInterviewEntity mockInterview,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(mockInterview);

    var existingMockInterview = await _mockInterviewRepository.GetByIdAsync(
      mockInterview.MockInterviewId,
      cancellationToken: cancellationToken
    );
    if (existingMockInterview == null)
    {
      throw new KeyNotFoundException(Messages.MockInterview.DoesNotExist);
    }

    _mockInterviewRepository.Update(mockInterview, cancellationToken);
  }

  #endregion
}
