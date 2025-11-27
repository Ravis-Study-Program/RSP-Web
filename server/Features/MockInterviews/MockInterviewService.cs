using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
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
using server.Shared;

namespace RSPWebAPI.Features.MockInterviews;

public class MockInterviewService : BaseService, IMockInterviewService
{
  private readonly IEnrollmentService _enrollmentService;
  private readonly IRepository<MockInterviewEntity> _mockInterviewRepository;
  private readonly IRepository<CustomMockInterviewRoundEntity> _customMockInterviewRoundRepository;
  private readonly IRepository<LeetcodeMockInterviewRoundEntity> _leetcodeMockInterviewRoundRepository;
  private readonly ISeasonWeekService _seasonWeekService;
  private readonly IRequestCache _cache;
  private readonly IUserService _userService;

  public MockInterviewService(
    IRepository<MockInterviewEntity> mockInterviewRepository,
    IRepository<CustomMockInterviewRoundEntity> customMockInterviewRoundRepository,
    IRepository<LeetcodeMockInterviewRoundEntity> leetcodeMockInterviewRoundRepository,
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
    _customMockInterviewRoundRepository = customMockInterviewRoundRepository;
    _leetcodeMockInterviewRoundRepository = leetcodeMockInterviewRoundRepository;
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
    var startDate = DateTime.UtcNow.AddMinutes(-request.TimeTakenInMinutes);
    if (!string.IsNullOrEmpty(request.SeasonId))
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

      var seasonWeeks = await _seasonWeekService.GetAllSeasonWeeksAsync(
        q =>
          q.StartDate <= startDate
          && q.EndDate >= startDate
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
      StartDate = startDate,
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

  public async Task<ListMockInterviewCursorResponse> ListMockInterviewWithCursor(
    ListMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var pageSize = request.PageSize ?? 10; // Default to 10
    var cursor = ParseCursor(request.Cursor);

    // Build the query with includes
    var includeFunc = BuildIncludeQuery(request);

    // Build the predicate
    var predicate = BuildPredicate(request);

    // Call repository with cursor pagination
    var (items, hasMore, hasPrevious) = await _mockInterviewRepository.GetPagedWithCursorAsync(
      pageSize: pageSize,
      predicate: predicate,
      orderBy: q => q.OrderByDescending(m => m.CreatedAtUtc).ThenByDescending(m => m.MockInterviewId),
      cursorSelector: entity => (entity.CreatedAtUtc, entity.MockInterviewId),
      cursor: cursor,
      forward: request.Forward,
      include: includeFunc,
      cancellationToken: cancellationToken
    );

    // Get total count (expensive, only if needed by UI)
    int? totalCount = null;
    if (request.PageSize.HasValue) // Only compute if pagination is being used
    {
      totalCount = await _mockInterviewRepository.Table
        .Where(predicate)
        .CountAsync(cancellationToken);
    }

    // Build next cursor from last item
    string? nextCursor = null;
    if (hasMore && items.Count > 0)
    {
      var lastItem = items.Last();
      nextCursor = EncodeCursor(lastItem.CreatedAtUtc, lastItem.MockInterviewId);
    }
    string? previousCursor = null;
    if (hasPrevious && items.Count > 0)
    {
      var firstItem = items.First();
      previousCursor = EncodeCursor(firstItem.CreatedAtUtc, firstItem.MockInterviewId);
    }

    return new ListMockInterviewCursorResponse
    {
      Items = items.ToList(),
      NextCursor = nextCursor,
      PreviousCursor = previousCursor,
      HasMore = hasMore,
      TotalCount = totalCount
    };
  }

  private Expression<Func<MockInterviewEntity, bool>> BuildPredicate(ListMockInterviewRequest request)
  {
    return m =>
      (request.UserIds.Contains(m.InterviewerUserId) || request.UserIds.Contains(m.IntervieweeUserId))
      && (string.IsNullOrEmpty(request.SeasonId) || m.SeasonId == request.SeasonId);
  }

  private Func<IQueryable<MockInterviewEntity>, IQueryable<MockInterviewEntity>> BuildIncludeQuery(
    ListMockInterviewRequest request
  )
  {
    return query =>
    {
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

      return query;
    };
  }

  private (DateTime timestamp, string id)? ParseCursor(string? cursorString)
  {
    if (string.IsNullOrEmpty(cursorString))
      return null;

    try
    {
      var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursorString));
      var cursor = JsonSerializer.Deserialize<Cursor>(json);
      if (cursor == null)
        return null;

      return (cursor.CreatedAtUtc, cursor.Id);
    }
    catch
    {
      return null; // Invalid cursor, treat as first page
    }
  }

  private string EncodeCursor(DateTime timestamp, string id)
  {
    var cursor = new Cursor
    {
      CreatedAtUtc = timestamp,
      Id = id
    };
    var json = JsonSerializer.Serialize(cursor);
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
  }

  private async Task<ListMockInterviewResponse> _listMockInterview(
    ListMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var query = _mockInterviewRepository.Table;
    if (!string.IsNullOrEmpty(request.SeasonId))
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
    var startDate = existingMockInterview.StartDate;
    if (!string.IsNullOrEmpty(request.SeasonId))
    {
      var seasonWeeks = await _seasonWeekService.GetAllSeasonWeeksAsync(
        q =>
          q.StartDate <= startDate
          && q.EndDate >= startDate
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
    var updatedRounds = new List<MockInterviewRoundEntity>();

    foreach (var newRound in newMockInterviewRounds)
    {
      MockInterviewRoundEntity? trackedRound = null;
      
      // Only try to find existing round if we have an ID
      if (!string.IsNullOrEmpty(newRound.MockInterviewRoundId))
      {
        // Find the corresponding tracked entity based on the ID and type compatibility
        trackedRound = existingRounds.FirstOrDefault(r =>
          r.MockInterviewRoundId == newRound.MockInterviewRoundId && CanUpdateRoundType(r, newRound)
        );
      }

      if (trackedRound != null)
      {
        // Update existing round of the same type
        UpdateExistingRound(trackedRound, newRound);
        updatedRounds.Add(trackedRound);
      }
      else
      {
        // Create new round (either completely new or replacing an incompatible type)
        var newRoundEntity = CreateNewRound(newRound);
        updatedRounds.Add(newRoundEntity);
      }
    }

    // Clear existing rounds and replace with updated rounds
    existingRounds.Clear();
    foreach (var round in updatedRounds)
    {
      existingRounds.Add(round);
    }
  }

  private static bool CanUpdateRoundType(MockInterviewRoundEntity existing, MockInterviewRoundDto newRound)
  {
    // Check if the round type matches what we're trying to update
    if (newRound.BehaviouralMockInterviewRound != null)
      return existing.BehaviouralMockInterviewRound != null;
    
    if (newRound.LeetcodeMockInterviewRound != null)
      return existing.LeetcodeMockInterviewRound != null;
    
    if (newRound.CustomMockInterviewRound != null)
      return existing.CustomMockInterviewRound != null;

    return false;
  }

  private static void UpdateExistingRound(MockInterviewRoundEntity existing, MockInterviewRoundDto newRound)
  {
    // Update Behavioral Round values
    if (newRound.BehaviouralMockInterviewRound != null && existing.BehaviouralMockInterviewRound != null)
    {
      existing.BehaviouralMockInterviewRound.BehavioralScore = newRound
        .BehaviouralMockInterviewRound
        .BehavioralScore;
    }

    // Update Leetcode Round values
    if (newRound.LeetcodeMockInterviewRound != null && existing.LeetcodeMockInterviewRound != null)
    {
      existing.LeetcodeMockInterviewRound.LeetcodeProblemId = newRound
        .LeetcodeMockInterviewRound
        .LeetcodeProblemId;
      existing.LeetcodeMockInterviewRound.ConfirmQuestionScore = newRound
        .LeetcodeMockInterviewRound
        .ConfirmQuestionScore;
      existing.LeetcodeMockInterviewRound.AlgorithmDesignScore = newRound
        .LeetcodeMockInterviewRound
        .AlgorithmDesignScore;
      existing.LeetcodeMockInterviewRound.ComplexityAnalysisScore = newRound
        .LeetcodeMockInterviewRound
        .ComplexityAnalysisScore;
      existing.LeetcodeMockInterviewRound.CodingScore = newRound
        .LeetcodeMockInterviewRound
        .CodingScore;
      existing.LeetcodeMockInterviewRound.TestingScore = newRound
        .LeetcodeMockInterviewRound
        .TestingScore;
    }

    // Update Custom Round values
    if (newRound.CustomMockInterviewRound != null && existing.CustomMockInterviewRound != null)
    {
      existing.CustomMockInterviewRound.Score = newRound.CustomMockInterviewRound.Score;
      existing.CustomMockInterviewRound.Link = newRound.CustomMockInterviewRound.Link;
      existing.CustomMockInterviewRound.Content = newRound.CustomMockInterviewRound.Content;
    }
  }

  private static MockInterviewRoundEntity CreateNewRound(MockInterviewRoundDto roundDto)
  {
    var mockInterviewRound = new MockInterviewRoundEntity
    {
      MockInterviewRoundId = !string.IsNullOrEmpty(roundDto.MockInterviewRoundId) 
        ? roundDto.MockInterviewRoundId 
        : Database.Constants.GeneratePrimaryKeyId(),
      IsReviewedByInterviewee = false,
      IntervieweeComment = "",
    };

    // Add BehaviouralMockInterviewRound if present
    if (roundDto.BehaviouralMockInterviewRound != null)
    {
      mockInterviewRound.BehaviouralMockInterviewRound = new BehaviouralMockInterviewRoundEntity
      {
        BehaviouralMockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
        BehavioralScore = roundDto.BehaviouralMockInterviewRound.BehavioralScore,
      };
    }

    // Add LeetcodeMockInterviewRound if present
    if (roundDto.LeetcodeMockInterviewRound != null)
    {
      mockInterviewRound.LeetcodeMockInterviewRound = new LeetcodeMockInterviewRoundEntity
      {
        LeetcodeMockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
        ConfirmQuestionScore = roundDto.LeetcodeMockInterviewRound.ConfirmQuestionScore,
        AlgorithmDesignScore = roundDto.LeetcodeMockInterviewRound.AlgorithmDesignScore,
        ComplexityAnalysisScore = roundDto.LeetcodeMockInterviewRound.ComplexityAnalysisScore,
        CodingScore = roundDto.LeetcodeMockInterviewRound.CodingScore,
        TestingScore = roundDto.LeetcodeMockInterviewRound.TestingScore,
        LeetcodeProblemId = roundDto.LeetcodeMockInterviewRound.LeetcodeProblemId,
      };
    }

    // Add CustomMockInterviewRound if present
    if (roundDto.CustomMockInterviewRound != null)
    {
      mockInterviewRound.CustomMockInterviewRound = new CustomMockInterviewRoundEntity
      {
        CustomMockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
        Content = roundDto.CustomMockInterviewRound.Content,
        Score = roundDto.CustomMockInterviewRound.Score,
        Link = roundDto.CustomMockInterviewRound.Link,
      };
    }

    return mockInterviewRound;
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
        numList.AddRange([
          r.ConfirmQuestionScore,
          r.AlgorithmDesignScore,
          r.ComplexityAnalysisScore,
          r.CodingScore,
          r.TestingScore,
        ]);
      }

      if (round.BehaviouralMockInterviewRound != null)
      {
        var r = round.BehaviouralMockInterviewRound;
        numList.AddRange([r.BehavioralScore]);
      }

      if (round.CustomMockInterviewRound != null)
      {
        var r = round.CustomMockInterviewRound;
        numList.AddRange([r.Score]);
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

    ArgumentNullException.ThrowIfNull(mockInterview, Messages.MockInterview.DoesNotExist);

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
    ArgumentNullException.ThrowIfNull(existingMockInterview, Messages.MockInterview.DoesNotExist);

    _mockInterviewRepository.Update(mockInterview, cancellationToken);
  }

  public async Task<UpdateMockInterviewRoundReviewResponse> UpdateCustomMockInterviewRoundReview(
    UpdateCustomMockInterviewRoundReviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
      var customRound = await _customMockInterviewRoundRepository.GetByIdAsync(
        request.CustomMockInterviewRoundId,
        cancellationToken
      );
      if (customRound == null)
      {
        throw new KeyNotFoundException(Messages.MockInterview.CustomRoundNotFound);
      }

      var mockInterview = await GetMockInterviewForCustomRound(request.CustomMockInterviewRoundId, cancellationToken);
      if (mockInterview == null)
      {
        throw new KeyNotFoundException(Messages.MockInterview.DoesNotExist);
      }

      if (mockInterview.IntervieweeUserId != request.UserId)
      {
        throw new UnauthorizedAccessException(Messages.MockInterview.OnlyIntervieweeCanUpdateReview);
      }

    return await ExecuteWithSaveAsync(
      () =>
      {
        customRound.IsReviewed = request.IsReviewed;
        _customMockInterviewRoundRepository.Update(customRound, cancellationToken);

        _cache.Remove(RouteCacheKeys.ListMockInterviews, mockInterview.IntervieweeUserId);
        _cache.Remove(RouteCacheKeys.ListMockInterviews, mockInterview.InterviewerUserId);

        return Task.FromResult(new UpdateMockInterviewRoundReviewResponse
        {
          Message = Messages.MockInterview.RoundReviewUpdated
        });
      },
      Messages.MockInterview.CustomRoundUpdateError,
      cancellationToken: cancellationToken
    );
  }

  public async Task<UpdateMockInterviewRoundReviewResponse> UpdateLeetcodeMockInterviewRoundReview(
    UpdateLeetcodeMockInterviewRoundReviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var leetcodeRound = await _leetcodeMockInterviewRoundRepository.GetByIdAsync(
      request.LeetcodeMockInterviewRoundId,
      cancellationToken
    );
    if (leetcodeRound == null)
    {
      throw new KeyNotFoundException(Messages.MockInterview.LeetcodeRoundNotFound);
    }

    var mockInterview = await GetMockInterviewForLeetcodeRound(request.LeetcodeMockInterviewRoundId, cancellationToken);
    if (mockInterview == null)
    {
      throw new KeyNotFoundException(Messages.MockInterview.DoesNotExist);
    }

    if (mockInterview.IntervieweeUserId != request.UserId)
    {
      throw new UnauthorizedAccessException(Messages.MockInterview.OnlyIntervieweeCanUpdateReview);
    }

    return await ExecuteWithSaveAsync(
      () =>
      {
        leetcodeRound.IsReviewed = request.IsReviewed;
        _leetcodeMockInterviewRoundRepository.Update(leetcodeRound, cancellationToken);
        
        _cache.Remove(RouteCacheKeys.ListMockInterviews, mockInterview.IntervieweeUserId);
        _cache.Remove(RouteCacheKeys.ListMockInterviews, mockInterview.InterviewerUserId);

        return Task.FromResult(new UpdateMockInterviewRoundReviewResponse
        {
          Message = Messages.MockInterview.RoundReviewUpdated
        });
      },
      Messages.MockInterview.LeetcodeRoundUpdateError,
      cancellationToken: cancellationToken
    );
  }

  private async Task<MockInterviewEntity?> GetMockInterviewForCustomRound(
    string customRoundId,
    CancellationToken cancellationToken
  )
  {
    var mockInterviews = await _mockInterviewRepository.GetAllAsync(
      mi => mi.MockInterviewRounds.Any(r => r.CustomMockInterviewRoundId == customRoundId),
      cancellationToken,
      include: q => q.Include(mi => mi.MockInterviewRounds)
    );

    return mockInterviews.FirstOrDefault();
  }

  private async Task<MockInterviewEntity?> GetMockInterviewForLeetcodeRound(
    string leetcodeRoundId,
    CancellationToken cancellationToken
  )
  {
    var mockInterviews = await _mockInterviewRepository.GetAllAsync(
      mi => mi.MockInterviewRounds.Any(r => r.LeetcodeMockInterviewRoundId == leetcodeRoundId),
      cancellationToken,
      include: q => q.Include(mi => mi.MockInterviewRounds)
    );

    return mockInterviews.FirstOrDefault();
  }

  #endregion
}
