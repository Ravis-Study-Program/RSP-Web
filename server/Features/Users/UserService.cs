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

  public async Task<AdminCreateUserResponse> CreateAdminUser(
    AdminCreateUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingUser = await GetUserByEmailAsync(request.Email, cancellationToken);
    if (existingUser != null)
    {
      throw new InvalidOperationException(Messages.User.EmailExists);
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
      return new AdminCreateUserResponse { UserId = user.UserId };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.User.CreationError);
      throw new InvalidOperationException(Messages.User.CreationError);
    }
  }

  public async Task<AdminDeleteUserResponse> DeleteAdminUser(
    AdminDeleteUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingUser = await GetUserByEmailAsync(request.Email, cancellationToken);
    if (existingUser == null)
    {
      throw new KeyNotFoundException(Messages.User.EmailDoesNotExist);
    }

    try
    {
      await DeleteUserAsync(existingUser.UserId, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new AdminDeleteUserResponse();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.User.DeletionError);
      throw new InvalidOperationException(Messages.User.DeletionError);
    }
  }

  public async Task<AdminListUserResponse> ListAdminUser(
    AdminListUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    try
    {
      var users = await GetAllUsersAsync(null, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new AdminListUserResponse { Users = users.ToList() };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.User.ListError);
      throw new InvalidOperationException(Messages.User.ListError);
    }
  }

  public async Task<AdminUpdateUserResponse> UpdateAdminUser(
    AdminUpdateUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingUser = await GetUserByIdAsync(request.UserId, cancellationToken);
    if (existingUser == null)
    {
      throw new KeyNotFoundException(Messages.User.IdDoesNotExist);
    }

    existingUser.DiscordId = request.DiscordId;
    existingUser.Name = request.Name;
    existingUser.Email = request.Email;
    existingUser.ProfileImage = request.ProfileImage;
    existingUser.IsAdmin = request.IsAdmin;

    try
    {
      await UpdateUserAsync(existingUser, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      return new AdminUpdateUserResponse();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.User.UpdateError);
      throw new InvalidOperationException(Messages.User.UpdateError);
    }
  }

  public async Task<GetCurrentUserResponse> GetCurrentUser(
    string? email,
    CancellationToken cancellationToken = default
  )
  {
    var user = await GetUserByEmailAsync(email, cancellationToken);
    if (user == null)
    {
      throw new KeyNotFoundException(Messages.User.EmailDoesNotExist);
    }

    return new GetCurrentUserResponse { User = user };
  }

  public async Task<GetUserResponse> GetUserBySlug(
    GetUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var user = await GetUserByEmailAsync(request.Email, cancellationToken);
    if (user == null)
    {
      throw new KeyNotFoundException(Messages.User.EmailDoesNotExist);
    }

    return new GetUserResponse { User = user };
  }

  public async Task<GetUserResponse> GetUser(
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
      throw new KeyNotFoundException(Messages.User.EmailDoesNotExist);
    }

    return new GetUserResponse { User = user };
  }

  public async Task<CreateUserIfNotExistsResponse> CreateUserIfNotExists(
    CreateUserIfNotExistsRequest request,
    string? email,
    CancellationToken cancellationToken = default
  )
  {
    await _unitOfWork.BeginTransactionAsync(cancellationToken);

    try
    {
      var auth0Users =
        (await _userIdentityService.GetUserByEmailAsync(email!, cancellationToken)) ?? [];
      var auth0User = auth0Users.First();
      if (auth0User == null)
      {
        throw new Exception("Couldn't find auth0 user");
      }

      var isVerified = auth0User?.EmailVerified == true;
      UserEntity? existingUser = null;
      var userExists = false;

      try
      {
        existingUser = await GetUserByEmailAsync(email, cancellationToken);
        userExists = existingUser != null;
      }
      catch (KeyNotFoundException)
      {
        userExists = false;
      }

      if (userExists && isVerified)
      {
        // Perform linking if it's another synonymous account
        if (auth0Users.Count >= 2)
        {
          var primaryUser = auth0Users
            .OrderBy(u => int.TryParse(u.LoginsCount, out var count) ? count : 0)
            .First();

          foreach (var user in auth0Users)
          {
            if (user.UserId != primaryUser.UserId)
            {
              await _userIdentityService.LinkAccountAsync(primaryUser.UserId, user);
            }
          }
        }

        return new CreateUserIfNotExistsResponse();
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

      // Send verification email if user is not verified
      if (!isVerified && auth0User?.UserId != null)
      {
        await _userIdentityService.SendVerificationEmailAsync(auth0User.UserId);
      }

      await _unitOfWork.SaveChangesAsync(cancellationToken);
      await _unitOfWork.CommitTransactionAsync(cancellationToken);

      return new CreateUserIfNotExistsResponse();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, Messages.User.CreationError);
      await _unitOfWork.RollbackTransactionAsync(cancellationToken);
      throw new InvalidOperationException(Messages.User.CreationError);
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
      throw new KeyNotFoundException(Messages.User.IdDoesNotExist);
    }

    _userRepository.Delete(user, cancellationToken);
  }

  public async Task UpdateUserAsync(UserEntity user, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(user);

    var existingUser = await _userRepository.GetByIdAsync(user.UserId, cancellationToken);
    if (existingUser == null)
    {
      throw new KeyNotFoundException(Messages.User.IdDoesNotExist);
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
