using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Xml;
using Bogus;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Seasons;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;
using AdminGenerateDummyDataResult = Microsoft.AspNetCore.Http.HttpResults.Results<
  Microsoft.AspNetCore.Http.HttpResults.Ok<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.DummyData.AdminGenerateDummyDataResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.NotFound<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.DummyData.AdminGenerateDummyDataResponse>>,
  Microsoft.AspNetCore.Http.HttpResults.BadRequest<RSPWebAPI.Shared.ApiResult<RSPWebAPI.Features.DummyData.AdminGenerateDummyDataResponse>>
>;

namespace RSPWebAPI.Features.DummyData;

public static class AdminGenerateDummyData
{
  public class Command : AdminAuthRequest<ApiResult<AdminGenerateDummyDataResponse>>
  {
    public int NumberOfSeasons { get; set; }
    public int NumberOfUsers { get; set; }
  }

  public class Validator : AbstractValidator<Command>
  {
    public Validator()
    {
      RuleFor(c => c.NumberOfSeasons).GreaterThan(0);
      RuleFor(c => c.NumberOfUsers).GreaterThan(0);
    }
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminGenerateDummyDataResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
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

    private static List<SeasonWeekEntity> GenerateSeasonWeeks(
      Faker faker,
      List<SeasonEntity> seasons
    )
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

    private List<ProblemAttemptEntity> GenerateProblemAttempts(
      Faker faker,
      List<UserEntity> users,
      List<EnrollmentEntity> enrollments
    )
    {
      var problemAttempts = new List<ProblemAttemptEntity>();

      var leetcodeProblems = _dbContext.LeetcodeProblems.ToList();
      enrollments = _dbContext.Enrollments.Include(e => e.Season).ToList();

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
          if (enrollment.Role != SeasonRole.Student)
          {
            continue;
          }

          var range = (
            enrollment.Season.EndDateInclusiveUtc - enrollment.Season.StartDateInclusiveUtc
          ).TotalSeconds;
          var randomSeconds = faker.Random.Double(0, range);
          var attemptStartDateUtc = enrollment.Season.StartDateInclusiveUtc.AddSeconds(
            randomSeconds
          );

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

    private List<MockInterviewEntity> GenerateMockInterviews(
      Faker faker,
      List<EnrollmentEntity> enrollments
    )
    {
      var mockInterviews = new List<MockInterviewEntity>();
      var interviewers = enrollments.Where(e => e.Role == SeasonRole.Mentor).ToList();
      var interviewees = enrollments.Where(e => e.Role == SeasonRole.Student).ToList();

      foreach (var interviewee in interviewees)
      {
        var interviewer = faker.PickRandom(interviewers);
        var mockInterview = new MockInterviewEntity
        {
          MockInterviewId = Database.Constants.GeneratePrimaryKeyId(),
          IsPass = faker.Random.Bool(),
          StartDate = faker.Date.Recent().ToUniversalTime(),
          TimeTakenInMinutes = faker.Random.Int(30, 120),
          InterviewerUserId = interviewer.UserId,
          IntervieweeUserId = interviewee.UserId,
          EnrollmentId = interviewee.EnrollmentId,
          Interviewer = interviewer.User,
          Interviewee = interviewee.User,
          Enrollment = interviewee,
          DeletedAtUtc = null,
        };

        mockInterview.MockInterviewRounds = GenerateMockInterviewRounds(
          faker,
          mockInterview.MockInterviewId
        );
        mockInterviews.Add(mockInterview);
      }

      return mockInterviews;
    }

    private List<MockInterviewRoundEntity> GenerateMockInterviewRounds(
      Faker faker,
      string mockInterviewId
    )
    {
      var rounds = new List<MockInterviewRoundEntity>();
      var roundCount = faker.Random.Int(1, 3);

      for (var i = 0; i < roundCount; i++)
      {
        var leetcodeRound = GenerateLeetcodeMockInterviewRound(faker);
        _dbContext.LeetcodeMockInterviewRounds.Add(leetcodeRound);
        _dbContext.SaveChanges();

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

    private LeetcodeMockInterviewRoundEntity GenerateLeetcodeMockInterviewRound(Faker faker)
    {
      var leetcodeProblems = _dbContext
        .LeetcodeProblems.Select(lp => lp.LeetcodeProblemId)
        .ToList();
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

    public async Task<ApiResult<AdminGenerateDummyDataResponse>> Handle(
      AdminGenerateDummyData.Command request,
      CancellationToken cancellationToken
    )
    {
      using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
      try
      {
        var faker = new Faker();

        var seasons = GenerateSeasons(faker, request.NumberOfSeasons);
        await _dbContext.Seasons.AddRangeAsync(seasons, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var seasonWeeks = GenerateSeasonWeeks(faker, seasons);
        await _dbContext.SeasonWeeks.AddRangeAsync(seasonWeeks, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var users = GenerateUsers(faker, request.NumberOfUsers);
        await _dbContext.Users.AddRangeAsync(users, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var enrollments = GenerateEnrollments(faker, users, seasons);
        await _dbContext.Enrollments.AddRangeAsync(enrollments, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var mentorships = GenerateMentorships(faker, enrollments);
        await _dbContext.Mentorships.AddRangeAsync(mentorships, cancellationToken);

        var problemAttempts = GenerateProblemAttempts(faker, users, enrollments);
        await _dbContext.ProblemAttempts.AddRangeAsync(problemAttempts, cancellationToken);

        var mockInterviews = GenerateMockInterviews(faker, enrollments);
        await _dbContext.MockInterviews.AddRangeAsync(mockInterviews, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ApiResult<AdminGenerateDummyDataResponse>
        {
          StatusCode = HttpStatusCode.OK,
          ResponseBody = new AdminGenerateDummyDataResponse
          {
            NumberOfSeasons = request.NumberOfSeasons,
            NumberOfUsers = request.NumberOfUsers,
          },
          SuccessMessage = Message.SeasonCreatedSuccessfully,
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error generating dummy data");
        await transaction.RollbackAsync(cancellationToken);

        return new ApiResult<AdminGenerateDummyDataResponse>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          Error = new ApiError("Failed to generate dummy data."),
        };
      }
    }
  }
}

public class AdminGenerateDummyDataEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapPost(
        "api/admin/generate-dummy-data",
        async Task<AdminGenerateDummyDataResult> (
          AdminGenerateDummyDataRequest request,
          ISender sender
        ) =>
        {
          var command = new AdminGenerateDummyData.Command
          {
            NumberOfSeasons = request.NumberOfSeasons,
            NumberOfUsers = request.NumberOfUsers,
          };
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminGenerateDummyData");
  }
}

public record AdminGenerateDummyDataRequest
{
  [Required]
  public int NumberOfSeasons { get; set; }

  [Required]
  public int NumberOfUsers { get; set; }
}

public class AdminGenerateDummyDataResponse
{
  [Required]
  public int NumberOfSeasons { get; set; }

  [Required]
  public int NumberOfUsers { get; set; }
}
