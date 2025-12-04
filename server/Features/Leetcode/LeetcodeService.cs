using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Cache;
using RSPWEBAPI.Common.Cache;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.Leetcode.Dtos;
using RSPWebAPI.Features.Leetcodes.Interfaces;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;

namespace RSPWebAPI.Features.Leetcodes;

public class LeetcodeService : BaseService, ILeetcodeService
{
  private readonly IEnrollmentService _enrollmentService;
  private readonly IRepository<LeetcodeProblemCategoryEntity> _leetcodeProblemCategoryRepository;
  private readonly IRepository<LeetcodeProblemRecommendationEntity> _leetcodeProblemRecommendationRepository;
  private readonly IRepository<LeetcodeProblemEntity> _leetcodeProblemRepository;
  private readonly IProblemAttemptService _problemAttemptService;
  private readonly IRepository<ProblemEntity> _problemRepository;
  private readonly IRequestCache _cache;
  private readonly IUserService _userService;

  public LeetcodeService(
    IRepository<LeetcodeProblemRecommendationEntity> leetcodeProblemRecommendationRepository,
    IRepository<LeetcodeProblemEntity> leetcodeProblemRepository,
    IRepository<LeetcodeProblemCategoryEntity> leetcodeProblemCategoryRepository,
    IRepository<ProblemEntity> problemRepository,
    IProblemAttemptService problemAttemptService,
    IUserService userService,
    IEnrollmentService enrollmentService,
    IRequestCache cache,
    IUnitOfWork unitOfWork,
    ILogger<LeetcodeService> logger
  )
    : base(unitOfWork, logger)
  {
    _leetcodeProblemRecommendationRepository = leetcodeProblemRecommendationRepository;
    _leetcodeProblemRepository = leetcodeProblemRepository;
    _leetcodeProblemCategoryRepository = leetcodeProblemCategoryRepository;
    _problemRepository = problemRepository;
    _problemAttemptService = problemAttemptService;
    _userService = userService;
    _enrollmentService = enrollmentService;
    _cache = cache;
  }

  public async Task<IEnumerable<LeetcodeProblemEntity>> GetAllLeetcodeProblemsAsync(
    Expression<Func<LeetcodeProblemEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<LeetcodeProblemEntity>, IQueryable<LeetcodeProblemEntity>>? include = null
  )
  {
    return await _leetcodeProblemRepository.GetAllAsync(predicate, cancellationToken, include);
  }

  public async Task<AdminPopulateLeetcodeQuestionsResponse> AdminPopulateLeetcodeQuestions(
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

      throw new InvalidOperationException(Messages.Common.UnexpectedError);
    }

    var output = await response.Content.ReadAsStringAsync(cancellationToken);
    var jsonData = JsonSerializer.Deserialize<LeetcodeProblemListApiResponse>(output);
    if (jsonData == null)
    {
      _logger.LogError("Failed to deserialize or missing required data in API response.");
      throw new InvalidOperationException(Messages.Common.UnexpectedError);
    }

    return await ExecuteWithTransactionAsync(
      async () =>
      {
        // Gather all unique values
        var categorySet = new HashSet<string>();
        foreach (var question in jsonData.Data.ProblemsetQuestionList.Questions)
        {
          foreach (var category in question.TopicTags)
          {
            categorySet.Add(category.Name.Trim());
          }
        }

        var existingCategories = _leetcodeProblemCategoryRepository.Table.ToDictionary(
          c => c.Name.Trim(),
          c => c
        );

        var existingIds = _leetcodeProblemCategoryRepository
          .Table.Select(c => c.LeetcodeProblemCategoryId)
          .ToHashSet();

        var pendingNewCategories = new List<LeetcodeProblemCategoryEntity>();

        foreach (var categoryName in categorySet)
        {
          if (existingCategories.ContainsKey(categoryName))
            continue;

          const int maxRetries = 5;
          string newId = null!;
          int retryCount = 0;

          do
          {
            newId = Database.Constants.GeneratePrimaryKeyId();
            retryCount++;
          } while (existingIds.Contains(newId) && retryCount < maxRetries);

          if (existingIds.Contains(newId))
          {
            throw new Exception(
              $"Failed to generate unique ID for category '{categoryName}' after {maxRetries} attempts."
            );
          }

          var newCategory = new LeetcodeProblemCategoryEntity
          {
            LeetcodeProblemCategoryId = newId,
            Name = categoryName,
          };

          pendingNewCategories.Add(newCategory);
          existingIds.Add(newId);
          existingCategories[categoryName] = newCategory;
        }

        foreach (var newCategory in pendingNewCategories)
        {
          await _leetcodeProblemCategoryRepository.AddAsync(newCategory, cancellationToken);
        }

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
            if (existingCategories.TryGetValue(category.Name.Trim(), out var existingCategory))
            {
              newLeetcodeProblem.LeetcodeProblemCategories.Add(existingCategory);
            }
          }

          await _leetcodeProblemRepository.AddAsync(newLeetcodeProblem, cancellationToken);
        }

        _cache.Remove(RouteCacheKeys.ListLeetcodeProblems);

        return new AdminPopulateLeetcodeQuestionsResponse();
      },
      Messages.Common.UnexpectedError,
      cancellationToken
    );
  }

  public async Task<ListLeetcodeProblemsResponse> ListLeetcodeProblems(
    ListLeetcodeProblemsRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var response = await _cache.GetOrCreateAsync(
      routeKey: RouteCacheKeys.ListLeetcodeProblems,
      primaryKey: null,
      factory: () => _listLeetcodeProblems(request, cancellationToken),
      ttl: TimeSpan.FromDays(1)
    );

    if (response == null)
    {
      throw new InvalidOperationException("Error listing leetcode problems");
    }

    return response;
  }

  private async Task<ListLeetcodeProblemsResponse> _listLeetcodeProblems(
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

    return new ListLeetcodeProblemsResponse { LeetcodeProblems = formattedLeetcodeProblems };
  }
}