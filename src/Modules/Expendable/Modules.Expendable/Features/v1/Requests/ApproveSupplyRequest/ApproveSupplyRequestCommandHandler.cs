using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Contracts.v1.Requests;
using FSH.Modules.Expendable.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Expendable.Features.v1.Requests.ApproveSupplyRequest;

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
