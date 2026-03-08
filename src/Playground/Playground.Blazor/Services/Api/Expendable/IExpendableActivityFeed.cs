namespace FSH.Playground.Blazor.Services.Api.Expendable;

internal interface IExpendableActivityFeed
{
    IReadOnlyList<ExpendableActivityEntry> GetRecent(int count = 15, string? category = null);

    void AddSuccess(string category, string action);

    void AddFailure(string category, string action, string error);

    void Clear();
}

internal sealed record ExpendableActivityEntry(DateTimeOffset At, string Category, string Message, bool IsSuccess);
