using Microsoft.Azure.Cosmos;
using System.Runtime.CompilerServices;

namespace OhMyWord.Data.Extensions;

public static class FeedIteratorExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this FeedIterator<T> iterator,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (iterator.HasMoreResults && !cancellationToken.IsCancellationRequested)
        {
            foreach (var item in await iterator.ReadNextAsync(cancellationToken))
            {
                yield return item;
            }
        }
    }
}
