using Carter;
using FluentValidation;
using MediatR;
using RSPWebAPI.Contracts;
using RSPWebAPI.Database;
using RSPWebAPI.Entities;
using RSPWebAPI.Features.Users;
using RSPWebAPI.Shared;
using RSPWebAPI.Shared.Behaviours;

namespace RSPWebAPI.Features.Users;

public static class CreateUser
{
    public class Command : AuthRequest<Result<CreateUserResponse>>
    {
        public string DiscordId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        
        public string ProfileImage { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.DiscordId).NotEmpty();
            RuleFor(c => c.Email).NotEmpty();
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<CreateUserResponse>>
    {
        private readonly ApplicationDbContext _dbContext;

        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<CreateUserResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                DiscordId = request.DiscordId,
                Email = request.Email,
                Name = request.Name,
            };

            _dbContext.Add(user);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CreateUserResponse
            {
                UserId = user.UserId,
                DiscordId = user.DiscordId,
                Email = user.Email,
                Name = user.Name,
                ProfileImage = user.ProfileImage
            };
        }
    }
}

public class CreateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users", async (CreateUserRequest request, ISender sender) =>
        {
            var command = new CreateUser.Command()
            {
                DiscordId = request.DiscordId,
                Email = request.Email,
                Name = request.Name,
                ProfileImage = request.ProfileImage
            };

            var result = await sender.Send(command);

            if (result.IsFailure)
            {
                return Results.NotFound(new
                {
                    code = result.Error.Code,
                    message = result.Error.Message
                });
            }

            return Results.Ok(result.Value);
        })
        .WithName("CreateUser")
        .WithMetadata("CreateUser Metadata");
    }
}
