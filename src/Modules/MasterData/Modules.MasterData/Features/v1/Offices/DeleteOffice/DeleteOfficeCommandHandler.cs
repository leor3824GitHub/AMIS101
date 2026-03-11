using FSH.Framework.Core.Context;
using FSH.Modules.MasterData.Contracts.v1.References;
using FSH.Modules.MasterData.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.MasterData.Features.v1.Offices.DeleteOffice;

public sealed class DeleteOfficeCommandHandler : ICommandHandler<DeleteOfficeCommand, Unit>
{
    private readonly MasterDataDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public DeleteOfficeCommandHandler(MasterDataDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(DeleteOfficeCommand command, CancellationToken cancellationToken)
    {
        var office = await _dbContext.Offices
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Office {command.Id} not found.");

        office.IsDeleted = true;
        office.DeletedOnUtc = DateTimeOffset.UtcNow;
        office.DeletedBy = _currentUser.GetUserId().ToString();

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
