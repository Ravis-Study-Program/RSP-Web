using System.Linq.Expressions;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.SeasonWeeks.Dtos;

namespace RSPWebAPI.Features.SeasonWeeks.Interfaces;

public interface ISeasonWeekService
{
  Task AddSeasonWeekAsync(
    SeasonWeekEntity seasonWeek,
    CancellationToken cancellationToken = default
  );

  Task DeleteSeasonWeekAsync(string seasonWeekId, CancellationToken cancellationToken = default);

  Task UpdateSeasonWeekAsync(
    SeasonWeekEntity seasonWeek,
    CancellationToken cancellationToken = default
  );

  Task<IEnumerable<SeasonWeekEntity>> GetAllSeasonWeeksAsync(
    Expression<Func<SeasonWeekEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonWeekEntity>, IQueryable<SeasonWeekEntity>>? include = null
  );

  Task<SeasonWeekEntity?> GetSeasonWeekByIdAsync(
    string seasonWeekId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonWeekEntity>, IQueryable<SeasonWeekEntity>>? include = null
  );

  Task<SeasonWeekEntity?> GetSeasonWeekBySeasonId(
    string seasonId,
    int? weekNumber = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<SeasonWeekEntity>, IQueryable<SeasonWeekEntity>>? include = null
  );

  Task<IServiceResponse<AdminCreateSeasonWeekResponse>> CreateAdminSeasonWeek(
    AdminCreateSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<AdminDeleteSeasonWeekResponse>> DeleteAdminSeasonWeek(
    AdminDeleteSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<AdminListSeasonWeekResponse>> ListAdminSeasonWeek(
    AdminListSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<AdminUpdateSeasonWeekResponse>> UpdateAdminSeasonWeek(
    AdminUpdateSeasonWeekRequest request,
    CancellationToken cancellationToken = default
  );

  bool IsSeasonWeekDateRangeValid(
    DateTime seasonWeekStartDate,
    DateTime seasonWeekEndDate,
    DateTime seasonStartDate,
    DateTime seasonEndDate,
    out string errorMessage
  );
}
