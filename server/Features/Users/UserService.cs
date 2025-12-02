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
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Strings;

namespace RSPWebAPI.Features.Users;

public class UserService : BaseService, IUserService
{
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
    : base(unitOfWork, logger)
  {
    _userRepository = userRepository;
    _userIdentityService = userIdentityService;
    _cache = cache;
  }

  public async Task<AdminCreateUserResponse> CreateAdminUser(
    AdminCreateUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingUser = await GetUserAsync(
      email: request.Email,
      cancellationToken: cancellationToken
    );
    if (existingUser != null)
    {
      throw new InvalidOperationException(Messages.User.EmailExists);
    }

    var slug = await createSlug();
    var user = new UserEntity
    {
      UserId = Database.Constants.GeneratePrimaryKeyId(),
      DiscordId = request.DiscordId,
      Email = request.Email,
      Name = request.Name,
      ProfileImage = request.ProfileImage,
      IsAdmin = request.IsAdmin,
      IsTestUser = request.IsTestUser,
      Slug = slug,
    };

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await AddUserAsync(user, cancellationToken);
        return new AdminCreateUserResponse { UserId = user.UserId };
      },
      Messages.User.CreationError,
      cancellationToken
    );
  }

  public async Task<AdminDeleteUserResponse> DeleteAdminUser(
    AdminDeleteUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingUser = await GetUserAsync(
      userId: request.UserId,
      cancellationToken: cancellationToken
    );
    if (existingUser == null)
    {
      throw new KeyNotFoundException(Messages.User.IdDoesNotExist);
    }

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await DeleteUserAsync(existingUser.UserId, cancellationToken);
        return new AdminDeleteUserResponse();
      },
      Messages.User.DeletionError,
      cancellationToken
    );
  }

  public async Task<AdminListUserResponse> ListAdminUser(
    AdminListUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    return await ExecuteWithSaveAsync(
      async () =>
      {
        var users = await _userRepository.Table.AsNoTracking().IgnoreQueryFilters().Where(x => x.DeletedAtUtc.HasValue == false).ToListAsync(cancellationToken);
        var adminUsers = users
          .Select(u => new AdminUserDto
          {
            UserId = u.UserId,
            DiscordId = u.DiscordId,
            Email = u.Email,
            IsAdmin = u.IsAdmin,
            IsTestUser = u.IsTestUser,
            Name = u.Name,
            Slug = u.Slug,
            ProfileImage = u.ProfileImage,
            DeletedAtUtc = u.DeletedAtUtc,
          })
          .ToList();
        return new AdminListUserResponse { Users = adminUsers };
      },
      Messages.User.ListError,
      cancellationToken
    );
  }

  public async Task<AdminUpdateUserResponse> UpdateAdminUser(
    AdminUpdateUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var existingUser = await GetUserAsync(
      userId: request.UserId,
      bypassCache: true,
      cancellationToken: cancellationToken
    );
    if (existingUser == null)
    {
      throw new KeyNotFoundException(Messages.User.IdDoesNotExist);
    }

    existingUser.DiscordId = request.DiscordId;
    existingUser.Name = request.Name;
    existingUser.Email = request.Email;
    existingUser.ProfileImage = request.ProfileImage;
    existingUser.IsAdmin = request.IsAdmin;
    existingUser.IsTestUser = request.IsTestUser;

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await UpdateUserAsync(existingUser, cancellationToken);
        return new AdminUpdateUserResponse();
      },
      Messages.User.UpdateError,
      cancellationToken
    );
  }

  public async Task<GetCurrentUserResponse> GetCurrentUser(
    string? userId,
    CancellationToken cancellationToken = default
  )
  {
    var user = await GetUserAsync(userId: userId, cancellationToken: cancellationToken);
    if (user == null)
    {
      throw new KeyNotFoundException(Messages.User.IdDoesNotExist);
    }

    return new GetCurrentUserResponse { User = user };
  }

  public async Task<GetUserResponse> GetUserBySlug(
    GetUserRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var user = await GetUserAsync(userId: request.UserId!, cancellationToken: cancellationToken);
    if (user == null)
    {
      throw new KeyNotFoundException(Messages.User.IdDoesNotExist);
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
      user = await GetUserAsync(slug: request.Slug, cancellationToken: cancellationToken);
    }
    else if (request.UserId != null)
    {
      user = await GetUserAsync(userId: request.UserId, cancellationToken: cancellationToken);
    }

    if (user == null)
    {
      throw new KeyNotFoundException(Messages.User.IdDoesNotExist);
    }

    return new GetUserResponse { User = user };
  }

  public async Task<CreateUserIfNotExistsResponse> CreateUserIfNotExists(
    CreateUserIfNotExistsRequest request,
    string? email,
    CancellationToken cancellationToken = default
  )
  {
    return await ExecuteWithTransactionAsync(
      async () =>
      {
        // Get Auth0 user data
        var auth0Users =
          (await _userIdentityService.GetUserByEmailAsync(email!, cancellationToken)) ?? [];
        var auth0User = auth0Users.First();
        if (auth0User == null)
        {
          throw new Exception("Couldn't find auth0 user");
        }

        var isVerified = auth0User.EmailVerified == true;

        // Check if user exists in database
        var existingUser = await GetUserByEmailOrNull(email, cancellationToken);
        var userExists = existingUser != null;
        var userId = existingUser?.UserId;

        // Handle account linking for existing verified users
        if (userExists && isVerified)
        {
          await LinkMultipleAuth0Accounts(auth0Users);
        }

        if (!userExists)
        {
          userId = await CreateNewUser(request, email!, cancellationToken);
        }

        if (!isVerified)
        {
          await _userIdentityService.SendVerificationEmailAsync(auth0User.UserId);
        }

        await _userIdentityService.AddMetadata(
          auth0User.UserId,
          new
          {
            isAdmin = existingUser?.IsAdmin ?? false,
            userId = existingUser?.UserId ?? "Unknown User",
            userSlug = existingUser?.Slug ?? "Unknown Slug",
          }
        );

        return new CreateUserIfNotExistsResponse() { UserId = userId ?? "" };
      },
      Messages.User.CreationError,
      cancellationToken
    );
  }

  public async Task<UpdateUserSlugResponse> UpdateUserSlug(
    UpdateUserSlugRequest request,
    string userId,
    CancellationToken cancellationToken = default
  )
  {
    var existingUser = await GetUserAsync(
      userId: userId,
      bypassCache: true,
      cancellationToken: cancellationToken
    );
    if (existingUser == null)
    {
      throw new KeyNotFoundException(Messages.User.IdDoesNotExist);
    }

    // Check if the new slug is already taken by another user
    var existingSlugUser = await GetUserBySlugAsync(
      slug: request.Slug,
      bypassCache: true,
      cancellationToken: cancellationToken
    );
    if (existingSlugUser != null && existingSlugUser.UserId != userId)
    {
      throw new InvalidOperationException(SlugConstants.Messages.SlugAlreadyTaken);
    }

    // Store the old slug before updating
    var oldSlug = existingUser.Slug;
    existingUser.Slug = request.Slug;

    return await ExecuteWithSaveAsync(
      async () =>
      {
        await UpdateUserAsync(existingUser, cancellationToken);
        
        // Evict user cache entries since slug has been updated
        _cache.Remove(RouteCacheKeys.GetUserByUserId, userId);
        _cache.Remove(RouteCacheKeys.GetUserByEmail, existingUser.Email);
        _cache.Remove(RouteCacheKeys.GetUserBySlug, request.Slug);
        
        // Also remove the old slug from cache if it was different
        if (!string.IsNullOrEmpty(oldSlug) && oldSlug != request.Slug)
        {
          _cache.Remove(RouteCacheKeys.GetUserBySlug, oldSlug);
        }
        
        return new UpdateUserSlugResponse();
      },
      Messages.User.UpdateError,
      cancellationToken
    );
  }

  public async Task<GenerateRandomSlugResponse> GenerateRandomSlug(
    GenerateRandomSlugRequest request,
    CancellationToken cancellationToken = default
  )
  {
    return await ExecuteWithSaveAsync(
      async () =>
      {
        var randomSlug = await createSlug();
        return new GenerateRandomSlugResponse { Slug = randomSlug };
      },
      SlugConstants.Messages.FailedToGenerateRandomSlug,
      cancellationToken
    );
  }

  private async Task<UserEntity?> GetUserByEmailOrNull(
    string? email,
    CancellationToken cancellationToken
  )
  {
    try
    {
      return await GetUserAsync(email: email, cancellationToken: cancellationToken);
    }
    catch (KeyNotFoundException)
    {
      return null;
    }
  }

  private async Task LinkMultipleAuth0Accounts(IList<User> auth0Users)
  {
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
  }

  private async Task<string> CreateNewUser(
    CreateUserIfNotExistsRequest request,
    string email,
    CancellationToken cancellationToken
  )
  {
    var slug = await createSlug();
    var userId = Database.Constants.GeneratePrimaryKeyId();
    var user = new UserEntity
    {
      UserId = userId,
      Email = email,
      Name = request.Name,
      Slug = slug,
      IsAdmin = false,
      IsTestUser = false,
    };

    await AddUserAsync(user, cancellationToken);
    return userId;
  }

  private async Task<string> createSlug()
  {
    const int maxAttemptsWithSameBase = 50;
    const int minRandomNumber = 100;
    const int maxRandomNumber = 9999;
    
    var random = new Random();
    string baseSlug = StringUtils.Slugify();
    int attemptCount = 0;

    while (true)
    {
      var randomNumber = random.Next(minRandomNumber, maxRandomNumber);
      var slug = $"{baseSlug}-{randomNumber}";
      
      if (await GetUserAsync(slug: slug, bypassCache: true) is null)
      {
        return slug;
      }

      attemptCount++;
      if (attemptCount >= maxAttemptsWithSameBase)
      {
        baseSlug = StringUtils.Slugify();
        attemptCount = 0;
      }
    }
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

  private async Task<UserEntity?> GetUserByUserIdAsync(
    string userId,
    Func<IQueryable<UserEntity>, IQueryable<UserEntity>>? include = null,
    bool bypassCache = false,
    CancellationToken cancellationToken = default
  )
  {
    if (bypassCache)
    {
      return await _userRepository.GetByIdAsync(userId, cancellationToken, include);
    }

    return await _cache.GetOrCreateAsync(
      routeKey: RouteCacheKeys.GetUserByUserId,
      primaryKey: userId,
      factory: async () =>
      {
        return await _userRepository.GetByIdAsync(userId, cancellationToken, include);
      },
      ttl: TimeSpan.FromHours(1)
    );
  }

  private async Task<UserEntity?> GetUserByEmailAsync(
    string? email,
    Func<IQueryable<UserEntity>, IQueryable<UserEntity>>? include = null,
    bool bypassCache = false,
    CancellationToken cancellationToken = default
  )
  {
    if (bypassCache)
    {
      return await _userRepository.FirstOrDefaultAsync(
        q => q.Email == email,
        cancellationToken,
        include
      );
    }

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

  private async Task<UserEntity?> GetUserBySlugAsync(
    string? slug,
    Func<IQueryable<UserEntity>, IQueryable<UserEntity>>? include = null,
    bool bypassCache = false,
    CancellationToken cancellationToken = default
  )
  {
    if (bypassCache)
    {
      return await _userRepository.FirstOrDefaultAsync(
        q => q.Slug == slug,
        cancellationToken,
        include
      );
    }

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

  public async Task<UserEntity?> GetUserAsync(
    string? userId = null,
    string? email = null,
    string? slug = null,
    Func<IQueryable<UserEntity>, IQueryable<UserEntity>>? include = null,
    bool bypassCache = false,
    CancellationToken cancellationToken = default
  )
  {
    if (!string.IsNullOrEmpty(userId))
    {
      return await GetUserByUserIdAsync(userId, include, bypassCache, cancellationToken);
    }

    if (!string.IsNullOrEmpty(email))
    {
      return await GetUserByEmailAsync(email, include, bypassCache, cancellationToken);
    }

    if (!string.IsNullOrEmpty(slug))
    {
      return await GetUserBySlugAsync(slug, include, bypassCache, cancellationToken);
    }

    throw new ArgumentException(
      "At least one identifier (userId, email, or slug) must be provided."
    );
  }

  #endregion
}
