using System.Linq.Expressions;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.ProblemAttempts.Dtos;

namespace RSPWebAPI.Features.ProblemAttempts.Interfaces;

public interface IProblemAttemptService
{
  Task AddProblemAttemptAsync(
    ProblemAttemptEntity problemAttempt,
    CancellationToken cancellationToken = default
  );

  Task DeleteProblemAttemptAsync(
    string problemAttemptId,
    CancellationToken cancellationToken = default
  );

  Task UpdateProblemAttemptAsync(
    ProblemAttemptEntity problemAttempt,
    CancellationToken cancellationToken = default
  );

  Task<IEnumerable<ProblemAttemptEntity>> GetAllProblemAttemptsAsync(
    Expression<Func<ProblemAttemptEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<ProblemAttemptEntity>, IQueryable<ProblemAttemptEntity>>? include = null
  );

  Task<ProblemAttemptEntity?> GetProblemAttemptByIdAsync(
    string problemAttemptId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<ProblemAttemptEntity>, IQueryable<ProblemAttemptEntity>>? include = null
  );

  Task<CreateProblemAttemptResponse> CreateProblemAttempt(
    CreateProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  );

  Task<DeleteProblemAttemptResponse> DeleteProblemAttempt(
    DeleteProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  );

  Task<ListProblemAttemptResponse> ListProblemAttempt(
    ListProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  );

  Task<UpdateProblemAttemptResponse> UpdateProblemAttempt(
    UpdateProblemAttemptRequest request,
    CancellationToken cancellationToken = default
  );
}
