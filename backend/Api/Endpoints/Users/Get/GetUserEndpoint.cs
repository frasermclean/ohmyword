using Microsoft.AspNetCore.Http.HttpResults;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace OhMyWord.Api.Endpoints.Users.Get;

[HttpGet("/users/{userId}")]
public class GetUserEndpoint : Endpoint<GetUserRequest, Results<Ok<User>, NotFound>>
{
    private readonly IUsersRepository usersRepository;

    public GetUserEndpoint(IUsersRepository usersRepository)
    {
        this.usersRepository = usersRepository;
    }

    public override async Task<Results<Ok<User>, NotFound>> ExecuteAsync(GetUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await usersRepository.GetUserAsync(request.UserId, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound();
    }
}
