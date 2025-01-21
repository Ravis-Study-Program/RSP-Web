using System.Linq.Expressions;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Seasons.Dtos;
using RSPWebAPI.Features.Seasons.Interfaces;

namespace RSPWebAPI.Features.Seasons;

public class SeasonService : ISeasonService
{
  private readonly ILogger<SeasonService> _logger;
  private readonly IRepository<SeasonEntity> _seasonRepository;
  private readonly IUnitOfWork _unitOfWork;

  public SeasonService(
    IRepository<SeasonEntity> seasonRepository,
    IUnitOfWork unitOfWork,
    ILogger<SeasonService> logger
  )
  {
    _seasonRepository = seasonRepository;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  public async Task<IServiceResponse<AdminCreateSeasonResponse>> CreateAdminSeason(
    AdminCreateSeasonRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingSeason = await GetSeasonBySlugAsync(request.Slug, cancellationToken);
    if (existingSeason != null)
    {
      return new ErrorServiceResponse<AdminCreateSeasonResponse>(Message.SeasonExists);
    }

    var season = new SeasonEntity
    {
      SeasonId = Database.Constants.GeneratePrimaryKeyId(),
      Name = request.Name,
      Slug = request.Slug,
      StartDateInclusiveUtc = request.StartDateInclusiveUtc,
      EndDateInclusiveUtc = request.EndDateInclusiveUtc,
      Location = request.Location,
      ImageUrl = request.ImageUrl,
    };

    try
    {
      await AddSeasonAsync(season, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminCreateSeasonResponse>(
        Message.SeasonCreatedSuccessfully,
        new AdminCreateSeasonResponse { SeasonId = season.SeasonId }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.SeasonCreationUnexpectedError);
      return new ErrorServiceResponse<AdminCreateSeasonResponse>(
        Message.SeasonCreationUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<AdminDeleteSeasonResponse>> DeleteAdminSeason(
    AdminDeleteSeasonRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingSeason = await GetSeasonByIdAsync(request.SeasonId, cancellationToken);
    if (existingSeason == null)
    {
      return new ErrorServiceResponse<AdminDeleteSeasonResponse>(Message.SeasonDoesNotExists);
    }

    try
    {
      await DeleteSeasonAsync(existingSeason.SeasonId, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminDeleteSeasonResponse>(
        Message.SeasonDeletedSuccessfully
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.SeasonDeletionUnexpectedError);
      return new ErrorServiceResponse<AdminDeleteSeasonResponse>(
        Message.SeasonDeletionUnexpectedError
      );
    }
  }

  public async Task<IServiceResponse<AdminListSeasonResponse>> ListAdminSeason(
    AdminListSeasonRequest request,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      var seasons = await GetAllSeasonsAsync(null, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminListSeasonResponse>(
        Message.SeasonListSuccessfully,
        new AdminListSeasonResponse() { Seasons = seasons.ToList() }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.SeasonListUnexpectedError);
      return new ErrorServiceResponse<AdminListSeasonResponse>(Message.SeasonListUnexpectedError);
    }
  }

  public async Task<IServiceResponse<AdminUpdateSeasonResponse>> UpdateAdminSeason(
    AdminUpdateSeasonRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingSeason = await GetSeasonByIdAsync(request.SeasonId, cancellationToken);
    if (existingSeason == null)
    {
      return new ErrorServiceResponse<AdminUpdateSeasonResponse>(Message.SeasonDoesNotExists);
    }

    existingSeason.Name = request.Name;
    existingSeason.Slug = request.Slug;
    existingSeason.StartDateInclusiveUtc = request.StartDateInclusiveUtc;
    existingSeason.EndDateInclusiveUtc = request.EndDateInclusiveUtc;
    existingSeason.Location = request.Location;
    existingSeason.ImageUrl = request.ImageUrl;

    try
    {
      await UpdateSeasonAsync(existingSeason, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminUpdateSeasonResponse>(
        Message.SeasonUpdatedSuccessfully
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.SeasonUpdateUnexpectedError);
      return new ErrorServiceResponse<AdminUpdateSeasonResponse>(
        Message.SeasonUpdateUnexpectedError
      );
    }
  }

  #region CRUD Operations

  public async Task AddSeasonAsync(
    SeasonEntity season,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(season);

    await _seasonRepository.AddAsync(season, cancellationToken);
  }

  public async Task DeleteSeasonAsync(
    string seasonId,
    CancellationToken cancellationToken = default
  )
  {
    var season = await _seasonRepository.GetByIdAsync(seasonId, cancellationToken);

    if (season == null)
    {
      throw new KeyNotFoundException(Message.SeasonDoesNotExists);
    }

    _seasonRepository.Delete(season, cancellationToken);
  }

  public async Task UpdateSeasonAsync(
    SeasonEntity season,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(season);

    var existingSeason = await _seasonRepository.GetByIdAsync(season.SeasonId, cancellationToken);
    if (existingSeason == null)
    {
      throw new KeyNotFoundException(Message.SeasonDoesNotExists);
    }

    _seasonRepository.Update(season, cancellationToken);
  }

  public async Task<IEnumerable<SeasonEntity>> GetAllSeasonsAsync(
    Expression<Func<SeasonEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonEntity>, IQueryable<SeasonEntity>>? include = null
  )
  {
    return await _seasonRepository.GetAllAsync(predicate, cancellationToken, include);
  }

  public async Task<SeasonEntity?> GetSeasonByIdAsync(
    string seasonId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonEntity>, IQueryable<SeasonEntity>>? include = null
  )
  {
    return await _seasonRepository.GetByIdAsync(seasonId, cancellationToken, include);
  }

  public async Task<SeasonEntity?> GetSeasonBySlugAsync(
    string? seasonSlug,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonEntity>, IQueryable<SeasonEntity>>? include = null
  )
  {
    return await _seasonRepository.FirstOrDefaultAsync(
      q => q.Slug == seasonSlug,
      cancellationToken,
      include
    );
  }

  #endregion
}
