namespace OhMyWord.Core.Services;

public interface IGraphApiClient
{
    Task<(string? Firstname, string? Lastname, string? DisplayName)> GetUserDetailsAsync(Guid userId,
        CancellationToken cancellationToken = default);
}
