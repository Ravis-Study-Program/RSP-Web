using System.Linq.Expressions;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.MockInterviews.Dtos;

namespace RSPWebAPI.Features.MockInterviews.Interfaces;

public interface IMockInterviewService
{
  Task AddMockInterviewAsync(
    MockInterviewEntity mockInterview,
    CancellationToken cancellationToken = default
  );

  Task DeleteMockInterviewAsync(
    string mockInterviewId,
    CancellationToken cancellationToken = default
  );

  Task UpdateMockInterviewAsync(
    MockInterviewEntity mockInterview,
    CancellationToken cancellationToken = default
  );

  Task<IEnumerable<MockInterviewEntity>> GetAllMockInterviewsAsync(
    Expression<Func<MockInterviewEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<MockInterviewEntity>, IQueryable<MockInterviewEntity>>? include = null
  );

  Task<MockInterviewEntity?> GetMockInterviewByIdAsync(
    string mockInterviewId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<MockInterviewEntity>, IQueryable<MockInterviewEntity>>? include = null
  );

  Task<IServiceResponse<CreateMockInterviewResponse>> CreateMockInterview(
    CreateMockInterviewRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<DeleteMockInterviewResponse>> DeleteMockInterview(
    DeleteMockInterviewRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<ListMockInterviewResponse>> ListMockInterview(
    ListMockInterviewRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<UpdateMockInterviewResponse>> UpdateMockInterview(
    UpdateMockInterviewRequest request,
    CancellationToken cancellationToken = default
  );
}
