using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Leetcode;

public static class AdminPopulateLeetcodeQuestions
{
  public class Command : AdminAuthRequest<ApiResult<AdminPopulateLeetcodeQuestionsResponse>> { }

  public class Validator : AbstractValidator<Command> { }

  public class Handler : IRequestHandler<Command, ApiResult<AdminPopulateLeetcodeQuestionsResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger)
    {
      _dbContext = dbContext;
      _logger = logger;
      _client = new HttpClient();
    }

    public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger, HttpClient client)
    {
      _dbContext = dbContext;
      _logger = logger;
      _client = client;
    }

    public HttpClient _client { get; set; }

    public async Task<ApiResult<AdminPopulateLeetcodeQuestionsResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      var skipNumber = 0;

      // Hit Leetcode endpoint to get data
      var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://leetcode.com/graphql/");

      var payload =
        $"{{\"query\":\"query problemsetQuestionList($categorySlug: String, $limit: Int, $skip: Int, $filters: QuestionListFilterInput) {{\\n  problemsetQuestionList: questionList(\\n    categorySlug: $categorySlug\\n    limit: $limit\\n    skip: $skip\\n    filters: $filters\\n  ) {{\\n    total: totalNum\\n    questions: data {{\\n      difficulty\\n      premium: isPaidOnly\\n      questionId: questionFrontendId\\n      title\\n      titleSlug\\n      topicTags {{\\n        name\\n      }}\\n    }}\\n  }}\\n}}\",\"variables\":{{\"categorySlug\":\"\",\"skip\":{skipNumber},\"limit\":10000,\"filters\":{{}}}}}}";

      var content = new StringContent(payload, null, "application/json");
      httpRequest.Content = content;
      var response = await _client.SendAsync(httpRequest, cancellationToken);

      if (!response.IsSuccessStatusCode)
      {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError(
          "Request failed. Status: {StatusCode}, Response: {ResponseContent}",
          response.StatusCode,
          errorContent
        );

        return new ApiResult<AdminPopulateLeetcodeQuestionsResponse>
        {
          StatusCode = response.StatusCode,
          Error = new ApiError("Failed to retrieve data from the Leetcode API."),
        };
      }

      var output = await response.Content.ReadAsStringAsync(cancellationToken);
      var jsonData = JsonSerializer.Deserialize<LeetcodeProblemListApiResponse>(output);
      if (jsonData == null)
      {
        _logger.LogError("Failed to deserialize or missing required data in API response.");
        return new ApiResult<AdminPopulateLeetcodeQuestionsResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError("Failed to retrieve valid data from the Leetcode API."),
        };
      }

      await using (
        var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken)
      )
      {
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
          var existingCategories = _dbContext
            .LeetcodeProblemCategories.AsNoTracking()
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
              _dbContext.LeetcodeProblemCategories.Add(newCategory);
              existingCategories[category] = newCategory; // Track the new category in-memory
            }
            else
            {
              // Attach existing category to ensure it's linked to the context without adding duplicates
              _dbContext.LeetcodeProblemCategories.Attach(existingCategories[category]);
            }
          }

          await _dbContext.SaveChangesAsync(cancellationToken);

          // Add leetcode problems
          foreach (var question in jsonData.Data.ProblemsetQuestionList.Questions)
          {
            var title = $"{question.QuestionId}. {question.Title}";
            var existingLeetcode = await _dbContext
              .Problems.AsNoTracking() // Avoid tracking errors
              .FirstOrDefaultAsync(p => p.Title == title, cancellationToken);
            if (existingLeetcode != null)
            {
              continue;
            }

            var newLeetcodeProblem = new LeetcodeProblemEntity
            {
              LeetcodeProblemId = Database.Constants.GeneratePrimaryKeyId(),
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
                // Attach each category to avoid duplicate tracking
                _dbContext.LeetcodeProblemCategories.Attach(existingCategory);
                newLeetcodeProblem.LeetcodeProblemCategories.Add(existingCategory);
              }
            }

            _dbContext.LeetcodeProblems.Add(newLeetcodeProblem);
          }

          await _dbContext.SaveChangesAsync(cancellationToken);
          await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error occurred during transaction.");
          await transaction.RollbackAsync(cancellationToken);
          return new ApiResult<AdminPopulateLeetcodeQuestionsResponse>
          {
            StatusCode = HttpStatusCode.InternalServerError,
            Error = new ApiError("An error occurred while populating questions."),
          };
        }
      }

      return new ApiResult<AdminPopulateLeetcodeQuestionsResponse>
      {
        StatusCode = HttpStatusCode.OK,
      };
    }
  }
}

public class AdminPopulateLeetcodeQuestionsEndpoint : ICarterModule
{
  public void AddRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet(
        "api/admin/leetcode/populate-questions",
        async (ISender sender) =>
        {
          var command = new AdminPopulateLeetcodeQuestions.Command();
          var response = await sender.Send(command);

          return ApiResultHelper.FormatResponse(response);
        }
      )
      .WithName("AdminPopulateLeetcodeQuestions");
  }
}

public class AdminPopulateLeetcodeQuestionsResponse { }

public class LeetcodeCategory
{
  [JsonPropertyName("name")]
  public string Name { get; set; }
}

public class LeetcodeProblemListQuestion
{
  [JsonPropertyName("difficulty")]
  public string Difficulty { get; set; } = string.Empty;

  [JsonPropertyName("premium")]
  public bool Premium { get; set; }

  [JsonPropertyName("questionId")]
  public string QuestionId { get; set; } = string.Empty;

  [JsonPropertyName("title")]
  public string Title { get; set; } = string.Empty;

  [JsonPropertyName("titleSlug")]
  public string TitleSlug { get; set; } = string.Empty;

  [JsonPropertyName("topicTags")]
  public List<LeetcodeCategory> TopicTags { get; set; } = new();
}

public class LeetcodeProblemList
{
  [JsonPropertyName("total")]
  public int Total { get; set; }

  [JsonPropertyName("questions")]
  public List<LeetcodeProblemListQuestion> Questions { get; set; } = new();
}

public class LeetcodeProblemListApiResponseData
{
  [JsonPropertyName("problemsetQuestionList")]
  public LeetcodeProblemList ProblemsetQuestionList { get; set; } = new();
}

public class LeetcodeProblemListApiResponse
{
  [JsonPropertyName("data")]
  public LeetcodeProblemListApiResponseData Data { get; set; } = new();
}
