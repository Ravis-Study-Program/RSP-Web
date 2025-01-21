using System.Linq.Expressions;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Mentorships.Dtos;

namespace RSPWebAPI.Features.Mentorships.Interfaces;

public interface IMentorshipService
{
  Task AddMentorshipAsync(
    MentorshipEntity mentorship,
    CancellationToken cancellationToken = default
  );

  Task DeleteMentorshipAsync(string mentorshipId, CancellationToken cancellationToken = default);

  Task UpdateMentorshipAsync(
    MentorshipEntity mentorship,
    CancellationToken cancellationToken = default
  );

  Task<IEnumerable<MentorshipEntity>> GetAllMentorshipsAsync(
    Expression<Func<MentorshipEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<MentorshipEntity>, IQueryable<MentorshipEntity>>? include = null
  );

  Task<MentorshipEntity?> GetMentorshipByIdAsync(
    string mentorshipId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<MentorshipEntity>, IQueryable<MentorshipEntity>>? include = null
  );

  Task<IServiceResponse<AdminCreateMentorshipResponse>> CreateAdminMentorship(
    AdminCreateMentorshipRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<AdminDeleteMentorshipResponse>> DeleteAdminMentorship(
    AdminDeleteMentorshipRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<AdminListMentorshipResponse>> ListAdminMentorship(
    AdminListMentorshipRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<AdminUpdateMentorshipResponse>> UpdateAdminMentorship(
    AdminUpdateMentorshipRequest request,
    CancellationToken cancellationToken = default
  );

  Task<IServiceResponse<GetCurrentUserMenteesListResponse>> GetCurrentUserMenteesList(
    GetCurrentUserMenteesListRequest request,
    CancellationToken cancellationToken = default
  );
}
