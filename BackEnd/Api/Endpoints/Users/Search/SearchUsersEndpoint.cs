using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Endpoints.Users.Search;

public class SearchUsersEndpoint : Endpoint<SearchUsersRequest, IEnumerable<User>>
{
    private readonly IUsersRepository usersRepository;

    public SearchUsersEndpoint(IUsersRepository usersRepository)
    {
        this.usersRepository = usersRepository;
    }

    public override void Configure()
    {
        Get("/users");
    }

    public override async Task HandleAsync(SearchUsersRequest request, CancellationToken cancellationToken)
    {
        var users = await usersRepository
            .SearchUsers(request.Filter, cancellationToken)
            .ToListAsync(cancellationToken);

        await SendOkAsync(users, cancellationToken);
    }
}
