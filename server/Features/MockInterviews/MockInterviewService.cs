using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.MockInterviews.Dtos;
using RSPWebAPI.Features.MockInterviews.Interfaces;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;

namespace RSPWebAPI.Features.MockInterviews;

public class MockInterviewService : IMockInterviewService
{
  private readonly IEnrollmentService _enrollmentService;
  private readonly ILogger<MockInterviewService> _logger;
  private readonly IRepository<MockInterviewEntity> _mockInterviewRepository;
  private readonly ISeasonWeekService _seasonWeekService;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IUserService _userService;

  public MockInterviewService(
    IRepository<MockInterviewEntity> mockInterviewRepository,
    ISeasonWeekService seasonWeekService,
    IUserService userService,
    IEnrollmentService enrollmentService,
    IUnitOfWork unitOfWork,
    ILogger<MockInterviewService> logger
  )
  {
    _mockInterviewRepository = mockInterviewRepository;
    _seasonWeekService = seasonWeekService;
    _userService = userService;
    _enrollmentService = enrollmentService;
    _unitOfWork = unitOfWork;
    _logger = logger;
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

  public async Task<IServiceResponse<CreateMockInterviewResponse>> CreateMockInterview(
    CreateMockInterviewRequest request,
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
      if (existingEnrollment == null || existingEnrollment.User.Email != request.IntervieweeEmail)
      {
        return new ErrorServiceResponse<CreateMockInterviewResponse>(
          Message.EnrollmentDoesNotExists
        );
      }

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
        return new ErrorServiceResponse<CreateMockInterviewResponse>(
          Message.MockInterviewOutOfSeasonDateRange
        );
      }
    }

    var interviewer = await _userService.GetUserByIdAsync(
      request.InterviewerUserId,
      cancellationToken
    );
    var interviewee = await _userService.GetUserByEmailAsync(
      request.IntervieweeEmail,
      cancellationToken
    );
    if (interviewee == null || interviewer == null)
    {
      return new ErrorServiceResponse<CreateMockInterviewResponse>(
        Message.MockInterviewInterviewerOrIntervieweeCannotBeFound
      );
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
      EnrollmentId = request.EnrollmentId,
      MockInterviewRounds = mockInterviewRounds,
      StartDate = request.StartDate,
      TimeTakenInMinutes = request.TimeTakenInMinutes,
      SeasonWeekId = seasonWeek?.SeasonWeekId,
    };

    try
    {
      await AddMockInterviewAsync(mockInterview, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<CreateMockInterviewResponse>(
        Message.MockInterviewCreatedSuccessfully,
        new CreateMockInterviewResponse { MockInterviewId = mockInterview.MockInterviewId }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.MockInterviewCreationUnexpectedError);
      return new ErrorServiceResponse<CreateMockInterviewResponse>(
        Message.MockInterviewCreationUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<DeleteMockInterviewResponse>> DeleteMockInterview(
    DeleteMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingMockInterview = await GetMockInterviewByIdAsync(
      request.MockInterviewId,
      cancellationToken,
      q => q.Include(m => m.Interviewer)
    );
    if (existingMockInterview == null || existingMockInterview.Interviewer.Email != request.Email)
    {
      return new ErrorServiceResponse<DeleteMockInterviewResponse>(
        Message.MockInterviewDoesNotExists
      );
    }

    try
    {
      await DeleteMockInterviewAsync(existingMockInterview.MockInterviewId, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<DeleteMockInterviewResponse>(
        Message.MockInterviewDeletedSuccessfully
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.MockInterviewDeletionUnexpectedError);
      return new ErrorServiceResponse<DeleteMockInterviewResponse>(
        Message.MockInterviewDeletionUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<ListMockInterviewResponse>> ListMockInterview(
    ListMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var query = _mockInterviewRepository.Table;
    if (request.EnrollmentId != null)
    {
      var existingEnrollment = await _enrollmentService.GetEnrollmentByIdAsync(
        request.EnrollmentId,
        cancellationToken,
        q => q.Include(e => e.User)
      );
      if (existingEnrollment == null || existingEnrollment.User.Email != request.Email)
      {
        return new ErrorServiceResponse<ListMockInterviewResponse>(Message.EnrollmentDoesNotExists);
      }

      query = query.Where(m => m.EnrollmentId == request.EnrollmentId);
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
      .Include(e => e.Enrollment);

    var mockInterviews = await _mockInterviewRepository.GetAllAsync(
      m => m.Interviewee.Email == request.Email,
      cancellationToken,
      _ => query
    );

    try
    {
      return new SuccessServiceResponse<ListMockInterviewResponse>(
        Message.MockInterviewListSuccessfully,
        new ListMockInterviewResponse { MockInterviews = mockInterviews.ToList() }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.MockInterviewListUnexpectedError);
      return new ErrorServiceResponse<ListMockInterviewResponse>(
        Message.MockInterviewListUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<UpdateMockInterviewResponse>> UpdateMockInterview(
    UpdateMockInterviewRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingMockInterview = await _mockInterviewRepository.GetByIdAsync(
      request.MockInterviewId,
      cancellationToken,
      q =>
        q.Include(m => m.Enrollment)
          .Include(m => m.MockInterviewRounds)
          .ThenInclude(r => r.BehaviouralMockInterviewRound)
          .Include(m => m.MockInterviewRounds)
          .ThenInclude(r => r.CustomMockInterviewRound)
          .Include(m => m.MockInterviewRounds)
          .ThenInclude(r => r.LeetcodeMockInterviewRound)
    );
    if (existingMockInterview == null || request.EnrollmentId != existingMockInterview.EnrollmentId)
    {
      return new ErrorServiceResponse<UpdateMockInterviewResponse>(
        Message.MockInterviewDoesNotExists
      );
    }

    SeasonWeekEntity? seasonWeek = null;
    if (request.EnrollmentId != null)
    {
      var currentDate = request.StartDate;
      var seasonWeeks = await _seasonWeekService.GetAllSeasonWeeksAsync(
        q =>
          q.StartDate <= currentDate
          && q.EndDate >= currentDate
          && q.SeasonId == existingMockInterview.Enrollment.SeasonId,
        cancellationToken
      );
      seasonWeek = seasonWeeks.FirstOrDefault();
      if (seasonWeek == null)
      {
        return new ErrorServiceResponse<UpdateMockInterviewResponse>(
          Message.MockInterviewOutOfSeasonDateRange
        );
      }
    }

    var interviewer = await _userService.GetUserByIdAsync(
      request.InterviewerUserId,
      cancellationToken
    );
    var interviewee = await _userService.GetUserByEmailAsync(
      request.IntervieweeEmail,
      cancellationToken
    );
    if (interviewee == null || interviewer == null)
    {
      return new ErrorServiceResponse<UpdateMockInterviewResponse>(
        Message.MockInterviewInterviewerOrIntervieweeCannotBeFound
      );
    }

    UpdateMockInterviewRounds(
      request.MockInterviewRounds,
      existingMockInterview.MockInterviewRounds
    );

    existingMockInterview.IsPass = IsAllScoresAboveThreshold(request.MockInterviewRounds);
    existingMockInterview.InterviewerUserId = interviewer.UserId;
    existingMockInterview.IntervieweeUserId = interviewee.UserId;
    existingMockInterview.EnrollmentId = request.EnrollmentId;
    existingMockInterview.StartDate = request.StartDate;
    existingMockInterview.TimeTakenInMinutes = request.TimeTakenInMinutes;
    existingMockInterview.SeasonWeekId = seasonWeek?.SeasonWeekId;

    try
    {
      await UpdateMockInterviewAsync(existingMockInterview, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<UpdateMockInterviewResponse>(
        Message.MockInterviewUpdatedSuccessfully
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.MockInterviewUpdateUnexpectedError);
      return new ErrorServiceResponse<UpdateMockInterviewResponse>(
        Message.MockInterviewUpdateUnexpectedError
      );
    }
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
      cancellationToken
    );

    if (mockInterview == null)
    {
      throw new KeyNotFoundException(Message.MockInterviewDoesNotExists);
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
      cancellationToken
    );
    if (existingMockInterview == null)
    {
      throw new KeyNotFoundException(Message.MockInterviewDoesNotExists);
    }

    _mockInterviewRepository.Update(mockInterview, cancellationToken);
  }

  #endregion
}
