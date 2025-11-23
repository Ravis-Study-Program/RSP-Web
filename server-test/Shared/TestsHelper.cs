using Bogus;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Dtos;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.Leetcodes.Interfaces;
using RSPWebAPI.Features.Mentorships.Dtos;
using RSPWebAPI.Features.Mentorships.Interfaces;
using RSPWebAPI.Features.MockInterviews.Dtos;
using RSPWebAPI.Features.MockInterviews.Interfaces;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.Seasons;
using RSPWebAPI.Features.Seasons.Dtos;
using RSPWebAPI.Features.Seasons.Interfaces;
using RSPWebAPI.Features.SeasonWeeks;
using RSPWebAPI.Features.SeasonWeeks.Dtos;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;
using RSPWebAPI.Features.Users;
using RSPWebAPI.Features.Users.Dtos;
using RSPWebAPI.Features.Users.Interfaces;
using Xunit;

namespace RSPWebAPI.Tests.Shared;

public class TestDataSeeder
{
  private readonly IUserService _userService;
  private readonly ISeasonService _seasonService;
  private readonly ISeasonWeekService _seasonWeekService;
  private readonly IEnrollmentService _enrollmentService;
  private readonly IProblemAttemptService _problemAttemptService;
  private readonly IMockInterviewService _mockInterviewService;
  private readonly IMentorshipService _mentorshipService;
  private readonly IRepository<LeetcodeProblemEntity> _leetcodeProblemRepository;
  private readonly IRepository<LeetcodeProblemRecommendationEntity> _leetcodeProblemRecommendationRepository;
  private readonly Faker _faker = new();
  private readonly ApplicationDbContext _dbContext;

  public TestDataSeeder(
    ApplicationDbContext dbContext,
    IUserService userService,
    ISeasonService seasonService,
    ISeasonWeekService seasonWeekService,
    IEnrollmentService enrollmentService,
    IProblemAttemptService problemAttemptService,
    IMockInterviewService mockInterviewService,
    IMentorshipService mentorshipService,
    IRepository<LeetcodeProblemEntity> leetcodeProblemRepository,
    IRepository<LeetcodeProblemRecommendationEntity> leetcodeProblemRecommendationRepository
  )
  {
    _dbContext = dbContext;
    _userService = userService;
    _seasonService = seasonService;
    _seasonWeekService = seasonWeekService;
    _enrollmentService = enrollmentService;
    _problemAttemptService = problemAttemptService;
    _mockInterviewService = mockInterviewService;
    _mentorshipService = mentorshipService;
    _leetcodeProblemRepository = leetcodeProblemRepository;
    _leetcodeProblemRecommendationRepository = leetcodeProblemRecommendationRepository;
  }

  public DateTime startDate = DateTime.UtcNow.AddDays(-7);

  public async Task<string> SeedUserAsync(
    string? emailOverride = null,
    bool? isAdminOverride = null
  )
  {
    var request = new AdminCreateUserRequest
    {
      Email = emailOverride ?? _faker.Internet.Email().ToLower(),
      Name = _faker.Name.FullName(),
      ProfileImage = _faker.Image.PicsumUrl(),
      DiscordId = _faker.Random.AlphaNumeric(8),
      IsAdmin = isAdminOverride ?? false,
    };

    var response = await _userService.CreateAdminUser(request);
    Assert.NotNull(response.UserId);

    return response.UserId;
  }

  public async Task<string> SeedSeasonAsync(
    string? slugOverride = null,
    DateTime? startDateOverride = null,
    DateTime? endDateOverride = null
  )
  {
    var startDate = startDateOverride ?? DateTime.UtcNow.AddDays(-1);
    var endDate = endDateOverride ?? startDate.AddDays(7 * 6 + 2);

    var request = new AdminCreateSeasonRequest
    {
      Name = _faker.Name.FirstName(),
      Slug = slugOverride ?? _faker.Name.FirstName(),
      StartDateInclusiveUtc = startDate,
      EndDateInclusiveUtc = endDate,
      Location = _faker.Address.City(),
      ImageUrl = _faker.Image.PicsumUrl(),
    };

    var response = await _seasonService.CreateAdminSeason(request);
    Assert.NotNull(response.SeasonId);

    return response.SeasonId;
  }

  public async Task<string> SeedSeasonWeekAsync(string seasonId, int weekNumber)
  {
    var request = new AdminCreateSeasonWeekRequest
    {
      SeasonId = seasonId,
      WeekNumber = weekNumber,
      StartDate = startDate.AddDays(weekNumber * 7),
      EndDate = startDate.AddDays((weekNumber + 1) * 7),
    };

    var response = await _seasonWeekService.CreateAdminSeasonWeek(request);
    Assert.NotNull(response.SeasonWeekId);

    return response.SeasonWeekId;
  }

  public async Task<string> SeedEnrollmentAsync(
    string? seasonId = null,
    string? userId = null,
    SeasonRole role = SeasonRole.Student,
    SeasonStudentRolePromotion rolePromotion = SeasonStudentRolePromotion.Beginner
  )
  {
    if (string.IsNullOrWhiteSpace(seasonId))
    {
      seasonId = await SeedSeasonAsync();
    }

    if (string.IsNullOrWhiteSpace(userId))
    {
      userId = await SeedUserAsync();
    }

    var request = new AdminCreateEnrollmentRequest
    {
      SeasonId = seasonId,
      UserId = userId,
      Role = role,
      StudentRolePromotion = rolePromotion,
    };

    var response = await _enrollmentService.CreateAdminEnrollment(request);
    Assert.NotNull(response.EnrollmentId);

    return response.EnrollmentId;
  }

  public async Task<List<AdminUserDto>> GetAllUsersAsync()
  {
    var response = await _userService.ListAdminUser(new AdminListUserRequest());
    Assert.NotNull(response.Users);

    return response.Users.ToList();
  }

  public async Task<List<SeasonEntity>> GetAllSeasonsAsync()
  {
    var seasons = await _seasonService.GetAllSeasonsAsync();
    return seasons.ToList();
  }

  public async Task<List<SeasonWeekEntity>> GetAllSeasonWeeksAsync()
  {
    var seasonWeeks = await _seasonWeekService.GetAllSeasonWeeksAsync();
    return seasonWeeks.ToList();
  }

  public async Task<List<EnrollmentEntity>> GetAllEnrollmentsAsync()
  {
    var all = await _enrollmentService.GetAllEnrollmentsAsync();
    return all.ToList();
  }

  public LeetcodeProblemEntity CreateLeetcodeProblemEntity()
  {
    var problem = new LeetcodeProblemEntity
    {
      LeetcodeProblemId = Constants.GeneratePrimaryKeyId(),
      IsPremium = _faker.Random.Bool(),
      LeetcodeProblemDifficulty = _faker.PickRandom<LeetcodeProblemDifficulty>(),
      Problem = new ProblemEntity
      {
        ProblemId = Constants.GeneratePrimaryKeyId(),
        Title = $"{_faker.Random.Number(1000, 9999)}. {_faker.Lorem.Word()}",
        Link = "https://leetcode.com/problems/" + _faker.Lorem.Word(),
      },
      LeetcodeProblemCategories = new List<LeetcodeProblemCategoryEntity>(),
    };
    return problem;
  }

  public async Task<string> SeedLeetcodeProblemAsync()
  {
    var problem = new LeetcodeProblemEntity
    {
      LeetcodeProblemId = Constants.GeneratePrimaryKeyId(),
      IsPremium = _faker.Random.Bool(),
      LeetcodeProblemDifficulty = _faker.PickRandom<LeetcodeProblemDifficulty>(),
      Problem = new ProblemEntity
      {
        ProblemId = Constants.GeneratePrimaryKeyId(),
        Title = $"{_faker.Random.Number(1000, 9999)}. {_faker.Lorem.Word()}",
        Link = "https://leetcode.com/problems/" + _faker.Lorem.Word(),
      },
      LeetcodeProblemCategories = new List<LeetcodeProblemCategoryEntity>(),
    };

    await _leetcodeProblemRepository.AddAsync(problem);
    await _dbContext.SaveChangesAsync();
    return problem.LeetcodeProblemId;
  }

  public async Task<string> SeedLeetcodeProblemRecommendationAsync(
    string? userId = null,
    string? leetcodeProblemId = null
  )
  {
    if (string.IsNullOrWhiteSpace(userId))
    {
      userId = await SeedUserAsync();
    }

    if (string.IsNullOrWhiteSpace(leetcodeProblemId))
    {
      leetcodeProblemId = await SeedLeetcodeProblemAsync();
    }

    var entity = new LeetcodeProblemRecommendationEntity
    {
      LeetcodeProblemRecommendationId = Constants.GeneratePrimaryKeyId(),
      UserId = userId,
      LeetcodeProblemId = leetcodeProblemId,
    };

    await _leetcodeProblemRecommendationRepository.AddAsync(entity);
    await _dbContext.SaveChangesAsync();

    return entity.LeetcodeProblemRecommendationId;
  }

  public async Task<string> SeedProblemAttemptAsync(
    string email,
    string? enrollmentId = null,
    string? leetcodeProblemId = null,
    string? customProblemId = null
  )
  {
    var user = await _userService.GetUserAsync(email: email);
    Assert.NotNull(user);

    if (string.IsNullOrWhiteSpace(enrollmentId))
    {
      // Optionally create an enrollment for them
      enrollmentId = await SeedEnrollmentAsync();
    }

    var attempt = new ProblemAttemptEntity
    {
      ProblemAttemptId = Constants.GeneratePrimaryKeyId(),
      AttemptStartDateUtc = DateTime.UtcNow,
      TimeTakenInMinutes = _faker.Random.Int(1, 60),
      LeetcodeProblemId = leetcodeProblemId,
      CustomProblemId = customProblemId,
      Notes = _faker.Lorem.Sentence(),
      EnrollmentId = enrollmentId,
      UserId = user.UserId,
      SeasonWeekId =
        null // or set if needed
      ,
    };

    await _dbContext.Set<ProblemAttemptEntity>().AddAsync(attempt);
    await _dbContext.SaveChangesAsync();

    return attempt.ProblemAttemptId;
  }

  public async Task<(string mockInterviewId, string customRoundId)> SeedMockInterviewWithCustomRoundAsync(
    string interviewerEmail,
    string? intervieweeUserId = null
  )
  {
    // Ensure interviewer exists
    var interviewer = await _userService.GetUserAsync(email: interviewerEmail);
    if (interviewer == null)
    {
      await SeedUserAsync(interviewerEmail);
      interviewer = await _userService.GetUserAsync(email: interviewerEmail);
    }
    var seasonId = await SeedSeasonAsync(null, startDate, startDate.AddDays(4 * 7 + 1));
    Assert.NotNull(interviewer);

    if (string.IsNullOrWhiteSpace(intervieweeUserId))
    {
      intervieweeUserId = await SeedUserAsync();
    }

    await SeedEnrollmentAsync(seasonId, intervieweeUserId);
    for (var i = 0; i <= 2; i++)
    {
      await SeedSeasonWeekAsync(seasonId, i);
    }

    var customRoundId = Constants.GeneratePrimaryKeyId();
    var rounds = new List<MockInterviewRoundDto>
    {
      new MockInterviewRoundDto
      {
        MockInterviewRoundId = customRoundId,
        CustomMockInterviewRound = new CustomMockInterviewRoundDto
        {
          Content = _faker.Lorem.Paragraph(),
          Link = _faker.Internet.Url(),
          Score = _faker.Random.Int(1, 10)
        }
      }
    };

    var request = new CreateMockInterviewRequest
    {
      InterviewerUserId = interviewer.UserId,
      IntervieweeUserId = intervieweeUserId,
      SeasonId = seasonId,
      TimeTakenInMinutes = _faker.Random.Int(10, 60),
      MockInterviewRounds = rounds,
    };

    var response = await _mockInterviewService.CreateMockInterview(request);
    Assert.NotNull(response.MockInterviewId);

    // Get the created mock interview to find the actual custom round ID
    var createdMockInterview = await _mockInterviewService.GetMockInterviewByIdAsync(
      response.MockInterviewId,
      include: q => q.Include(mi => mi.MockInterviewRounds)
        .ThenInclude(r => r.CustomMockInterviewRound)
    );
    Assert.NotNull(createdMockInterview);
    var actualCustomRoundId = createdMockInterview.MockInterviewRounds
      .FirstOrDefault(r => r.CustomMockInterviewRound != null)?
      .CustomMockInterviewRound?.CustomMockInterviewRoundId;
    Assert.NotNull(actualCustomRoundId);

    return (response.MockInterviewId, actualCustomRoundId);
  }

  public async Task<(string mockInterviewId, string leetcodeRoundId)> SeedMockInterviewWithLeetcodeRoundAsync(
    string interviewerEmail,
    string? intervieweeUserId = null
  )
  {
    // Ensure interviewer exists
    var interviewer = await _userService.GetUserAsync(email: interviewerEmail);
    if (interviewer == null)
    {
      await SeedUserAsync(interviewerEmail);
      interviewer = await _userService.GetUserAsync(email: interviewerEmail);
    }
    var seasonId = await SeedSeasonAsync(null, startDate, startDate.AddDays(4 * 7 + 1));
    var leetcodeProblemId = await SeedLeetcodeProblemAsync();
    Assert.NotNull(interviewer);

    if (string.IsNullOrWhiteSpace(intervieweeUserId))
    {
      intervieweeUserId = await SeedUserAsync();
    }

    await SeedEnrollmentAsync(seasonId, intervieweeUserId);
    for (var i = 0; i <= 2; i++)
    {
      await SeedSeasonWeekAsync(seasonId, i);
    }

    var leetcodeRoundId = Constants.GeneratePrimaryKeyId();
    var rounds = new List<MockInterviewRoundDto>
    {
      new MockInterviewRoundDto
      {
        MockInterviewRoundId = leetcodeRoundId,
        LeetcodeMockInterviewRound = new LeetcodeMockInterviewRoundDto
        {
          ConfirmQuestionScore = _faker.Random.Int(1, 10),
          AlgorithmDesignScore = _faker.Random.Int(1, 10),
          ComplexityAnalysisScore = _faker.Random.Int(1, 10),
          CodingScore = _faker.Random.Int(1, 10),
          TestingScore = _faker.Random.Int(1, 10),
          LeetcodeProblemId = leetcodeProblemId
        }
      }
    };

    var request = new CreateMockInterviewRequest
    {
      InterviewerUserId = interviewer.UserId,
      IntervieweeUserId = intervieweeUserId,
      SeasonId = seasonId,
      TimeTakenInMinutes = _faker.Random.Int(10, 60),
      MockInterviewRounds = rounds,
    };

    var response = await _mockInterviewService.CreateMockInterview(request);
    Assert.NotNull(response.MockInterviewId);

    // Get the created mock interview to find the actual leetcode round ID
    var createdMockInterview = await _mockInterviewService.GetMockInterviewByIdAsync(
      response.MockInterviewId,
      include: q => q.Include(mi => mi.MockInterviewRounds)
        .ThenInclude(r => r.LeetcodeMockInterviewRound)
    );
    Assert.NotNull(createdMockInterview);
    var actualLeetcodeRoundId = createdMockInterview.MockInterviewRounds
      .FirstOrDefault(r => r.LeetcodeMockInterviewRound != null)?
      .LeetcodeMockInterviewRound?.LeetcodeMockInterviewRoundId;
    Assert.NotNull(actualLeetcodeRoundId);

    return (response.MockInterviewId, actualLeetcodeRoundId);
  }

  public async Task<string> SeedMockInterviewAsync(
    string interviewerEmail,
    string? enrollmentId = null,
    string? intervieweeUserId = null,
    int? timeTakenInMinutes = null,
    List<MockInterviewRoundDto>? rounds = null
  )
  {
    var interviewer = await _userService.GetUserAsync(email: interviewerEmail);
    var seasonId = await SeedSeasonAsync(null, startDate, startDate.AddDays(4 * 7 + 1));
    Assert.NotNull(interviewer);

    if (string.IsNullOrWhiteSpace(intervieweeUserId))
    {
      // Create a new user for the interviewer
      intervieweeUserId = await SeedUserAsync();
    }

    if (string.IsNullOrWhiteSpace(enrollmentId))
    {
      // If none provided, create an enrollment that matches the user
      await SeedEnrollmentAsync(seasonId, intervieweeUserId);
      for (var i = 0; i <= 2; i++)
      {
        await SeedSeasonWeekAsync(seasonId, i);
      }
    }

    var request = new CreateMockInterviewRequest
    {
      InterviewerUserId = interviewer.UserId,
      IntervieweeUserId = intervieweeUserId,
      SeasonId = seasonId,
      TimeTakenInMinutes = timeTakenInMinutes ?? _faker.Random.Int(10, 60),
      MockInterviewRounds = rounds ?? new List<MockInterviewRoundDto>(),
    };

    var response = await _mockInterviewService.CreateMockInterview(request);
    Assert.NotNull(response.MockInterviewId);

    return response.MockInterviewId;
  }

  public async Task<string> SeedSeasonAndSeasonWeeks()
  {
    var seasonId = await SeedSeasonAsync(null, startDate.AddDays(-7), startDate.AddDays(4 * 7 + 1));
    for (var i = 0; i <= 2; i++)
    {
      await SeedSeasonWeekAsync(seasonId, i);
    }

    return seasonId;
  }

  public async Task<string> SeedMentorshipAsync(
    string? mentorEnrollmentId = null,
    string? menteeEnrollmentId = null,
    string? seasonId = null
  )
  {
    if (seasonId == null)
    {
      seasonId = await SeedSeasonAsync(null, startDate, startDate.AddDays(4 * 7 + 1));
      for (var i = 0; i <= 2; i++)
      {
        await SeedSeasonWeekAsync(seasonId, i);
      }
    }

    // If none provided, create a user with role=Mentor, get that enrollment
    if (mentorEnrollmentId == null)
    {
      var mentorUserId = await SeedUserAsync();
      mentorEnrollmentId = await SeedEnrollmentAsync(
        seasonId,
        mentorUserId,
        SeasonRole.Mentor,
        SeasonStudentRolePromotion.NotApplicable
      );
    }

    // If none provided, create a user with role=Student, get that enrollment
    if (menteeEnrollmentId == null)
    {
      var menteeUserId = await SeedUserAsync();
      menteeEnrollmentId = await SeedEnrollmentAsync(
        seasonId,
        menteeUserId,
        SeasonRole.Student,
        SeasonStudentRolePromotion.Advanced
      );
    }

    var request = new AdminCreateMentorshipRequest
    {
      MentorEnrollmentId = mentorEnrollmentId,
      MenteeEnrollmentId = menteeEnrollmentId,
    };

    var response = await _mentorshipService.CreateAdminMentorship(request);
    Assert.NotNull(response.MentorshipId);

    return response.MentorshipId;
  }

  public async Task<
    List<LeetcodeProblemRecommendationEntity>
  > GetAllLeetcodeProblemRecommendationsAsync()
  {
    return await _leetcodeProblemRecommendationRepository.TableNoTracking.ToListAsync();
  }

  public async Task<List<ProblemAttemptEntity>> GetAllProblemAttemptsAsync()
  {
    var attempts = await _problemAttemptService.GetAllProblemAttemptsAsync();
    return attempts.ToList();
  }

  public async Task<List<MockInterviewEntity>> GetAllMockInterviewsAsync()
  {
    var mockInterviews = await _mockInterviewService.GetAllMockInterviewsAsync();
    return mockInterviews.ToList();
  }

  public async Task<List<MentorshipEntity>> GetAllMentorshipsAsync()
  {
    var mentorships = await _mentorshipService.GetAllMentorshipsAsync();
    return mentorships.ToList();
  }
}
