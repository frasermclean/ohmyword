using OhMyWord.Core.Models;

namespace OhMyWord.Api.Responses.Words;

public record WordResponse(string Value, PartOfSpeech PartOfSpeech, string Definition);
