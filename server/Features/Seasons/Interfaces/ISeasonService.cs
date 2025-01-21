using System.Linq.Expressions;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Seasons.Dtos;

namespace RSPWebAPI.Features.Seasons.Interfaces;

public interface ISeasonService
{
  Task AddSeasonAsync(SeasonEntity season, CancellationToken cancellationToken = default);
  Task DeleteSeasonAsync(string seasonId, CancellationToken cancellationToken = default);
  Task UpdateSeasonAsync(SeasonEntity season, CancellationToken cancellationToken = default);

  Task<IEnumerable<SeasonEntity>> GetAllSeasonsAsync(
    Expression<Func<SeasonEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonEntity>, IQueryable<SeasonEntity>>? include = null
  );

  Task<SeasonEntity?> GetSeasonByIdAsync(
    string seasonId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonEntity>, IQueryable<SeasonEntity>>? include = null
  );

  Task<SeasonEntity?> GetSeasonBySlugAsync(
    string seasonSlug,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonEntity>, IQueryable<SeasonEntity>>? include = null
  );

  Task<IServiceResponse<AdminCreateSeasonResponse>> CreateAdminSeason(
    AdminCreateSeasonRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<AdminDeleteSeasonResponse>> DeleteAdminSeason(
    AdminDeleteSeasonRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<AdminListSeasonResponse>> ListAdminSeason(
    AdminListSeasonRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<AdminUpdateSeasonResponse>> UpdateAdminSeason(
    AdminUpdateSeasonRequest request,
    CancellationToken cancellationToken = default
  );
}
