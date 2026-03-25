using System.Collections.Generic;

namespace FSH.Playground.Blazor.ApiClient;

/// <summary>
/// Additional response types for MasterData API - paging wrappers
/// </summary>

public partial class PagedResponseOfSupplierDto
{
    public ICollection<SupplierDto>? Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public partial class PagedResponseOfCategoryDto
{
    public ICollection<CategoryDto>? Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
