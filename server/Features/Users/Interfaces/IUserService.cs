using System.Linq.Expressions;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Users.Dtos;

namespace RSPWebAPI.Features.Users.Interfaces;

public interface IUserService
{
  Task AddUserAsync(UserEntity user, CancellationToken cancellationToken = default);
  Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
  Task UpdateUserAsync(UserEntity user, CancellationToken cancellationToken = default);

  Task<IEnumerable<UserEntity>> GetAllUsersAsync(
    Expression<Func<UserEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<UserEntity>, IQueryable<UserEntity>>? include = null
  );

  Task<UserEntity?> GetUserAsync(
    string? userId = null,
    string? email = null,
    string? slug = null,
    Func<IQueryable<UserEntity>, IQueryable<UserEntity>>? include = null,
    bool bypassCache = false,
    CancellationToken cancellationToken = default
  );

  Task<AdminCreateUserResponse> CreateAdminUser(
    AdminCreateUserRequest request,
    CancellationToken cancellationToken = default
  );

  Task<AdminDeleteUserResponse> DeleteAdminUser(
    AdminDeleteUserRequest request,
    CancellationToken cancellationToken = default
  );

  Task<AdminListUserResponse> ListAdminUser(
    AdminListUserRequest request,
    CancellationToken cancellationToken = default
  );

  Task<AdminUpdateUserResponse> UpdateAdminUser(
    AdminUpdateUserRequest request,
    CancellationToken cancellationToken = default
  );

  Task<GetCurrentUserResponse> GetCurrentUser(
    string? userId,
    CancellationToken cancellationToken = default
  );

  Task<GetUserResponse> GetUser(
    GetUserRequest request,
    CancellationToken cancellationToken = default
  );

  Task<CreateUserIfNotExistsResponse> CreateUserIfNotExists(
    CreateUserIfNotExistsRequest request,
    string? email,
    CancellationToken cancellationToken = default
  );

  Task<UpdateUserSlugResponse> UpdateUserSlug(
    UpdateUserSlugRequest request,
    string userId,
    CancellationToken cancellationToken = default
  );

  Task<GenerateRandomSlugResponse> GenerateRandomSlug(
    GenerateRandomSlugRequest request,
    CancellationToken cancellationToken = default
  );
}
