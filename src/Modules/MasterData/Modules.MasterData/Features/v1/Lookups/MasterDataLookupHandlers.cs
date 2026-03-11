using FSH.Framework.Persistence;
using FSH.Framework.Shared.Persistence;
using FSH.Modules.MasterData.Contracts.v1.References;
using FSH.Modules.MasterData.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.MasterData.Features.v1.Lookups;

public sealed class GetEmployeeReferenceByIdQueryHandler(MasterDataDbContext dbContext)
    : IQueryHandler<GetEmployeeReferenceByIdQuery, EmployeeReferenceDto?>
{
    public async ValueTask<EmployeeReferenceDto?> Handle(GetEmployeeReferenceByIdQuery query, CancellationToken cancellationToken)
    {
        return await MasterDataLookupQueryBuilder.BuildEmployeeReferenceQuery(dbContext)
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class GetEmployeeReferenceByIdentityUserIdQueryHandler(MasterDataDbContext dbContext)
    : IQueryHandler<GetEmployeeReferenceByIdentityUserIdQuery, EmployeeReferenceDto?>
{
    public async ValueTask<EmployeeReferenceDto?> Handle(GetEmployeeReferenceByIdentityUserIdQuery query, CancellationToken cancellationToken)
    {
        return await MasterDataLookupQueryBuilder.BuildEmployeeReferenceQuery(dbContext)
            .FirstOrDefaultAsync(x => x.IdentityUserId == query.IdentityUserId, cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class SearchEmployeeReferencesQueryHandler(MasterDataDbContext dbContext)
    : IQueryHandler<SearchEmployeeReferencesQuery, PagedResponse<EmployeeReferenceDto>>
{
    public async ValueTask<PagedResponse<EmployeeReferenceDto>> Handle(SearchEmployeeReferencesQuery query, CancellationToken cancellationToken)
    {
        var employeeQuery = MasterDataLookupQueryBuilder.BuildEmployeeReferenceQuery(dbContext);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            employeeQuery = employeeQuery.Where(x =>
                x.EmployeeNumber.Contains(query.Keyword) ||
                x.FirstName.Contains(query.Keyword) ||
                x.LastName.Contains(query.Keyword) ||
                (x.WorkEmail != null && x.WorkEmail.Contains(query.Keyword)) ||
                x.OfficeName.Contains(query.Keyword) ||
                x.DepartmentName.Contains(query.Keyword) ||
                x.PositionName.Contains(query.Keyword));
        }

        if (!string.IsNullOrWhiteSpace(query.IdentityUserId))
        {
            employeeQuery = employeeQuery.Where(x => x.IdentityUserId == query.IdentityUserId);
        }

        if (query.OfficeId.HasValue)
        {
            employeeQuery = employeeQuery.Where(x => x.OfficeId == query.OfficeId.Value);
        }

        if (query.DepartmentId.HasValue)
        {
            employeeQuery = employeeQuery.Where(x => x.DepartmentId == query.DepartmentId.Value);
        }

        if (query.PositionId.HasValue)
        {
            employeeQuery = employeeQuery.Where(x => x.PositionId == query.PositionId.Value);
        }

        if (query.IsActive.HasValue)
        {
            employeeQuery = employeeQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        employeeQuery = employeeQuery.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.EmployeeNumber);

        return await employeeQuery.ToPagedResponseAsync(query, cancellationToken).ConfigureAwait(false);
    }
}

public sealed class ListOfficeReferencesQueryHandler(MasterDataDbContext dbContext)
    : IQueryHandler<ListOfficeReferencesQuery, IReadOnlyList<OfficeReferenceDto>>
{
    public async ValueTask<IReadOnlyList<OfficeReferenceDto>> Handle(ListOfficeReferencesQuery query, CancellationToken cancellationToken)
    {
        var officesQuery = dbContext.Offices.AsNoTracking();

        if (!query.IncludeInactive)
        {
            officesQuery = officesQuery.Where(x => x.IsActive);
        }

        return await officesQuery
            .OrderBy(x => x.Name)
            .Select(x => new OfficeReferenceDto(x.Id, x.Code, x.Name, x.IsActive))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class GetOfficeReferenceByIdQueryHandler(MasterDataDbContext dbContext)
    : IQueryHandler<GetOfficeReferenceByIdQuery, OfficeReferenceDto?>
{
    public async ValueTask<OfficeReferenceDto?> Handle(GetOfficeReferenceByIdQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.Offices.AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new OfficeReferenceDto(x.Id, x.Code, x.Name, x.IsActive))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class ListDepartmentReferencesQueryHandler(MasterDataDbContext dbContext)
    : IQueryHandler<ListDepartmentReferencesQuery, IReadOnlyList<DepartmentReferenceDto>>
{
    public async ValueTask<IReadOnlyList<DepartmentReferenceDto>> Handle(ListDepartmentReferencesQuery query, CancellationToken cancellationToken)
    {
        var departmentsQuery = dbContext.Departments.AsNoTracking();

        if (!query.IncludeInactive)
        {
            departmentsQuery = departmentsQuery.Where(x => x.IsActive);
        }

        return await departmentsQuery
            .OrderBy(x => x.Name)
            .Select(x => new DepartmentReferenceDto(x.Id, x.Code, x.Name, x.IsActive))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class GetDepartmentReferenceByIdQueryHandler(MasterDataDbContext dbContext)
    : IQueryHandler<GetDepartmentReferenceByIdQuery, DepartmentReferenceDto?>
{
    public async ValueTask<DepartmentReferenceDto?> Handle(GetDepartmentReferenceByIdQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.Departments.AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new DepartmentReferenceDto(x.Id, x.Code, x.Name, x.IsActive))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class ListPositionReferencesQueryHandler(MasterDataDbContext dbContext)
    : IQueryHandler<ListPositionReferencesQuery, IReadOnlyList<PositionReferenceDto>>
{
    public async ValueTask<IReadOnlyList<PositionReferenceDto>> Handle(ListPositionReferencesQuery query, CancellationToken cancellationToken)
    {
        var positionsQuery = dbContext.Positions.AsNoTracking();

        if (!query.IncludeInactive)
        {
            positionsQuery = positionsQuery.Where(x => x.IsActive);
        }

        return await positionsQuery
            .OrderBy(x => x.Name)
            .Select(x => new PositionReferenceDto(x.Id, x.Code, x.Name, x.IsActive))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class GetPositionReferenceByIdQueryHandler(MasterDataDbContext dbContext)
    : IQueryHandler<GetPositionReferenceByIdQuery, PositionReferenceDto?>
{
    public async ValueTask<PositionReferenceDto?> Handle(GetPositionReferenceByIdQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.Positions.AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new PositionReferenceDto(x.Id, x.Code, x.Name, x.IsActive))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class ListUnitOfMeasureReferencesQueryHandler(MasterDataDbContext dbContext)
    : IQueryHandler<ListUnitOfMeasureReferencesQuery, IReadOnlyList<UnitOfMeasureReferenceDto>>
{
    public async ValueTask<IReadOnlyList<UnitOfMeasureReferenceDto>> Handle(ListUnitOfMeasureReferencesQuery query, CancellationToken cancellationToken)
    {
        var uomQuery = dbContext.UnitOfMeasures.AsNoTracking();

        if (!query.IncludeInactive)
        {
            uomQuery = uomQuery.Where(x => x.IsActive);
        }

        return await uomQuery
            .OrderBy(x => x.Name)
            .Select(x => new UnitOfMeasureReferenceDto(x.Id, x.Code, x.Name, x.IsActive))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

public sealed class GetUnitOfMeasureReferenceByIdQueryHandler(MasterDataDbContext dbContext)
    : IQueryHandler<GetUnitOfMeasureReferenceByIdQuery, UnitOfMeasureReferenceDto?>
{
    public async ValueTask<UnitOfMeasureReferenceDto?> Handle(GetUnitOfMeasureReferenceByIdQuery query, CancellationToken cancellationToken)
    {
        return await dbContext.UnitOfMeasures.AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new UnitOfMeasureReferenceDto(x.Id, x.Code, x.Name, x.IsActive))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

internal static class MasterDataLookupQueryBuilder
{
    internal static IQueryable<EmployeeReferenceDto> BuildEmployeeReferenceQuery(MasterDataDbContext dbContext)
    {
        return
            from employee in dbContext.Employees.AsNoTracking()
            join office in dbContext.Offices.AsNoTracking() on employee.OfficeId equals office.Id
            join department in dbContext.Departments.AsNoTracking() on employee.DepartmentId equals department.Id
            join position in dbContext.Positions.AsNoTracking() on employee.PositionId equals position.Id
            join unitOfMeasure in dbContext.UnitOfMeasures.AsNoTracking() on employee.DefaultUnitOfMeasureId equals unitOfMeasure.Id into uomGroup
            from uom in uomGroup.DefaultIfEmpty()
            select new EmployeeReferenceDto(
                employee.Id,
                employee.EmployeeNumber,
                employee.IdentityUserId,
                employee.FirstName,
                employee.LastName,
                employee.WorkEmail,
                office.Id,
                office.Code,
                office.Name,
                department.Id,
                department.Code,
                department.Name,
                position.Id,
                position.Code,
                position.Name,
                employee.DefaultUnitOfMeasureId,
                uom != null ? uom.Code : null,
                uom != null ? uom.Name : null,
                employee.IsActive);
    }
}



