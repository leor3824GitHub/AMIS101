using FSH.Framework.Core.Domain;

namespace FSH.Modules.Expendable.Domain.Products;

/// <summary>Product status enumeration</summary>
public enum ProductStatus
{
    None = 0,
    Active = 1,
    Inactive = 2,
    Discontinued = 3,
    OutOfStock = 4
}

public class Product : AggregateRoot<Guid>, IHasTenant, IAuditableEntity
{
    public string TenantId { get; private set; } = default!;
    public string SKU { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Description { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public string UnitOfMeasure { get; set; } = default!; // e.g., "PCS", "BOX", "KG"
    public int MinimumStockLevel { get; set; }
    public int ReorderQuantity { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public string? CategoryId { get; set; }
    public string? SupplierId { get; set; }
    public string? ImageUrl { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();

    private readonly List<ProductImage> _images = new();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    // IAuditableEntity
    public DateTimeOffset CreatedOnUtc { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedOnUtc { get; set; }
    public string? LastModifiedBy { get; set; }

    // ISoftDeletable
    public DateTimeOffset? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }

    /// <summary>Factory method to create a new product</summary>
    public static Product Create(string tenantId, string sku, string name, string description,
        decimal unitPrice, string unitOfMeasure, int minimumStockLevel, int reorderQuantity,
        IEnumerable<string>? imageUrls = null)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SKU = sku,
            Name = name,
            Description = description,
            UnitPrice = unitPrice,
            UnitOfMeasure = unitOfMeasure,
            MinimumStockLevel = minimumStockLevel,
            ReorderQuantity = reorderQuantity,
            Status = ProductStatus.Active,
            CreatedOnUtc = DateTimeOffset.UtcNow
        };

        product.SetImages(imageUrls);
        return product;
    }

    /// <summary>Activate the product</summary>
    public void Activate()
    {
        Status = ProductStatus.Active;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Deactivate the product</summary>
    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Mark the product as discontinued</summary>
    public void Discontinue()
    {
        Status = ProductStatus.Discontinued;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Mark the product as out of stock</summary>
    public void MarkOutOfStock()
    {
        Status = ProductStatus.OutOfStock;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>Update product details</summary>
        public void Update(string name, string description, decimal unitPrice,
        int minimumStockLevel, int reorderQuantity, IEnumerable<string>? imageUrls = null)
    {
        Name = name;
        Description = description;
        UnitPrice = unitPrice;
        MinimumStockLevel = minimumStockLevel;
        ReorderQuantity = reorderQuantity;
        SetImages(imageUrls);
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    public void SetImages(IEnumerable<string>? imageUrls)
    {
        _images.Clear();

        if (imageUrls is null)
        {
            return;
        }

        foreach (var url in imageUrls
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3))
        {
            _images.Add(ProductImage.Create(url));
        }
    }

    /// <summary>Soft delete the product</summary>
    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOnUtc = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }
}

public sealed class ProductImage
{
    public string Url { get; private set; } = default!;

    private ProductImage()
    {
    }

    private ProductImage(string url)
    {
        Url = url;
    }

    public static ProductImage Create(string url) => new(url);
}

