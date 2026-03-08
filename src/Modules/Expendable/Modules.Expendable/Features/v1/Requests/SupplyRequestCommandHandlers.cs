using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Contracts.v1.Requests;
using FSH.Modules.Expendable.Data;
using FSH.Modules.Expendable.Domain.Requests;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Requests;

public sealed class CreateSupplyRequestCommandHandler : ICommandHandler<CreateSupplyRequestCommand, SupplyRequestDto>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public CreateSupplyRequestCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<SupplyRequestDto> Handle(CreateSupplyRequestCommand command, CancellationToken cancellationToken)
    {
        // Generate request number (simplified - use your own logic)
        var requestNumber = $"REQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}";

        var request = SupplyRequest.Create(
            _currentUser.GetTenant() ?? throw new InvalidOperationException("Tenant ID required"),
            requestNumber,
            _currentUser.GetUserId().ToString(),
            command.DepartmentId,
            command.BusinessJustification,
            command.NeededByDate);

        request.CreatedBy = _currentUser.GetUserId().ToString();

        _dbContext.SupplyRequests.Add(request);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return request.ToSupplyRequestDto();
    }
}

public sealed class AddSupplyRequestItemCommandHandler : ICommandHandler<AddSupplyRequestItemCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public AddSupplyRequestItemCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(AddSupplyRequestItemCommand command, CancellationToken cancellationToken)
    {
        var request = await _dbContext.SupplyRequests
            .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Supply request {command.RequestId} not found.");

        request.AddItem(command.ProductId, command.Quantity, command.Notes);
        request.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class RemoveSupplyRequestItemCommandHandler : ICommandHandler<RemoveSupplyRequestItemCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public RemoveSupplyRequestItemCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(RemoveSupplyRequestItemCommand command, CancellationToken cancellationToken)
    {
        var request = await _dbContext.SupplyRequests
            .FirstOrDefaultAsync(r => r.Id == command.RequestId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Supply request {command.RequestId} not found.");

        request.RemoveItem(command.ProductId);
        request.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class SubmitSupplyRequestCommandHandler : ICommandHandler<SubmitSupplyRequestCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public SubmitSupplyRequestCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(SubmitSupplyRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _dbContext.SupplyRequests
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Supply request {command.Id} not found.");

        request.Submit();
        request.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class ApproveSupplyRequestCommandHandler : ICommandHandler<ApproveSupplyRequestCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public ApproveSupplyRequestCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(ApproveSupplyRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _dbContext.SupplyRequests
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Supply request {command.Id} not found.");

        request.Approve(_currentUser.GetUserId().ToString(), command.ApprovedQuantities);
        request.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class RejectSupplyRequestCommandHandler : ICommandHandler<RejectSupplyRequestCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public RejectSupplyRequestCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(RejectSupplyRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _dbContext.SupplyRequests
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Supply request {command.Id} not found.");

        request.Reject(command.Reason);
        request.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class MarkSupplyRequestFulfilledCommandHandler : ICommandHandler<MarkSupplyRequestFulfilledCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public MarkSupplyRequestFulfilledCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(MarkSupplyRequestFulfilledCommand command, CancellationToken cancellationToken)
    {
        var request = await _dbContext.SupplyRequests
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Supply request {command.Id} not found.");

        request.MarkFulfilled();
        request.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}

public sealed class CancelSupplyRequestCommandHandler : ICommandHandler<CancelSupplyRequestCommand>
{
    private readonly ExpendableDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public CancelSupplyRequestCommandHandler(ExpendableDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(CancelSupplyRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _dbContext.SupplyRequests
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Supply request {command.Id} not found.");

        request.Cancel();
        request.LastModifiedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return default;
    }
}



