using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Endpoints.Users.Get;

public class GetUserEndpoint : Endpoint<GetUserRequest, User?>
{
    private readonly IUserService userService;

    public GetUserEndpoint(IUserService userService)
    {
        this.userService = userService;
    }

    public override void Configure()
    {
        Get("/users/{userId}");
    }

    public override async Task HandleAsync(GetUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.GetUserAsync(request.UserId, cancellationToken);

        var responseTask = user is not null
            ? SendOkAsync(user, cancellationToken)
            : SendNotFoundAsync(cancellationToken);

        await responseTask;
    }
}
