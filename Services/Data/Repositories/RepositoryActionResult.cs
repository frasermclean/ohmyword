using Microsoft.Azure.Cosmos;
using OhMyWord.Core.Models;
using System.Net;

namespace OhMyWord.Services.Data.Repositories;

public class RepositoryActionResult<T> where T : Entity
{
    public RepositoryAction Action { get; init; }
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public T? Resource { get; init; }

    public static RepositoryActionResult<T> FromResponseMessage(
        ResponseMessage response,
        RepositoryAction action,
        string id)
    {
        var errorMessage = response.IsSuccessStatusCode
            ? string.Empty
            : response.StatusCode switch
            {
                HttpStatusCode.NotFound => $"Could not {action} a {typeof(T).Name} with ID: {id}",
                _ => $"Error occurred during {action} of {typeof(T).Name} with ID: {id}"
            };

        return new RepositoryActionResult<T>
        {
            Action = action,
            Success = response.IsSuccessStatusCode,
            StatusCode = (int) response.StatusCode,
            ErrorMessage = errorMessage,
            Resource = response.IsSuccessStatusCode && response.Content.CanRead
                ? EntitySerializer.ConvertFromStream<T>(response.Content)
                : null,
        };
    }
}
