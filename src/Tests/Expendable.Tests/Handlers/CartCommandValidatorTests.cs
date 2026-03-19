using FSH.Modules.Expendable.Contracts.v1.Cart;
using FSH.Modules.Expendable.Features.v1.Cart.AddToCart;
using FSH.Modules.Expendable.Features.v1.Cart;
using FSH.Modules.Expendable.Features.v1.Cart.ConvertCartToRequest;
using Shouldly;
using Xunit;

namespace Expendable.Tests.Handlers;

public sealed class CartCommandValidatorTests
{
    [Fact]
    public void AddToCartCommandValidator_ZeroQuantity_ShouldFail()
    {
        // Arrange
        var validator = new AddToCartCommandValidator();
        var command = new AddToCartCommand(Guid.NewGuid(), Guid.NewGuid(), 0, 0);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Quantity));
    }

    [Fact]
    public void AddToCartCommandValidator_ZeroUnitPrice_ShouldPass()
    {
        // Arrange
        var validator = new AddToCartCommandValidator();
        var command = new AddToCartCommand(Guid.NewGuid(), Guid.NewGuid(), 1, 0);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void UpdateCartItemQuantityCommandValidator_NegativeQuantity_ShouldFail()
    {
        // Arrange
        var validator = new UpdateCartItemQuantityCommandValidator();
        var command = new UpdateCartItemQuantityCommand(Guid.NewGuid(), Guid.NewGuid(), -1);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.NewQuantity));
    }

    [Fact]
    public void UpdateCartItemQuantityCommandValidator_ZeroQuantity_ShouldPass()
    {
        // Arrange
        var validator = new UpdateCartItemQuantityCommandValidator();
        var command = new UpdateCartItemQuantityCommand(Guid.NewGuid(), Guid.NewGuid(), 0);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void ConvertCartToSupplyRequestCommandValidator_EmptyDepartment_ShouldFail()
    {
        // Arrange
        var validator = new ConvertCartToSupplyRequestCommandValidator();
        var command = new ConvertCartToSupplyRequestCommand(Guid.NewGuid(), string.Empty);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.DepartmentId));
    }

    [Fact]
    public void ConvertCartToSupplyRequestCommandValidator_ValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new ConvertCartToSupplyRequestCommandValidator();
        var command = new ConvertCartToSupplyRequestCommand(
            Guid.NewGuid(),
            "OPS",
            "Need printer paper for operations");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
