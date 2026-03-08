namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal sealed class ExpendableActivityFeed : IExpendableActivityFeed
{
    private const int MaxEntries = 100;
    private readonly List<ExpendableActivityEntry> _entries = new();

    public IReadOnlyList<ExpendableActivityEntry> GetRecent(int count = 15, string? category = null)
    {
        count = Math.Clamp(count, 1, MaxEntries);

        IEnumerable<ExpendableActivityEntry> query = _entries;
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => string.Equals(x.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        return query.Take(count).ToList();
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
        _entries.Clear();
    }

    private void Add(ExpendableActivityEntry entry)
    {
        _entries.Insert(0, entry);
        if (_entries.Count > MaxEntries)
        {
            _entries.RemoveAt(_entries.Count - 1);
        }
    }
}
