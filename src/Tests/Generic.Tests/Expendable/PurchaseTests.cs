using FSH.Modules.Expendable.Domain.Purchases;

namespace Generic.Tests.Expendable;

public sealed class PurchaseTests
{
    [Fact]
    public void Submit_WhenNoLineItems_ThrowsInvalidOperationException()
    {
        // Arrange
        var purchase = Purchase.Create("tenant-1", "PO-123", "SUP-1");

        // Act
        var action = purchase.Submit;

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void RecordReceipt_WhenReceivedQuantityExceedsOrdered_ThrowsInvalidOperationException()
    {
        // Arrange
        var purchase = Purchase.Create("tenant-1", "PO-123", "SUP-1");
        var productId = Guid.NewGuid();
        purchase.AddLineItem(productId, 5, 2.5m);
        purchase.Submit();
        purchase.Approve();

        // Act
        var action = () => purchase.RecordReceipt(productId, 6);

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }
}
