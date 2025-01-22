using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.ProblemAttempts.Dtos;
using RSPWebAPI.Features.Seasons.Interfaces;
using RSPWebAPI.Features.SeasonWeeks.Dtos;
using RSPWebAPI.Features.SeasonWeeks.Interfaces;

namespace RSPWebAPI.Features.SeasonWeeks;

public class SeasonWeekService : ISeasonWeekService
{
  private readonly ILogger<SeasonWeekService> _logger;
  private readonly ISeasonService _seasonService;
  private readonly IRepository<SeasonWeekEntity> _seasonWeekRepository;
  private readonly IUnitOfWork _unitOfWork;

  public SeasonWeekService(
    IRepository<SeasonWeekEntity> seasonWeekRepository,
    ISeasonService seasonService,
    IUnitOfWork unitOfWork,
    ILogger<SeasonWeekService> logger
  )
  {
    _seasonWeekRepository = seasonWeekRepository;
    _seasonService = seasonService;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  public async Task<IServiceResponse<AdminCreateSeasonWeekResponse>> CreateAdminSeasonWeek(
    AdminCreateSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingSeason = await _seasonService.GetSeasonByIdAsync(
      request.SeasonId,
      cancellationToken
    );
    if (existingSeason == null)
    {
      return new ErrorServiceResponse<AdminCreateSeasonWeekResponse>(Message.SeasonDoesNotExists);
    }

    var existingWeek = await GetSeasonWeekBySeasonId(
      request.SeasonId,
      request.WeekNumber,
      cancellationToken
    );
    if (existingWeek != null)
    {
      return new ErrorServiceResponse<AdminCreateSeasonWeekResponse>(Message.SeasonWeekExists);
    }

    if (
      !IsSeasonWeekDateRangeValid(
        request.StartDate,
        request.EndDate,
        existingSeason.StartDateInclusiveUtc,
        existingSeason.EndDateInclusiveUtc,
        out var errorMessage
      )
    )
    {
      return new ErrorServiceResponse<AdminCreateSeasonWeekResponse>(errorMessage);
    }

    var seasonWeek = new SeasonWeekEntity
    {
      SeasonWeekId = Database.Constants.GeneratePrimaryKeyId(),
      SeasonId = request.SeasonId,
      WeekNumber = request.WeekNumber,
      StartDate = request.StartDate,
      EndDate = request.EndDate,
    };

    try
    {
      await AddSeasonWeekAsync(seasonWeek, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminCreateSeasonWeekResponse>(
        Message.SeasonWeekCreatedSuccessfully,
        new AdminCreateSeasonWeekResponse { SeasonWeekId = seasonWeek.SeasonWeekId }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.SeasonWeekCreationUnexpectedError);
      return new ErrorServiceResponse<AdminCreateSeasonWeekResponse>(
        Message.SeasonWeekCreationUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<AdminDeleteSeasonWeekResponse>> DeleteAdminSeasonWeek(
    AdminDeleteSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingSeasonWeek = await GetSeasonWeekByIdAsync(request.SeasonWeekId, cancellationToken);
    if (existingSeasonWeek == null)
    {
      return new ErrorServiceResponse<AdminDeleteSeasonWeekResponse>(
        Message.SeasonWeekDoesNotExists
      );
    }

    try
    {
      await DeleteSeasonWeekAsync(existingSeasonWeek.SeasonWeekId, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminDeleteSeasonWeekResponse>(
        Message.SeasonWeekDeletedSuccessfully
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.SeasonWeekDeletionUnexpectedError);
      return new ErrorServiceResponse<AdminDeleteSeasonWeekResponse>(
        Message.SeasonWeekDeletionUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<AdminListSeasonWeekResponse>> ListAdminSeasonWeek(
    AdminListSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      var seasonWeeks = await GetAllSeasonWeeksAsync(
        null,
        cancellationToken,
        q => q.Include(s => s.Season)
      );
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminListSeasonWeekResponse>(
        Message.SeasonWeekListSuccessfully,
        new AdminListSeasonWeekResponse { SeasonWeeks = seasonWeeks.ToList() }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.SeasonWeekListUnexpectedError);
      return new ErrorServiceResponse<AdminListSeasonWeekResponse>(
        Message.SeasonWeekListUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<AdminUpdateSeasonWeekResponse>> UpdateAdminSeasonWeek(
    AdminUpdateSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingSeasonWeek = await GetSeasonWeekByIdAsync(request.SeasonWeekId, cancellationToken);
    if (existingSeasonWeek == null)
    {
      return new ErrorServiceResponse<AdminUpdateSeasonWeekResponse>(
        Message.SeasonWeekDoesNotExists
      );
    }

    var existingSeason = await _seasonService.GetSeasonByIdAsync(
      request.SeasonId,
      cancellationToken
    );
    if (existingSeason == null)
    {
      return new ErrorServiceResponse<AdminUpdateSeasonWeekResponse>(Message.SeasonDoesNotExists);
    }

    var existingWeek = await GetSeasonWeekBySeasonId(
      request.SeasonId,
      request.WeekNumber,
      cancellationToken
    );
    if (existingWeek != null)
    {
      return new ErrorServiceResponse<AdminUpdateSeasonWeekResponse>(Message.SeasonWeekExists);
    }

    if (
      !IsSeasonWeekDateRangeValid(
        request.StartDate,
        request.EndDate,
        existingSeason.StartDateInclusiveUtc,
        existingSeason.EndDateInclusiveUtc,
        out var errorMessage
      )
    )
    {
      return new ErrorServiceResponse<AdminUpdateSeasonWeekResponse>(errorMessage);
    }

    existingSeasonWeek.SeasonId = request.SeasonId;
    existingSeasonWeek.WeekNumber = request.WeekNumber;
    existingSeasonWeek.StartDate = request.StartDate;
    existingSeasonWeek.EndDate = request.EndDate;

    try
    {
      await UpdateSeasonWeekAsync(existingSeasonWeek, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminUpdateSeasonWeekResponse>(
        Message.SeasonWeekUpdateSuccessfully
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.SeasonWeekUpdateUnexpectedError);
      return new ErrorServiceResponse<AdminUpdateSeasonWeekResponse>(
        Message.SeasonWeekUpdateUnexpectedError
      );
    }
  }

  public async Task<
    IServiceResponse<GetSeasonWeeksBySeasonSlugResponse>
  > GetSeasonWeeksBySeasonSlug(
    GetSeasonWeeksBySeasonSlugRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var seasonWeeks = await GetAllSeasonWeeksAsync(
      q => q.Season.Slug == request.SeasonSlug,
      cancellationToken,
      q => q.Include(s => s.Season)
    );

    try
    {
      return new SuccessServiceResponse<GetSeasonWeeksBySeasonSlugResponse>(
        Message.SeasonWeekListSuccessfully,
        new GetSeasonWeeksBySeasonSlugResponse() { SeasonWeeks = seasonWeeks.ToList() }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.SeasonWeekListUnexpectedError);
      return new ErrorServiceResponse<GetSeasonWeeksBySeasonSlugResponse>(
        Message.SeasonWeekListUnexpectedError
      );
    }
  }

  public bool IsSeasonWeekDateRangeValid(
    DateTime seasonWeekStartDate,
    DateTime seasonWeekEndDate,
    DateTime seasonStartDate,
    DateTime seasonEndDate,
    out string errorMessage
  )
  {
    if (seasonStartDate > seasonWeekStartDate || seasonEndDate < seasonWeekEndDate)
    {
      errorMessage = Message.SeasonWeekDatesNotWithinSeasonDates;
      return false;
    }

    errorMessage = "";
    return true;
  }

  #region CRUD Operations

  public async Task AddSeasonWeekAsync(
    SeasonWeekEntity seasonWeek,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(seasonWeek);

    await _seasonWeekRepository.AddAsync(seasonWeek, cancellationToken);
  }

  public async Task DeleteSeasonWeekAsync(
    string seasonWeekId,
    CancellationToken cancellationToken = default
  )
  {
    var seasonWeek = await _seasonWeekRepository.GetByIdAsync(seasonWeekId, cancellationToken);

    if (seasonWeek == null)
    {
      throw new KeyNotFoundException(Message.SeasonWeekDoesNotExists);
    }

    _seasonWeekRepository.Delete(seasonWeek, cancellationToken);
  }

  public async Task UpdateSeasonWeekAsync(
    SeasonWeekEntity seasonWeek,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(seasonWeek);

    var existingSeasonWeek = await _seasonWeekRepository.GetByIdAsync(
      seasonWeek.SeasonWeekId,
      cancellationToken
    );
    if (existingSeasonWeek == null)
    {
      throw new KeyNotFoundException(Message.SeasonWeekDoesNotExists);
    }

    _seasonWeekRepository.Update(seasonWeek, cancellationToken);
  }

  public async Task<IEnumerable<SeasonWeekEntity>> GetAllSeasonWeeksAsync(
    Expression<Func<SeasonWeekEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonWeekEntity>, IQueryable<SeasonWeekEntity>>? include = null
  )
  {
    return await _seasonWeekRepository.GetAllAsync(predicate, cancellationToken, include);
  }

  public async Task<SeasonWeekEntity?> GetSeasonWeekByIdAsync(
    string seasonWeekId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonWeekEntity>, IQueryable<SeasonWeekEntity>>? include = null
  )
  {
    return await _seasonWeekRepository.GetByIdAsync(seasonWeekId, cancellationToken, include);
  }

  public async Task<SeasonWeekEntity?> GetSeasonWeekBySeasonId(
    string seasonId,
    int? weekNumber = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonWeekEntity>, IQueryable<SeasonWeekEntity>>? include = null
  )
  {
    return await _seasonWeekRepository.FirstOrDefaultAsync(
      q => q.Season.SeasonId == seasonId && (weekNumber == null || q.WeekNumber == weekNumber),
      cancellationToken,
      include
    );
  }

  #endregion
}
