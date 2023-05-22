using OneOf;

namespace OhMyWord.Domain.Results;

public class CreateResult<T> : OneOfBase<T, Conflict>
{
    private CreateResult(OneOf<T, Conflict> oneOf) : base(oneOf) { }

    public static implicit operator CreateResult<T>(T _) => new(_);
    public static implicit operator CreateResult<T>(Conflict _) => new(_);
}
