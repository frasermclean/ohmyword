using AutoFixture;
using OhMyWord.Core.Models;

namespace OhMyWord.Data.CosmosDb.Tests.Fixtures;

/// <summary>
/// AutoFixture customization for <see cref="Word"/>. This customization sets the <see cref="Word.LastModifiedTime"/>
/// </summary>
public class WordCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).UtcDateTime;
        fixture.Customize<Word>(
            composer => composer.With(word => word.LastModifiedTime, now));
    }
}
