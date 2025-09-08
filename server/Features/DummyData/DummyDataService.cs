using System.Linq.Expressions;
using Bogus;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.DummyData.Dtos;
using RSPWebAPI.Features.DummyData.Interfaces;

namespace RSPWebAPI.Features.DummyData;

public class DummyDataService : IDummyDataService
{
  private readonly ILogger<DummyDataService> _logger;
  private readonly IUnitOfWork _unitOfWork;

  public DummyDataService(IUnitOfWork unitOfWork, ILogger<DummyDataService> logger)
  {
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  private static List<SeasonEntity> GenerateSeasons(Faker faker, int total)
  {
    var seasons = new List<SeasonEntity>();
    for (var i = 0; i < total; i++)
    {
      var startDate = faker.Date.PastOffset(5).DateTime.ToUniversalTime();
      var endDate = startDate.AddMonths(3).ToUniversalTime();
      var country = faker.Address.Country();
      var placeholderCountryCode = new string(faker.Random.Chars('A', 'Z', 3));

      seasons.Add(
        new SeasonEntity
        {
          SeasonId = Database.Constants.GeneratePrimaryKeyId(),
          Name = $"{country} {startDate.Year}/{endDate.Year}",
          Slug = $"{placeholderCountryCode}-{startDate.Year}-{endDate.Year}",
          StartDateInclusiveUtc = startDate,
          EndDateInclusiveUtc = endDate,
          Location = country,
          ImageUrl = faker.Image.PicsumUrl(width: 1280, height: 720),
          DeletedAtUtc = null,
        }
      );
    }

    return seasons;
  }

  private static List<SeasonWeekEntity> GenerateSeasonWeeks(Faker faker, List<SeasonEntity> seasons)
  {
    var seasonWeeks = new List<SeasonWeekEntity>();
    foreach (var season in seasons)
    {
      var seasonStartDate = season.StartDateInclusiveUtc;
      for (var i = 1; i <= 12; i++)
      {
        var startDaysOffset = (i - 1) * 7;
        var endDaysOffset = i * 7;
        seasonWeeks.Add(
          new SeasonWeekEntity
          {
            SeasonWeekId = Database.Constants.GeneratePrimaryKeyId(),
            SeasonId = season.SeasonId,
            WeekNumber = i,
            StartDate = seasonStartDate.AddDays(startDaysOffset).AddSeconds(1),
            EndDate = seasonStartDate.AddDays(endDaysOffset),
            DeletedAtUtc = null,
          }
        );
      }
    }

    return seasonWeeks;
  }

  private static List<UserEntity> GenerateUsers(Faker faker, int total)
  {
    var users = new List<UserEntity>();
    for (var i = 0; i < total; i++)
    {
      users.Add(
        new UserEntity
        {
          UserId = Database.Constants.GeneratePrimaryKeyId(),
          DiscordId = faker.Random.AlphaNumeric(8),
          Email = faker.Internet.Email().ToLower(),
          IsAdmin = false,
          Name = faker.Name.FullName(),
          ProfileImage = faker.Image.PicsumUrl(width: 300, height: 300),
          DeletedAtUtc = null,
        }
      );
    }

    return users;
  }

  private List<EnrollmentEntity> GenerateEnrollments(
    Faker faker,
    List<UserEntity> users,
    List<SeasonEntity> seasons
  )
  {
    var enrollments = new List<EnrollmentEntity>();

    foreach (var season in seasons)
    {
      var shuffledUsers = users.OrderBy(_ => faker.Random.Int()).ToList();

      // Assign one Coordinator
      var coordinator = shuffledUsers.First();
      enrollments.Add(
        new EnrollmentEntity
        {
          EnrollmentId = Database.Constants.GeneratePrimaryKeyId(),
          UserId = coordinator.UserId,
          SeasonId = season.SeasonId,
          Role = SeasonRole.Coordinator,
          StudentRolePromotion = SeasonStudentRolePromotion.NotApplicable,
          DeletedAtUtc = null,
        }
      );
      shuffledUsers.RemoveAt(0);

      var totalUsers = shuffledUsers.Count;
      var mentorCount = Math.Max(1, totalUsers / 15);
      var studentCount = totalUsers - mentorCount;

      // Assign Mentors
      var mentors = shuffledUsers.Take(mentorCount).ToList();
      foreach (var mentor in mentors)
      {
        enrollments.Add(
          new EnrollmentEntity
          {
            EnrollmentId = Database.Constants.GeneratePrimaryKeyId(),
            UserId = mentor.UserId,
            SeasonId = season.SeasonId,
            Role = SeasonRole.Mentor,
            StudentRolePromotion = SeasonStudentRolePromotion.NotApplicable,
            DeletedAtUtc = null,
          }
        );
      }

      shuffledUsers = shuffledUsers.Skip(mentorCount).ToList();

      // Assign Students
      foreach (var student in shuffledUsers)
      {
        var studentRole = faker.PickRandom(
          new[]
          {
            SeasonStudentRolePromotion.Novice,
            SeasonStudentRolePromotion.Beginner,
            SeasonStudentRolePromotion.Advanced,
            SeasonStudentRolePromotion.Intermediate,
          }
        );

        enrollments.Add(
          new EnrollmentEntity
          {
            EnrollmentId = Database.Constants.GeneratePrimaryKeyId(),
            UserId = student.UserId,
            SeasonId = season.SeasonId,
            Role = SeasonRole.Student,
            StudentRolePromotion = studentRole,
            DeletedAtUtc = null,
          }
        );
      }
    }

    return enrollments;
  }

  private async Task<List<ProblemAttemptEntity>> GenerateProblemAttempts(
    Faker faker,
    List<UserEntity> users
  )
  {
    var problemAttempts = new List<ProblemAttemptEntity>();

    var leetcodeProblemsQueryable = await _unitOfWork
      .GetRepository<LeetcodeProblemEntity>()
      .GetAllAsync();
    var leetcodeProblems = leetcodeProblemsQueryable.ToList();
    var enrollmentsQueryable = await _unitOfWork
      .GetRepository<EnrollmentEntity>()
      .GetAllAsync(null, default, q => q.Include(e => e.Season));
    var enrollments = enrollmentsQueryable.ToList();
    var seasonWeeksQueryable = await _unitOfWork.GetRepository<SeasonWeekEntity>().GetAllAsync();
    var seasonWeeks = seasonWeeksQueryable.ToList();

    foreach (var user in users)
    {
      var userEnrollments = enrollments.Where(e => e.UserId == user.UserId).ToList();

      if (!userEnrollments.Any())
      {
        continue;
      }

      var attemptsCount = faker.Random.Int(70, 100);

      for (var j = 0; j < attemptsCount; j++)
      {
        var leetcodeProblemId = faker.PickRandom(leetcodeProblems).LeetcodeProblemId;
        var enrollment = faker.PickRandom(userEnrollments);
        var currentSeasonWeeks = seasonWeeks.Where(s => s.SeasonId == enrollment.SeasonId);
        var seasonWeek = faker.PickRandom(currentSeasonWeeks);

        if (enrollment.Role != SeasonRole.Student)
        {
          continue;
        }

        var range = (
          enrollment.Season.EndDateInclusiveUtc - enrollment.Season.StartDateInclusiveUtc
        ).TotalSeconds;
        var randomSeconds = faker.Random.Double(0, range);
        var attemptStartDateUtc = enrollment.Season.StartDateInclusiveUtc.AddSeconds(randomSeconds);

        problemAttempts.Add(
          new ProblemAttemptEntity
          {
            ProblemAttemptId = Database.Constants.GeneratePrimaryKeyId(),
            AttemptStartDateUtc = attemptStartDateUtc,
            TimeTakenInMinutes = faker.Random.Int(10, 180),
            Notes = faker.Lorem.Sentence(),
            UserId = user.UserId,
            LeetcodeProblemId = leetcodeProblemId,
            CustomProblemId = null,
            EnrollmentId = enrollment.EnrollmentId,
            DeletedAtUtc = null,
            SeasonWeekId = seasonWeek.SeasonWeekId,
          }
        );
      }
    }

    return problemAttempts;
  }

  private List<MentorshipEntity> GenerateMentorships(
    Faker faker,
    List<EnrollmentEntity> enrollments
  )
  {
    var mentorships = new List<MentorshipEntity>();

    // Group enrollments by season and role
    var groupedEnrollments = enrollments.GroupBy(e => e.SeasonId);

    foreach (var group in groupedEnrollments)
    {
      var mentors = group.Where(e => e.Role == SeasonRole.Mentor).ToList();
      var mentees = group.Where(e => e.Role == SeasonRole.Student).ToList();

      foreach (var mentor in mentors)
      {
        var menteeCount = Math.Min(faker.Random.Int(10, 15), mentees.Count);
        var assignedMentees = faker.PickRandom(mentees, menteeCount);

        foreach (var mentee in assignedMentees)
        {
          mentorships.Add(
            new MentorshipEntity
            {
              MentorshipId = Database.Constants.GeneratePrimaryKeyId(),
              MentorEnrollmentId = mentor.EnrollmentId,
              MenteeEnrollmentId = mentee.EnrollmentId,
              MentorEnrollment = mentor,
              MenteeEnrollment = mentee,
              DeletedAtUtc = null,
            }
          );
        }
      }
    }

    return mentorships;
  }

  private async Task<List<MockInterviewEntity>> GenerateMockInterviews(Faker faker)
  {
    var mockInterviews = new List<MockInterviewEntity>();

    var enrollmentsQueryable = await _unitOfWork
      .GetRepository<EnrollmentEntity>()
      .GetAllAsync(null, default, q => q.Include(e => e.Season));
    var enrollments = enrollmentsQueryable.ToList();
    var seasonWeeksQueryable = await _unitOfWork.GetRepository<SeasonWeekEntity>().GetAllAsync();
    var seasonWeeks = seasonWeeksQueryable.ToList();

    var interviewers = enrollments.Where(e => e.Role == SeasonRole.Student).ToList();
    var interviewees = enrollments.Where(e => e.Role == SeasonRole.Student).ToList();

    foreach (var interviewee in interviewees)
    {
      for (int i = 0; i < 10; i++)
      {
        var filteredInterviewers = interviewers.Where(i => i.UserId != interviewee.UserId);
        var interviewer = faker.PickRandom(filteredInterviewers);
        var currentSeasonWeeks = seasonWeeks.Where(s => s.SeasonId == interviewer.SeasonId);
        var seasonWeek = faker.PickRandom(currentSeasonWeeks);

        var mockInterview = new MockInterviewEntity
        {
          MockInterviewId = Database.Constants.GeneratePrimaryKeyId(),
          IsPass = faker.Random.Bool(),
          StartDate = faker.Date.Recent().ToUniversalTime(),
          TimeTakenInMinutes = faker.Random.Int(30, 120),
          InterviewerUserId = interviewer.UserId,
          IntervieweeUserId = interviewee.UserId,
          SeasonId = interviewee.EnrollmentId,
          Interviewer = interviewer.User,
          Interviewee = interviewee.User,
          Season = interviewee.Season,
          SeasonWeekId = seasonWeek.SeasonWeekId,
          DeletedAtUtc = null,
        };

        mockInterview.MockInterviewRounds = await GenerateMockInterviewRounds(
          faker,
          mockInterview.MockInterviewId
        );
        mockInterviews.Add(mockInterview);
      }
    }

    return mockInterviews;
  }

  private async Task<List<MockInterviewRoundEntity>> GenerateMockInterviewRounds(
    Faker faker,
    string mockInterviewId
  )
  {
    var rounds = new List<MockInterviewRoundEntity>();

    var behaviouralRound = new BehaviouralMockInterviewRoundEntity()
    {
      BehaviouralMockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
      BehavioralScore = faker.Random.Int(0, 10),
      DeletedAtUtc = null,
    };
    await _unitOfWork
      .GetRepository<BehaviouralMockInterviewRoundEntity>()
      .AddAsync(behaviouralRound);
    await _unitOfWork.SaveChangesAsync();
    rounds.Add(
      new MockInterviewRoundEntity
      {
        MockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
        MockInterviewId = mockInterviewId,
        IsReviewedByInterviewee = faker.Random.Bool(),
        IntervieweeComment = faker.Lorem.Sentence(),
        BehaviouralMockInterviewRoundId = behaviouralRound.BehaviouralMockInterviewRoundId,
        LeetcodeMockInterviewRoundId = null,
        CustomMockInterviewRoundId = null,
        DeletedAtUtc = null,
      }
    );

    // 2 is intentionally chosen here, because there is an assumption that we always have 2 rounds
    for (var i = 0; i < 2; i++)
    {
      var leetcodeRound = await GenerateLeetcodeMockInterviewRound(faker);
      await _unitOfWork.GetRepository<LeetcodeMockInterviewRoundEntity>().AddAsync(leetcodeRound);
      await _unitOfWork.SaveChangesAsync();

      rounds.Add(
        new MockInterviewRoundEntity
        {
          MockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
          MockInterviewId = mockInterviewId,
          IsReviewedByInterviewee = faker.Random.Bool(),
          IntervieweeComment = faker.Lorem.Sentence(),
          BehaviouralMockInterviewRoundId = null,
          LeetcodeMockInterviewRoundId = leetcodeRound?.LeetcodeMockInterviewRoundId,
          CustomMockInterviewRoundId = null,
          DeletedAtUtc = null,
        }
      );
    }

    return rounds;
  }

  private async Task<LeetcodeMockInterviewRoundEntity> GenerateLeetcodeMockInterviewRound(
    Faker faker
  )
  {
    var leetcodeProblemsQueryable = await _unitOfWork
      .GetRepository<LeetcodeProblemEntity>()
      .GetAllAsync();
    var leetcodeProblems = leetcodeProblemsQueryable.ToList().Select(lp => lp.LeetcodeProblemId);

    return new LeetcodeMockInterviewRoundEntity
    {
      LeetcodeMockInterviewRoundId = Database.Constants.GeneratePrimaryKeyId(),
      LeetcodeProblemId = faker.PickRandom(leetcodeProblems),
      ConfirmQuestionScore = faker.Random.Int(1, 10),
      AlgorithmDesignScore = faker.Random.Int(1, 10),
      ComplexityAnalysisScore = faker.Random.Int(1, 10),
      CodingScore = faker.Random.Int(1, 10),
      TestingScore = faker.Random.Int(1, 10),
      DeletedAtUtc = null,
    };
  }

  public async Task<AdminGenerateDummyDataResponse> AdminGenerateDummyData(
    AdminGenerateDummyDataRequest request,
    CancellationToken cancellationToken = default
  )
  {
    // Chose a shortcut way of accessing the entity directly which is not recommended.
    // The only reason why this is valid is because we want to move fast and
    await _unitOfWork.BeginTransactionAsync(cancellationToken);
    try
    {
      var faker = new Faker();

      var seasons = GenerateSeasons(faker, request.NumberOfSeasons);
      await _unitOfWork.GetRepository<SeasonEntity>().AddRangeAsync(seasons, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      var seasonWeeks = GenerateSeasonWeeks(faker, seasons);
      await _unitOfWork
        .GetRepository<SeasonWeekEntity>()
        .AddRangeAsync(seasonWeeks, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      var users = GenerateUsers(faker, request.NumberOfUsers);
      await _unitOfWork.GetRepository<UserEntity>().AddRangeAsync(users, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      var enrollments = GenerateEnrollments(faker, users, seasons);
      await _unitOfWork
        .GetRepository<EnrollmentEntity>()
        .AddRangeAsync(enrollments, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);

      var mentorships = GenerateMentorships(faker, enrollments);
      await _unitOfWork
        .GetRepository<MentorshipEntity>()
        .AddRangeAsync(mentorships, cancellationToken);

      var problemAttempts = await GenerateProblemAttempts(faker, users);
      await _unitOfWork
        .GetRepository<ProblemAttemptEntity>()
        .AddRangeAsync(problemAttempts, cancellationToken);

      var mockInterviews = await GenerateMockInterviews(faker);
      await _unitOfWork
        .GetRepository<MockInterviewEntity>()
        .AddRangeAsync(mockInterviews, cancellationToken);

      await _unitOfWork.SaveChangesAsync(cancellationToken);
      await _unitOfWork.CommitTransactionAsync(cancellationToken);

      return new AdminGenerateDummyDataResponse();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.DummyData.GenerationError);
      await _unitOfWork.RollbackTransactionAsync(cancellationToken);
      throw new InvalidOperationException(Messages.DummyData.GenerationError);
    }
  }
}
