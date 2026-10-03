using System.Collections;
namespace ThaiReference.Utilities;

public sealed class CachedEnumerable<T>(IEnumerator<T> enumerator) : IEnumerable<T>, IDisposable
{
    private readonly IEnumerator<T> enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
    private readonly List<T> cache = [];

    public CachedEnumerable(IEnumerable<T> enumerable) : this(enumerable.GetEnumerator()) { }

    public IEnumerator<T> GetEnumerator()
    {
        int index = 0;
        while (true)
        {
            if (index < cache.Count)
            {
                yield return cache[index];
                index++;
            }
            else if (enumerator.MoveNext())
                cache.Add(enumerator.Current);
            else
                yield break;
        }
    }

    public void Dispose() => enumerator.Dispose();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
}

