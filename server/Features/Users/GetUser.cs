using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RSPWebAPI.Contracts;
using RSPWebAPI.Database;
using RSPWebAPI.Shared;

namespace RSPWebAPI.Features.Users;

public static class GetUser
{
    public class Query : IRequest<Result<GetUserResponse>>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, Result<GetUserResponse>>
    {
        private readonly ApplicationDbContext _dbContext;

        public Handler(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<GetUserResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userResponse = await _dbContext
                .Users
                .Where(user => user.UserId == request.Id)
                .Select(user => new GetUserResponse
                {
                    DiscordId = user.DiscordId,
                    Email = user.Email,
                    Name = user.Name,
                    ProfileImage = user.ProfileImage
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (userResponse is null)
            {
                return Result.Failure<GetUserResponse>(new Error(
                    "GetUser.Null",
                    "The user with the specified ID was not found"));
            }

            return userResponse;
        }
    }
}

public class GetUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetUser.Query { Id = id };

            var result = await sender.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound(new
                {
                    code = result.Error.Code,
                    message = result.Error.Message
                });
            }

            return Results.Ok(result.Value);
        });
    }
}
