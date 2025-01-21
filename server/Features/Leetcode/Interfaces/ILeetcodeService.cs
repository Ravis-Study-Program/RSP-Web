using System.Linq.Expressions;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Leetcode.Dtos;

namespace RSPWebAPI.Features.Leetcodes.Interfaces;

public interface ILeetcodeService
{
  Task<IEnumerable<LeetcodeProblemEntity>> GetAllLeetcodeProblemsAsync(
    Expression<Func<LeetcodeProblemEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<LeetcodeProblemEntity>, IQueryable<LeetcodeProblemEntity>>? include = null
  );

  Task<IServiceResponse<AdminPopulateLeetcodeQuestionsResponse>> AdminPopulateLeetcodeQuestions(
    AdminPopulateLeetcodeQuestionsRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<ListLeetcodeProblemsResponse>> ListLeetcodeProblems(
    ListLeetcodeProblemsRequest request,
    CancellationToken cancellationToken = default
  );
}
