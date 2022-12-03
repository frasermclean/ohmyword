using OhMyWord.Data.Models;

namespace OhMyWord.Data.Extensions;

public static class PartOfSpeechExtensions
{
    public static string ToPartitionKey(this PartOfSpeech partOfSpeech) =>
        partOfSpeech.ToString().ToLowerInvariant();
}
