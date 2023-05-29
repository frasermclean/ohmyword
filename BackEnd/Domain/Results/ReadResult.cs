using OneOf;
using OneOf.Types;

namespace OhMyWord.Domain.Results;

public class ReadResult<T> : OneOfBase<T, NotFound>
{
    internal ReadResult(OneOf<T, NotFound> oneOf) : base(oneOf) { }

    public static implicit operator ReadResult<T>(T t) => new(t);
    public static implicit operator ReadResult<T>(NotFound notFound) => new(notFound);
}
