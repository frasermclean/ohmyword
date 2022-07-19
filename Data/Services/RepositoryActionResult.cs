﻿using Microsoft.Azure.Cosmos;
using OhMyWord.Data.Models;

namespace OhMyWord.Data.Services;

public class RepositoryActionResult<T> where T : Entity
{
    public RepositoryAction Action { get; }
    public bool Success { get; }
    public int StatusCode { get; }
    public string Message { get; }
    public T? Resource { get; }

    public RepositoryActionResult(ResponseMessage response, RepositoryAction action, Guid id)
    {
        Action = action;
        Success = response.IsSuccessStatusCode;
        StatusCode = (int)response.StatusCode;

        Message = response.IsSuccessStatusCode
            ? string.Empty
            : $"Could not {action.ToString().ToLowerInvariant()} a {typeof(T).Name.ToLowerInvariant()} with ID: {id}.";

        Resource = response.IsSuccessStatusCode && response.Content is not null
            ? EntitySerializer.ConvertFromStream<T>(response.Content)
            : null;
    }
}
