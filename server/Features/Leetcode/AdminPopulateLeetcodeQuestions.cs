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
  public class Command : AdminAuthRequest<ApiResult<AdminPopulateLeetcodeQuestionsResponse>>
  {
  }

  public class Validator : AbstractValidator<Command>
  {
  }

  public class Handler : IRequestHandler<Command, ApiResult<AdminPopulateLeetcodeQuestionsResponse>>
  {
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Handler> _logger;
    public HttpClient _client { get; set; }

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

    public async Task<ApiResult<AdminPopulateLeetcodeQuestionsResponse>> Handle(
      Command request,
      CancellationToken cancellationToken
    )
    {
      // TODO: Implement a more optimal solution in querying. I tried skip parameter but GraphQL doesn't sort it for some reason.
      var skipNumber = 0; 
      
      // Hit Leetcode endpoint to get data
      var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://leetcode.com/graphql/");

      var payload =
        $"{{\"query\":\"query problemsetQuestionList($categorySlug: String, $limit: Int, $skip: Int, $filters: QuestionListFilterInput) {{\\n  problemsetQuestionList: questionList(\\n    categorySlug: $categorySlug\\n    limit: $limit\\n    skip: $skip\\n    filters: $filters\\n  ) {{\\n    total: totalNum\\n    questions: data {{\\n      difficulty\\n      premium: isPaidOnly\\n      questionId: questionFrontendId\\n      title\\n      titleSlug\\n      topicTags {{\\n        name\\n      }}\\n    }}\\n  }}\\n}}\",\"variables\":{{\"categorySlug\":\"\",\"skip\":{skipNumber},\"limit\":10000,\"filters\":{{}}}}}}";

      var content = new StringContent(payload, null, "application/json");
      httpRequest.Content = content;
      var response = await _client.SendAsync(httpRequest, cancellationToken);
      response.EnsureSuccessStatusCode();
      var output = await response.Content.ReadAsStringAsync(cancellationToken);

      var jsonData = JsonSerializer.Deserialize<LeetcodeProblemListApiResponse>(output);
      if (jsonData == null)
      {
        _logger.LogError("Failed to deserialize or missing required data in API response.");
        return new ApiResult<AdminPopulateLeetcodeQuestionsResponse>
        {
          StatusCode = HttpStatusCode.BadRequest,
          Error = new ApiError("Failed to retrieve valid data from the Leetcode API.")
        };
      }
      
      // Process obtained data and save into database
      await using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
      {
        try
        {
          // Gather all unique values
          var categorySet = new HashSet<string>();
          var difficultySet = new HashSet<string>();
          foreach (var question in jsonData.Data.ProblemsetQuestionList.Questions)
          {
            foreach (var category in question.TopicTags)
            {
              categorySet.Add(category.Name);
            }
            difficultySet.Add(question.Difficulty);
          }
          
          // Add to database if it doesn't exist
          foreach (var category in categorySet)
          {
            if (!_dbContext.LeetcodeProblemCategories.Any(c => c.Name == category))
            {
              var newCategory = new LeetcodeProblemCategory { Name = category };
              _dbContext.LeetcodeProblemCategories.Add(newCategory);
            }
          }
          foreach (var difficulty in difficultySet)
          {
            if (!_dbContext.LeetcodeProblemDifficulties.Any(d => d.Name == difficulty))
            {
              var newDifficulty = new LeetcodeProblemDifficulty { Name = difficulty };
              _dbContext.LeetcodeProblemDifficulties.Add(newDifficulty);
            }
          }
          await _dbContext.SaveChangesAsync(cancellationToken);

          // Add leetcode problem
          foreach (var question in jsonData.Data.ProblemsetQuestionList.Questions)
          {
            var title = $"{question.QuestionId}. {question.Title}";
            // Check if the leetcode problem exists (there's probably a better way in doing this for sure)
            var existingLeetcode = await _dbContext
                                         .Problems
                                         .FirstOrDefaultAsync(p => p.Title == title, cancellationToken);
            if (existingLeetcode != null)
            {
              continue;
            }
            
            var newLeetcodeProblem = new LeetcodeProblem
            {
              Problem = new Problem
              {
                Title = title,
                Link = $"https://leetcode.com/problems/{question.TitleSlug}"
              },
              IsPremium = question.Premium,
              LeetcodeProblemCategories = new List<LeetcodeProblemCategory>()
            };

            // Add difficulty
            var existingDifficulty =
              _dbContext.LeetcodeProblemDifficulties.FirstOrDefault(d => d.Name == question.Difficulty);
            if (existingDifficulty != null)
            {
              newLeetcodeProblem.LeetcodeProblemDifficulty = existingDifficulty;
            }
            
            // Add categories
            foreach (var category in question.TopicTags)
            {
              var existingCategory =
                _dbContext.LeetcodeProblemCategories.FirstOrDefault(c => c.Name == category.Name);
              if (existingCategory != null)
              {
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
          Console.WriteLine(ex.Message);
          await transaction.RollbackAsync(cancellationToken);
        }
      }

      return new ApiResult<AdminPopulateLeetcodeQuestionsResponse>
      {
        StatusCode = HttpStatusCode.OK
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

public class AdminPopulateLeetcodeQuestionsResponse
{
}

public class LeetcodeCategory
{
  [JsonPropertyName("name")]
  public string Name { get; set; }
}

public class LeetcodeProblemListQuestion
{
  [JsonPropertyName("difficulty")]
  public string Difficulty { get; set; }
  [JsonPropertyName("premium")]
  public bool Premium { get; set; }
  [JsonPropertyName("questionId")]
  public string QuestionId { get; set; }
  [JsonPropertyName("title")]
  public string Title { get; set; }
  [JsonPropertyName("titleSlug")]
  public string TitleSlug { get; set; }
  [JsonPropertyName("topicTags")]
  public List<LeetcodeCategory> TopicTags { get; set; }
}

public class LeetcodeProblemList
{
  [JsonPropertyName("total")]
  public int Total { get; set; }
  [JsonPropertyName("questions")]
  public List<LeetcodeProblemListQuestion> Questions { get; set; }
}

public class LeetcodeProblemListApiResponseData
{
  [JsonPropertyName("problemsetQuestionList")]
  public LeetcodeProblemList ProblemsetQuestionList { get; set; }
}

public class LeetcodeProblemListApiResponse
{
  [JsonPropertyName("data")]
  public LeetcodeProblemListApiResponseData Data { get; set; }
}
