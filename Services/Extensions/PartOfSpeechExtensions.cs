using OhMyWord.Core.Models;

namespace OhMyWord.Services.Extensions;

internal static class PartOfSpeechExtensions
{
    public static string ToPartitionKey(this PartOfSpeech partOfSpeech) =>
        partOfSpeech.ToString().ToLowerInvariant();
}
