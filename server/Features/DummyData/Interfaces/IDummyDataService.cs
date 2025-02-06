using System.Linq.Expressions;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.DummyData.Dtos;

namespace RSPWebAPI.Features.DummyData.Interfaces;

public interface IDummyDataService
{
  Task<IServiceResponse<AdminGenerateDummyDataResponse>> AdminGenerateDummyData(
    AdminGenerateDummyDataRequest request,
    CancellationToken cancellationToken = default
  );
}
