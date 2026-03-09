namespace FSH.Playground.Blazor.Services.Api.Expendable;

/// <summary>
/// Thread-safe activity feed for tracking Expendable module operations.
/// Uses Queue for O(1) insert/remove instead of List.Insert which is O(n).
/// </summary>
internal sealed class ExpendableActivityFeed : IExpendableActivityFeed
{
    private const int MaxEntries = 100;
    private readonly object _lock = new();
    private readonly Queue<ExpendableActivityEntry> _entries = new(MaxEntries);

    public IReadOnlyList<ExpendableActivityEntry> GetRecent(int count = 15, string? category = null)
    {
        count = Math.Clamp(count, 1, MaxEntries);

        lock (_lock)
        {
            IEnumerable<ExpendableActivityEntry> query = _entries;
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase));
            }

            return query.Take(count).ToList();
        }
    }

    public void AddSuccess(string category, string action)
    {
        Add(new ExpendableActivityEntry(DateTimeOffset.Now, category, action, true));
    }

    public void AddFailure(string category, string action, string error)
    {
        Add(new ExpendableActivityEntry(DateTimeOffset.Now, category, $"{action} failed: {error}", false));
    }

    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }

    private void Add(ExpendableActivityEntry entry)
    {
        lock (_lock)
        {
            // Convert to list to prepend (queue doesn't support front insertion)
            var temp = new List<ExpendableActivityEntry> { entry };
            temp.AddRange(_entries);

            _entries.Clear();
            foreach (var item in temp)
            {
                _entries.Enqueue(item);

                // Remove oldest if exceeding max
                if (_entries.Count > MaxEntries)
                {
                    _entries.Dequeue();
                }
            }
        }
    }
}
