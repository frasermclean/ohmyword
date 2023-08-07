using OhMyWord.Core.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Endpoints.Users.Search;

public class SearchUsersEndpoint : Endpoint<SearchUsersRequest, IEnumerable<User>>
{
    private readonly IUsersService usersService;

    public SearchUsersEndpoint(IUsersService usersService)
    {
        this.usersService = usersService;
    }

    public override void Configure()
    {
        Get("/users");
    }

    public override async Task HandleAsync(SearchUsersRequest request, CancellationToken cancellationToken)
    {
        var users = await usersService
            .SearchUsers(request.Filter, cancellationToken)
            .ToListAsync(cancellationToken);

        await SendOkAsync(users, cancellationToken);
    }
}
