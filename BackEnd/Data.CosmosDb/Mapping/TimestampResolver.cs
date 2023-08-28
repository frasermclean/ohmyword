using AutoMapper;
using OhMyWord.Core.Models;
using OhMyWord.Data.CosmosDb.Models;

namespace OhMyWord.Data.CosmosDb.Mapping;

public sealed class TimestampResolver : IValueResolver<Entity, Item, long>
{
    public long Resolve(Entity source, Item destination, long destMember, ResolutionContext context)
    {
        var dateTimeOffset = new DateTimeOffset(source.LastModifiedTime.ToUniversalTime());
        return dateTimeOffset.ToUnixTimeSeconds();
    }
}
