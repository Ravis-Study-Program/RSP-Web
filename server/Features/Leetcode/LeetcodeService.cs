using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.Leetcode.Dtos;
using RSPWebAPI.Features.Leetcodes.Interfaces;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;

namespace RSPWebAPI.Features.Leetcodes;

public class LeetcodeService : ILeetcodeService
{
  private readonly IEnrollmentService _enrollmentService;
  private readonly IRepository<LeetcodeProblemCategoryEntity> _leetcodeProblemCategoryRepository;
  private readonly IRepository<LeetcodeProblemRecommendationEntity> _leetcodeProblemRecommendationRepository;
  private readonly IRepository<LeetcodeProblemEntity> _leetcodeProblemRepository;
  private readonly ILogger<LeetcodeService> _logger;
  private readonly IProblemAttemptService _problemAttemptService;
  private readonly IRepository<ProblemEntity> _problemRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IUserService _userService;

  public LeetcodeService(
    IRepository<LeetcodeProblemRecommendationEntity> leetcodeProblemRecommendationRepository,
    IRepository<LeetcodeProblemEntity> leetcodeProblemRepository,
    IRepository<LeetcodeProblemCategoryEntity> leetcodeProblemCategoryRepository,
    IRepository<ProblemEntity> problemRepository,
    IProblemAttemptService problemAttemptService,
    IUserService userService,
    IEnrollmentService enrollmentService,
    IUnitOfWork unitOfWork,
    ILogger<LeetcodeService> logger
  )
  {
    _leetcodeProblemRecommendationRepository = leetcodeProblemRecommendationRepository;
    _leetcodeProblemRepository = leetcodeProblemRepository;
    _leetcodeProblemCategoryRepository = leetcodeProblemCategoryRepository;
    _problemRepository = problemRepository;
    _problemAttemptService = problemAttemptService;
    _userService = userService;
    _enrollmentService = enrollmentService;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  public async Task<IEnumerable<LeetcodeProblemEntity>> GetAllLeetcodeProblemsAsync(
    Expression<Func<LeetcodeProblemEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<LeetcodeProblemEntity>, IQueryable<LeetcodeProblemEntity>>? include = null
  )
  {
    return await _leetcodeProblemRepository.GetAllAsync(predicate, cancellationToken, include);
  }

  public async Task<
    IServiceResponse<AdminPopulateLeetcodeQuestionsResponse>
  > AdminPopulateLeetcodeQuestions(
    AdminPopulateLeetcodeQuestionsRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var skipNumber = 0;

    // Hit Leetcode endpoint to get data
    var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://leetcode.com/graphql/");

    var payload =
      $"{{\"query\":\"query problemsetQuestionList($categorySlug: String, $limit: Int, $skip: Int, $filters: QuestionListFilterInput) {{\\n  problemsetQuestionList: questionList(\\n    categorySlug: $categorySlug\\n    limit: $limit\\n    skip: $skip\\n    filters: $filters\\n  ) {{\\n    total: totalNum\\n    questions: data {{\\n      difficulty\\n      premium: isPaidOnly\\n      questionId: questionFrontendId\\n      title\\n      titleSlug\\n      topicTags {{\\n        name\\n      }}\\n    }}\\n  }}\\n}}\",\"variables\":{{\"categorySlug\":\"\",\"skip\":{skipNumber},\"limit\":10000,\"filters\":{{}}}}}}";

    var httpClient = new HttpClient();
    var content = new StringContent(payload, null, "application/json");
    httpRequest.Content = content;
    var response = await httpClient.SendAsync(httpRequest, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
      _logger.LogError(
        "Request failed. Status: {StatusCode}, Response: {ResponseContent}",
        response.StatusCode,
        errorContent
      );

      return new ErrorServiceResponse<AdminPopulateLeetcodeQuestionsResponse>(
        Message.UnexpectedError
      );
    }

    var output = await response.Content.ReadAsStringAsync(cancellationToken);
    var jsonData = JsonSerializer.Deserialize<LeetcodeProblemListApiResponse>(output);
    if (jsonData == null)
    {
      _logger.LogError("Failed to deserialize or missing required data in API response.");
      return new ErrorServiceResponse<AdminPopulateLeetcodeQuestionsResponse>(
        Message.UnexpectedError
      );
    }

    await _unitOfWork.BeginTransactionAsync(cancellationToken);
    try
    {
      // Gather all unique values
      var categorySet = new HashSet<string>();
      foreach (var question in jsonData.Data.ProblemsetQuestionList.Questions)
      {
        foreach (var category in question.TopicTags)
        {
          categorySet.Add(category.Name);
        }
      }

      // Retrieve existing categories without tracking to avoid duplicate tracking errors
      var existingCategories = _leetcodeProblemCategoryRepository
        .Table.AsNoTracking()
        .ToDictionary(c => c.Name, c => c);

      foreach (var category in categorySet)
      {
        if (!existingCategories.ContainsKey(category))
        {
          var newCategory = new LeetcodeProblemCategoryEntity
          {
            LeetcodeProblemCategoryId = Database.Constants.GeneratePrimaryKeyId(),
            Name = category,
          };
          await _leetcodeProblemCategoryRepository.AddAsync(newCategory, cancellationToken);
          existingCategories[category] = newCategory; // Track the new category in-memory
        }
      }

      await _unitOfWork.SaveChangesAsync(cancellationToken);

      // Add leetcode problems
      foreach (var question in jsonData.Data.ProblemsetQuestionList.Questions)
      {
        var title = $"{question.QuestionId}. {question.Title}";
        var existingLeetcode =
          await _leetcodeProblemRepository.TableNoTracking // Avoid tracking errors
          .FirstOrDefaultAsync(
            p => p.LeetcodeNumber == int.Parse(question.QuestionId),
            cancellationToken
          );
        if (existingLeetcode != null)
        {
          continue;
        }

        var newLeetcodeProblem = new LeetcodeProblemEntity
        {
          LeetcodeProblemId = Database.Constants.GeneratePrimaryKeyId(),
          LeetcodeNumber = int.Parse(question.QuestionId),
          Problem = new ProblemEntity
          {
            ProblemId = Database.Constants.GeneratePrimaryKeyId(),
            Title = title,
            Link = $"https://leetcode.com/problems/{question.TitleSlug}",
          },
          IsPremium = question.Premium,
          LeetcodeProblemCategories = new List<LeetcodeProblemCategoryEntity>(),
        };

        switch (question.Difficulty)
        {
          case "Easy":
            newLeetcodeProblem.LeetcodeProblemDifficulty = LeetcodeProblemDifficulty.Easy;
            break;
          case "Medium":
            newLeetcodeProblem.LeetcodeProblemDifficulty = LeetcodeProblemDifficulty.Medium;
            break;
          case "Hard":
            newLeetcodeProblem.LeetcodeProblemDifficulty = LeetcodeProblemDifficulty.Hard;
            break;
        }

        foreach (var category in question.TopicTags)
        {
          if (existingCategories.TryGetValue(category.Name, out var existingCategory))
          {
            newLeetcodeProblem.LeetcodeProblemCategories.Add(existingCategory);
          }
        }

        await _leetcodeProblemRepository.AddAsync(newLeetcodeProblem, cancellationToken);
      }

      await _unitOfWork.SaveChangesAsync(cancellationToken);
      await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error occurred during transaction.");
      await _unitOfWork.RollbackTransactionAsync(cancellationToken);
      return new ErrorServiceResponse<AdminPopulateLeetcodeQuestionsResponse>(
        Message.UnexpectedError
      );
    }

    return new SuccessServiceResponse<AdminPopulateLeetcodeQuestionsResponse>(
      Message.LeetcodeProblemsScrapedSuccessfully
    );
  }

  public async Task<IServiceResponse<ListLeetcodeProblemsResponse>> ListLeetcodeProblems(
    ListLeetcodeProblemsRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var leetcodeProblems = await GetAllLeetcodeProblemsAsync(
      null,
      cancellationToken,
      q => q.Include(l => l.Problem).Include(l => l.LeetcodeProblemCategories)
    );
    var formattedLeetcodeProblems = leetcodeProblems
      .OrderBy(x => x.LeetcodeNumber)
      .Select(l => new LeetcodeProblemDto
      {
        LeetcodeProblemId = l.LeetcodeProblemId,
        IsPremium = l.IsPremium,
        Link = l.Problem.Link ?? "",
        Title = l.Problem.Title,
        Difficulty = l.LeetcodeProblemDifficulty,
        LeetcodeProblemCategories = l
          .LeetcodeProblemCategories.Select(c => new LeetcodeProblemCategoryDto { Name = c.Name })
          .ToList(),
      })
      .ToList();

    return new SuccessServiceResponse<ListLeetcodeProblemsResponse>(
      Message.LeetcodeProblemsListSuccessfully,
      new ListLeetcodeProblemsResponse { LeetcodeProblems = formattedLeetcodeProblems }
    );
  }
}
