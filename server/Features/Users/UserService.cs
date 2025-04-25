using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Auth0.ManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Clients.Interfaces;
using RSPWebAPI.Common;
using RSPWebAPI.Common.Cache;
using RSPWEBAPI.Common.Cache;
using RSPWebAPI.Common.Interfaces;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Constants;
using RSPWebAPI.Features.Users.Dtos;
using RSPWebAPI.Features.Users.Interfaces;
using RSPWebAPI.Shared.Strings;

namespace RSPWebAPI.Features.Users;

public class UserService : IUserService
{
  private readonly ILogger<UserService> _logger;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<UserEntity> _userRepository;
  private readonly IUserIdentityService _userIdentityService;
  private readonly IRequestCache _cache;

  public UserService(
    IRepository<UserEntity> userRepository,
    IUnitOfWork unitOfWork,
    ILogger<UserService> logger,
    IUserIdentityService userIdentityService,
    IRequestCache cache
  )
  {
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _logger = logger;
    _userIdentityService = userIdentityService;
    _cache = cache;
  }

  public async Task<IServiceResponse<AdminCreateUserResponse>> CreateAdminUser(
    AdminCreateUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingUser = await GetUserByEmailAsync(request.Email, cancellationToken);
    if (existingUser != null)
    {
      return new ErrorServiceResponse<AdminCreateUserResponse>(Message.UserEmailExists);
    }

    var slug = await createSlug(request.Name);
    var user = new UserEntity
    {
      UserId = Database.Constants.GeneratePrimaryKeyId(),
      DiscordId = request.DiscordId,
      Email = request.Email,
      Name = request.Name,
      ProfileImage = request.ProfileImage,
      IsAdmin = request.IsAdmin,
      Slug = slug,
    };

    try
    {
      await AddUserAsync(user, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminCreateUserResponse>(
        Message.UserCreatedSuccessfully,
        new AdminCreateUserResponse { UserId = user.UserId }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.UserCreationUnexpectedError);
      return new ErrorServiceResponse<AdminCreateUserResponse>(Message.UserCreationUnexpectedError);
    }
  }

  public async Task<IServiceResponse<AdminDeleteUserResponse>> DeleteAdminUser(
    AdminDeleteUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingUser = await GetUserByEmailAsync(request.Email, cancellationToken);
    if (existingUser == null)
    {
      return new ErrorServiceResponse<AdminDeleteUserResponse>(Message.UserEmailDoesNotExists);
    }

    try
    {
      await DeleteUserAsync(existingUser.UserId, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminDeleteUserResponse>(Message.UserDeletedSuccessfully);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.UserDeletionUnexpectedError);
      return new ErrorServiceResponse<AdminDeleteUserResponse>(Message.UserDeletionUnexpectedError);
    }
  }

  public async Task<IServiceResponse<AdminListUserResponse>> ListAdminUser(
    AdminListUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      var users = await GetAllUsersAsync(null, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminListUserResponse>(
        Message.UserListSuccessfully,
        new AdminListUserResponse { Users = users.ToList() }
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.UserListUnexpectedError);
      return new ErrorServiceResponse<AdminListUserResponse>(Message.UserListUnexpectedError);
    }
  }

  public async Task<IServiceResponse<AdminUpdateUserResponse>> UpdateAdminUser(
    AdminUpdateUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingUser = await GetUserByIdAsync(request.UserId, cancellationToken);
    if (existingUser == null)
    {
      return new ErrorServiceResponse<AdminUpdateUserResponse>(Message.UserIdDoesNotExists);
    }

    existingUser.DiscordId = request.DiscordId;
    existingUser.Name = request.Name;
    existingUser.ProfileImage = request.ProfileImage;
    existingUser.IsAdmin = request.IsAdmin;

    try
    {
      await UpdateUserAsync(existingUser, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new SuccessServiceResponse<AdminUpdateUserResponse>(Message.UserUpdatedSuccessfully);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.UserUpdateUnexpectedError);
      return new ErrorServiceResponse<AdminUpdateUserResponse>(Message.UserUpdateUnexpectedError);
    }
  }

  public async Task<IServiceResponse<GetCurrentUserResponse>> GetCurrentUser(
    string? email,
    CancellationToken cancellationToken = default
  )
  {
    var user = await GetUserByEmailAsync(email, cancellationToken);
    if (user == null)
    {
      return new ErrorServiceResponse<GetCurrentUserResponse>(Message.UserEmailDoesNotExists);
    }

    return new SuccessServiceResponse<GetCurrentUserResponse>(
      Message.UserEmailExists,
      new GetCurrentUserResponse { User = user }
    );
  }

  public async Task<IServiceResponse<GetUserResponse>> GetUserBySlug(
    GetUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var user = await GetUserByEmailAsync(request.Email, cancellationToken);
    if (user == null)
    {
      return new ErrorServiceResponse<GetUserResponse>(Message.UserEmailDoesNotExists);
    }

    return new SuccessServiceResponse<GetUserResponse>(
      Message.GetUserSuccessfully,
      new GetUserResponse { User = user }
    );
  }

  public async Task<IServiceResponse<GetUserResponse>> GetUser(
    GetUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    UserEntity? user = null;

    if (request.Slug != null)
    {
      user = await GetUserBySlugAsync(request.Slug, cancellationToken);
    }
    else if (request.Email != null)
    {
      user = await GetUserByEmailAsync(request.Email, cancellationToken);
    }

    if (user == null)
    {
      return new ErrorServiceResponse<GetUserResponse>(Message.UserEmailDoesNotExists);
    }

    return new SuccessServiceResponse<GetUserResponse>(
      Message.GetUserSuccessfully,
      new GetUserResponse { User = user }
    );
  }

  public async Task<IServiceResponse<CreateUserIfNotExistsResponse>> CreateUserIfNotExists(
    CreateUserIfNotExistsRequest request,
    string? email,
    CancellationToken cancellationToken = default
  )
  {
    await _unitOfWork.BeginTransactionAsync(cancellationToken);

    try
    {
      var auth0User = await _userIdentityService.GetUserByEmailAsync(email!, cancellationToken);
      var existingUserResponse = await GetCurrentUser(email, cancellationToken);

      var isVerified = auth0User?.EmailVerified == true;
      var userExists = existingUserResponse.IsSuccess;

      // Do nothing since user exists and verified
      if (userExists && isVerified)
      {
        return new SuccessServiceResponse<CreateUserIfNotExistsResponse>(
          existingUserResponse.Message
        );
      }

      // Create user in database if it doesn't exists
      if (!userExists)
      {
        var slug = await createSlug(request.Name);
        var user = new UserEntity
        {
          UserId = Database.Constants.GeneratePrimaryKeyId(),
          Email = email!, // null email would have been captured earlier in GetCurrentUser
          Name = request.Name,
          Slug = slug,
          IsAdmin = false,
        };

        await AddUserAsync(user, cancellationToken);
      }

      if (!isVerified && auth0User?.UserId != null)
      {
        await _userIdentityService.SendVerificationEmailAsync(auth0User.UserId);
      }

      await _unitOfWork.SaveChangesAsync(cancellationToken);
      await _unitOfWork.CommitTransactionAsync(cancellationToken);

      return new SuccessServiceResponse<CreateUserIfNotExistsResponse>(
        Message.UserCreatedSuccessfully,
        new CreateUserIfNotExistsResponse()
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Message.UserCreationUnexpectedError);
      await _unitOfWork.RollbackTransactionAsync(cancellationToken);
      return new ErrorServiceResponse<CreateUserIfNotExistsResponse>(
        Message.UserCreationUnexpectedError
      );
    }
  }

  private async Task<string> createSlug(string name)
  {
    string baseSlug = StringUtils.Slugify(name);
    string slug = baseSlug;
    var random = new Random();

    // Keep adding a suffix until it sticks
    while (await GetUserBySlugAsync(slug) is not null)
    {
      int randomNumber = random.Next(1, 10001);
      slug = $"{baseSlug}-{randomNumber}";
    }

    return slug;
  }

  #region CRUD Operations

  public async Task AddUserAsync(UserEntity user, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(user);

    await _userRepository.AddAsync(user, cancellationToken);
  }

  public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
  {
    var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

    if (user == null)
    {
      throw new KeyNotFoundException(Message.UserIdDoesNotExists);
    }

    _userRepository.Delete(user, cancellationToken);
  }

  public async Task UpdateUserAsync(UserEntity user, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(user);

    var existingUser = await _userRepository.GetByIdAsync(user.UserId, cancellationToken);
    if (existingUser == null)
    {
      throw new KeyNotFoundException(Message.UserIdDoesNotExists);
    }

    _userRepository.Update(user, cancellationToken);
  }

  public async Task<IEnumerable<UserEntity>> GetAllUsersAsync(
    Expression<Func<UserEntity, bool>>? predicate = null,
    CancellationToken cancellationToken = default,
    Func<IQueryable<UserEntity>, IQueryable<UserEntity>>? include = null
  )
  {
    return await _userRepository.GetAllAsync(predicate, cancellationToken, include);
  }

  public async Task<UserEntity?> GetUserByIdAsync(
    string userId,
    CancellationToken cancellationToken = default,
    Func<IQueryable<UserEntity>, IQueryable<UserEntity>>? include = null
  )
  {
    return await _userRepository.GetByIdAsync(userId, cancellationToken, include);
  }

  public async Task<UserEntity?> GetUserByEmailAsync(
    string? email,
    CancellationToken cancellationToken = default,
    Func<IQueryable<UserEntity>, IQueryable<UserEntity>>? include = null
  )
  {
    return await _cache.GetOrCreateAsync(
      routeKey: RouteCacheKeys.GetUserByEmail,
      primaryKey: email,
      factory: async () =>
      {
        return await _userRepository.FirstOrDefaultAsync(
          q => q.Email == email,
          cancellationToken,
          include
        );
      },
      ttl: TimeSpan.FromHours(1)
    );
  }

  public async Task<UserEntity?> GetUserBySlugAsync(
    string? slug,
    CancellationToken cancellationToken = default,
    Func<IQueryable<UserEntity>, IQueryable<UserEntity>>? include = null
  )
  {
    return await _cache.GetOrCreateAsync(
      routeKey: RouteCacheKeys.GetUserBySlug,
      primaryKey: slug,
      factory: async () =>
      {
        return await _userRepository.FirstOrDefaultAsync(
          q => q.Slug == slug,
          cancellationToken,
          include
        );
      },
      ttl: TimeSpan.FromHours(1)
    );
  }

  #endregion
}
