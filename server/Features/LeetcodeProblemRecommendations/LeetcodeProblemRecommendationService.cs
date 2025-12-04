using System.Linq.Expressions;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Extensions;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Enrollments.Interfaces;
using RSPWebAPI.Features.LeetcodeProblemRecommendations.Dtos;
using RSPWebAPI.Features.LeetcodeProblemRecommendations.Interfaces;
using RSPWebAPI.Features.ProblemAttempts.Dtos;
using RSPWebAPI.Features.ProblemAttempts.Interfaces;
using RSPWebAPI.Features.Users.Interfaces;

namespace RSPWebAPI.Features.LeetcodeProblemRecommendations;

public class LeetcodeProblemRecommendationService
  : BaseService,
    ILeetcodeProblemRecommendationService
{
  private readonly IEnrollmentService _enrollmentService;
  private readonly IRepository<LeetcodeProblemCategoryEntity> _leetcodeProblemCategoryRepository;
  private readonly IRepository<LeetcodeProblemRecommendationEntity> _leetcodeProblemRecommendationRepository;
  private readonly IRepository<LeetcodeProblemEntity> _leetcodeProblemRepository;
  private readonly IProblemAttemptService _problemAttemptService;
  private readonly IRepository<ProblemEntity> _problemRepository;
  private readonly IUserService _userService;

  public LeetcodeProblemRecommendationService(
    IRepository<LeetcodeProblemRecommendationEntity> leetcodeProblemRecommendationRepository,
    IRepository<LeetcodeProblemEntity> leetcodeProblemRepository,
    IRepository<LeetcodeProblemCategoryEntity> leetcodeProblemCategoryRepository,
    IRepository<ProblemEntity> problemRepository,
    IProblemAttemptService problemAttemptService,
    IUserService userService,
    IEnrollmentService enrollmentService,
    IUnitOfWork unitOfWork,
    ILogger<LeetcodeProblemRecommendationService> logger
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
  }

  public async Task<GenerateLeetcodeProblemRecommendationResponse> GenerateLeetcodeProblemRecommendation(
    GenerateLeetcodeProblemRecommendationRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingRecommendation = await GetUnsolvedByUserId(request.UserId, cancellationToken);
    if (existingRecommendation != null)
    {
      // Return success even though we didn't generate anything
      return new GenerateLeetcodeProblemRecommendationResponse
      {
        LeetcodeProblemRecommendationId = existingRecommendation.LeetcodeProblemRecommendationId,
      };
    }

    var user = await _userService.GetUserAsync(
      userId: request.UserId,
      cancellationToken: cancellationToken
    );
    if (user == null)
    {
      throw new KeyNotFoundException(Messages.User.EmailDoesNotExist);
    }

    var problemAttemptsResponse = await _problemAttemptService.ListProblemAttempt(
      new ListProblemAttemptRequest
      {
        UserIds = new List<string> { user.UserId },
        IncludeLeetcode = true,
        IncludeCustom = false,
      },
      cancellationToken
    );
    var problemRecommendation = await GenerateRandom(
      problemAttemptsResponse
        ?.ProblemAttempts.Select(dto => new ProblemAttemptEntity
        {
          ProblemAttemptId = dto.ProblemAttemptId,
          AttemptStartDateUtc = dto.AttemptStartDateUtc,
          TimeTakenInMinutes = dto.TimeTakenInMinutes,
          Notes = dto.Notes,
          LeetcodeProblemId = dto.LeetcodeProblemId,
          CustomProblemId = dto.CustomProblemId,
          SeasonWeekId = dto.SeasonWeekId,
        })
        .ToList() ?? new List<ProblemAttemptEntity>(),
      cancellationToken
    );
    if (problemRecommendation == null)
    {
      throw new InvalidOperationException(Messages.LeetcodeProblemRecommendation.NoProblemsLeft);
    }

    var newRecommendation = new LeetcodeProblemRecommendationEntity
    {
      LeetcodeProblemRecommendationId = Database.Constants.GeneratePrimaryKeyId(),
      UserId = request.UserId,
      LeetcodeProblemId = problemRecommendation.LeetcodeProblemId,
    };

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await AddLeetcodeProblemRecommendationAsync(newRecommendation, cancellationToken);
        return new GenerateLeetcodeProblemRecommendationResponse
        {
          LeetcodeProblemRecommendationId = newRecommendation.LeetcodeProblemRecommendationId,
        };
      },
      Messages.LeetcodeProblemRecommendation.CreationError,
      cancellationToken
    );
  }

  #region CRUD Operations

  public async Task AddLeetcodeProblemRecommendationAsync(
    LeetcodeProblemRecommendationEntity leetcodeProblemRecommendation,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(leetcodeProblemRecommendation);

    await _leetcodeProblemRecommendationRepository.AddAsync(
      leetcodeProblemRecommendation,
      cancellationToken
    );
  }

  #endregion

  public async Task<IEnumerable<LeetcodeProblemEntity>> GetAllLeetcodeProblemsAsync(
    Expression<Func<LeetcodeProblemEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<LeetcodeProblemEntity>, IQueryable<LeetcodeProblemEntity>>? include = null
  )
  {
    return await _leetcodeProblemRepository.GetAllAsync(predicate, cancellationToken, include);
  }

  private async Task<LeetcodeProblemRecommendationEntity?> GetUnsolvedByUserId(
    string userId,
    CancellationToken cancellationToken = default
  )
  {
    return await _leetcodeProblemRecommendationRepository.FirstOrDefaultAsync(
      r => r.UserId == userId && r.ProblemAttemptId == null,
      cancellationToken
    );
  }

  private async Task<LeetcodeProblemEntity?> GenerateRandom(
    List<ProblemAttemptEntity> problemAttempts,
    CancellationToken cancellationToken = default
  )
  {
    var leetcodeProblemIds = problemAttempts.Select(p => p.LeetcodeProblemId).ToList();
    var newRecommendation = await _leetcodeProblemRepository.FirstOrDefaultAsync(
      problem => !leetcodeProblemIds.Contains(problem.LeetcodeProblemId),
      cancellationToken
    );
    return newRecommendation;
  }
}
