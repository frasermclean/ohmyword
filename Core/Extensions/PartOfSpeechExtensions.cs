using OhMyWord.Core.Models;

namespace OhMyWord.Core.Extensions;

public static class PartOfSpeechExtensions
{
    public static string ToPartitionKey(this PartOfSpeech partOfSpeech) =>
        partOfSpeech.ToString().ToLowerInvariant();
}
