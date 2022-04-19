using OhMyWord.Core.Models;
using System.Net;

namespace OhMyWord.Services.Data.Repositories;

public class RepositoryActionResult<T> where T : Entity
{
    public RepositoryAction Action { get; init; }
    public bool Success { get; init; }
    public HttpStatusCode StatusCode { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public T? Resource { get; init; }
}
