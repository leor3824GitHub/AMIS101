using Mediator;

namespace FSH.Modules.MasterData.Features.v1.Suppliers.DeleteSupplier;

public sealed record DeleteSupplierCommand(Guid Id) : ICommand;
