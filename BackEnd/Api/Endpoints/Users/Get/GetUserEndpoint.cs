using OhMyWord.Core.Models;
using OhMyWord.Core.Services;

namespace OhMyWord.Api.Endpoints.Users.Get;

public class GetUserEndpoint : Endpoint<GetUserRequest, User?>
{
    private readonly IUsersRepository usersRepository;

    public GetUserEndpoint(IUsersRepository usersRepository)
    {
        this.usersRepository = usersRepository;
    }

    public override void Configure()
    {
        Get("/users/{userId}");
    }

    public override async Task HandleAsync(GetUserRequest request, CancellationToken cancellationToken)
    {
        var user = await usersRepository.GetUserAsync(request.UserId, cancellationToken);

        var responseTask = user is not null
            ? SendOkAsync(user, cancellationToken)
            : SendNotFoundAsync(cancellationToken);

        await responseTask;
    }
}
