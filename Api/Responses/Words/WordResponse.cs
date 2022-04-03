using OhMyWord.Data.Models;

namespace OhMyWord.Api.Responses.Words;

public record WordResponse(string Id, PartOfSpeech PartOfSpeech, string Definition);
