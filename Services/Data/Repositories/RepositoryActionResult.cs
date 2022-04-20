using Microsoft.Azure.Cosmos;
using OhMyWord.Core.Models;

namespace OhMyWord.Services.Data.Repositories;

public class RepositoryActionResult<T> where T : Entity
{
    public RepositoryAction Action { get; init; }
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Resource { get; init; }

    public static RepositoryActionResult<T> FromResponseMessage(
        ResponseMessage response,
        RepositoryAction action,
        string id) => new()
        {
            Action = action,
            Success = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Message = response.IsSuccessStatusCode
                ? string.Empty
                : $"Could not {action.ToString().ToLowerInvariant()} a {typeof(T).Name.ToLowerInvariant()} with ID: {id}.",
            Resource = response.IsSuccessStatusCode && response.Content is not null
                ? EntitySerializer.ConvertFromStream<T>(response.Content)
                : null,
        };
}
