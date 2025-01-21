using System.Text.Json.Serialization;

namespace RSPWebAPI.Features.Leetcode.Dtos;

public record AdminPopulateLeetcodeQuestionsRequest { };

public record AdminPopulateLeetcodeQuestionsResponse { }

public class LeetcodeCategory
{
  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;
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
