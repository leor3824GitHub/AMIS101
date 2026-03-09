namespace FSH.Playground.Blazor.Services.Api.Expendable;

/// <summary>
/// Utility for building query strings consistently across Expendable API clients.
/// Eliminates duplication of URL parameter encoding and joining logic.
/// </summary>
internal static class QueryStringBuilder
{
    /// <summary>
    /// Builds a relative URI with query parameters, properly encoding values.
    /// </summary>
    public static Uri Build(string basePath, params (string key, object? value)[] parameters)
    {
        var queryParams = parameters
            .Where(p => p.value != null && !string.IsNullOrWhiteSpace(p.value.ToString()))
            .Select(p => $"{p.key}={Uri.EscapeDataString(p.value!.ToString()!)}")
            .ToList();

        var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        return new Uri(basePath + query, UriKind.Relative);
    }
}
