namespace OhMyWord.Core.Responses.Words;

public class GetWordsResponse
{
    public int Offset { get; init; }
    public int Limit { get; init; }    
    public int Total { get; init; }
    public IEnumerable<WordResponse> Words { get; init; } = Enumerable.Empty<WordResponse>();
}
