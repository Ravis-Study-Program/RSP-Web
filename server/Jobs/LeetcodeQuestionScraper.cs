using RSPWebAPI.Features.Leetcode.Dtos;
using RSPWebAPI.Features.Leetcodes.Interfaces;

namespace RSPWebAPI.Jobs;

public class LeetcodeQuestionScraper : BackgroundService
{
  private readonly ILeetcodeService _leetcodeService;

  public LeetcodeQuestionScraper(ILeetcodeService leetcodeService)
  {
    _leetcodeService = leetcodeService;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        var request = new AdminPopulateLeetcodeQuestionsRequest();
        await _leetcodeService.AdminPopulateLeetcodeQuestions(request, stoppingToken);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
      }

      await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
    }
  }
}
