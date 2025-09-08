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

  public async Task<AdminCreateSeasonWeekResponse> CreateAdminSeasonWeek(
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
      throw new KeyNotFoundException(Messages.Season.DoesNotExist);
    }

    var existingWeek = await GetSeasonWeekBySeasonId(
      request.SeasonId,
      request.WeekNumber,
      cancellationToken
    );
    if (existingWeek != null)
    {
      throw new InvalidOperationException(Messages.SeasonWeek.Exists);
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
      throw new ArgumentException(errorMessage);
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
      return new AdminCreateSeasonWeekResponse { SeasonWeekId = seasonWeek.SeasonWeekId };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.SeasonWeek.CreationError);
      throw new InvalidOperationException(Messages.SeasonWeek.CreationError);
    }
  }

  public async Task<AdminDeleteSeasonWeekResponse> DeleteAdminSeasonWeek(
    AdminDeleteSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingSeasonWeek = await GetSeasonWeekByIdAsync(request.SeasonWeekId, cancellationToken);
    if (existingSeasonWeek == null)
    {
      throw new KeyNotFoundException(Messages.SeasonWeek.DoesNotExist);
    }

    try
    {
      await DeleteSeasonWeekAsync(existingSeasonWeek.SeasonWeekId, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new AdminDeleteSeasonWeekResponse();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.SeasonWeek.DeletionError);
      throw new InvalidOperationException(Messages.SeasonWeek.DeletionError);
    }
  }

  public async Task<AdminListSeasonWeekResponse> ListAdminSeasonWeek(
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
      return new AdminListSeasonWeekResponse { SeasonWeeks = seasonWeeks.ToList() };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.SeasonWeek.ListError);
      throw new InvalidOperationException(Messages.SeasonWeek.ListError);
    }
  }

  public async Task<AdminUpdateSeasonWeekResponse> UpdateAdminSeasonWeek(
    AdminUpdateSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingSeasonWeek = await GetSeasonWeekByIdAsync(request.SeasonWeekId, cancellationToken);
    if (existingSeasonWeek == null)
    {
      throw new KeyNotFoundException(Messages.SeasonWeek.DoesNotExist);
    }

    var existingSeason = await _seasonService.GetSeasonByIdAsync(
      request.SeasonId,
      cancellationToken
    );
    if (existingSeason == null)
    {
      throw new KeyNotFoundException(Messages.Season.DoesNotExist);
    }

    var existingWeek = await GetSeasonWeekBySeasonId(
      request.SeasonId,
      request.WeekNumber,
      cancellationToken
    );
    if (existingWeek != null && existingWeek.SeasonWeekId != request.SeasonWeekId)
    {
      throw new InvalidOperationException(Messages.SeasonWeek.Exists);
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
      throw new ArgumentException(errorMessage);
    }

    existingSeasonWeek.SeasonId = request.SeasonId;
    existingSeasonWeek.WeekNumber = request.WeekNumber;
    existingSeasonWeek.StartDate = request.StartDate;
    existingSeasonWeek.EndDate = request.EndDate;

    try
    {
      await UpdateSeasonWeekAsync(existingSeasonWeek, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new AdminUpdateSeasonWeekResponse();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.SeasonWeek.UpdateError);
      throw new InvalidOperationException(Messages.SeasonWeek.UpdateError);
    }
  }

  public async Task<GetSeasonWeeksBySeasonSlugResponse> GetSeasonWeeksBySeasonSlug(
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
      return new GetSeasonWeeksBySeasonSlugResponse() { SeasonWeeks = seasonWeeks.ToList() };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.SeasonWeek.ListError);
      throw new InvalidOperationException(Messages.SeasonWeek.ListError);
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
      errorMessage = Messages.SeasonWeek.DatesNotWithinSeasonDates;
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
      throw new KeyNotFoundException(Messages.SeasonWeek.DoesNotExist);
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
      throw new KeyNotFoundException(Messages.SeasonWeek.DoesNotExist);
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
