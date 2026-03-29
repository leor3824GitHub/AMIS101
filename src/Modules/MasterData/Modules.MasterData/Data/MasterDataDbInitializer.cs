using FSH.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FSH.Modules.MasterData.Domain;
using FSH.Framework.Shared.Multitenancy;

namespace FSH.Modules.MasterData.Data;

internal sealed class MasterDataDbInitializer(
    ILogger<MasterDataDbInitializer> logger,
    MasterDataDbContext context) : IDbInitializer
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if ((await context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).Any())
        {
            await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("[{Tenant}] applied database migrations for master data module", context.TenantInfo?.Identifier);
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        // Offices
        if (!await context.Offices.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var offices = new[]
            {
                Office.Create("OFF-HQ", "Headquarters", "Main office"),
                Office.Create("OFF-BR1", "Branch 1", "First branch"),
                Office.Create("OFF-BR2", "Branch 2", "Second branch"),
                Office.Create("OFF-WH", "Warehouse", "Distribution center"),
                Office.Create("OFF-REMOTE", "Remote Office", "Remote operations"),
            };
            await context.Offices.AddRangeAsync(offices, cancellationToken).ConfigureAwait(false);
        }

        // Departments
        if (!await context.Departments.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var departments = new[]
            {
                Department.Create("DPT-SALES", "Sales", "Sales department"),
                Department.Create("DPT-PUR", "Purchasing", "Purchasing and procurement"),
                Department.Create("DPT-HR", "Human Resources", "HR and talent"),
                Department.Create("DPT-IT", "IT", "Information Technology"),
                Department.Create("DPT-FIN", "Finance", "Accounting and finance"),
            };
            await context.Departments.AddRangeAsync(departments, cancellationToken).ConfigureAwait(false);
        }

        // Positions
        if (!await context.Positions.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var positions = new[]
            {
                Position.Create("POS-MGR", "Manager", "Manager"),
                Position.Create("POS-SUP", "Supervisor", "Supervisor"),
                Position.Create("POS-ENG", "Engineer", "Engineer"),
                Position.Create("POS-CLK", "Clerk", "Clerk"),
                Position.Create("POS-DIR", "Director", "Director"),
            };
            await context.Positions.AddRangeAsync(positions, cancellationToken).ConfigureAwait(false);
        }

        // UnitOfMeasures
        if (!await context.UnitOfMeasures.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var uoms = new[]
            {
                UnitOfMeasure.Create("UOM-PCE", "Piece", "Piece"),
                UnitOfMeasure.Create("UOM-KG", "Kilogram", "Kilogram"),
                UnitOfMeasure.Create("UOM-G", "Gram", "Gram"),
                UnitOfMeasure.Create("UOM-L", "Liter", "Liter"),
                UnitOfMeasure.Create("UOM-M", "Meter", "Meter"),
            };
            await context.UnitOfMeasures.AddRangeAsync(uoms, cancellationToken).ConfigureAwait(false);
        }

        // Suppliers
        if (!await context.Suppliers.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var suppliers = new[]
            {
                Supplier.Create("SUP-001", "Acme Supplies", "General supplier", "Alice Smith", "alice@acme.example", "555-0101", "1 Acme Way"),
                Supplier.Create("SUP-002", "Global Foods", "Food supplier", "Bob Jones", "bob@globalfoods.example", "555-0102", "12 Market St"),
                Supplier.Create("SUP-003", "TechSource", "Electronics supplier", "Carol Lee", "carol@techsource.example", "555-0103", "42 Tech Park"),
                Supplier.Create("SUP-004", "OfficePro", "Office supplies", "David Kim", "david@officepro.example", "555-0104", "8 Office Plaza"),
                Supplier.Create("SUP-005", "Furnishings Co", "Furniture supplier", "Eve Turner", "eve@furnishings.example", "555-0105", "99 Furniture Rd"),
            };
            await context.Suppliers.AddRangeAsync(suppliers, cancellationToken).ConfigureAwait(false);
        }

        // Categories
        if (!await context.Categories.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var categories = new[]
            {
                Category.Create("CAT-BEV", "Beverages", "Drinks and beverages"),
                Category.Create("CAT-FOOD", "Food", "Food items"),
                Category.Create("CAT-ELEC", "Electronics", "Electronic goods"),
                Category.Create("CAT-STAT", "Stationery", "Office stationery"),
                Category.Create("CAT-FURN", "Furniture", "Furniture and fixtures"),
            };
            await context.Categories.AddRangeAsync(categories, cancellationToken).ConfigureAwait(false);
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // EmployeeProfiles - link to existing office/department/position
        if (!await context.Employees.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            var tenantId = context.TenantInfo?.Identifier ?? MultitenancyConstants.Root.Id;

            var firstOffice = await context.Offices.FirstAsync(cancellationToken).ConfigureAwait(false);
            var firstDepartment = await context.Departments.FirstAsync(cancellationToken).ConfigureAwait(false);
            var firstPosition = await context.Positions.FirstAsync(cancellationToken).ConfigureAwait(false);

            var employees = new[]
            {
                EmployeeProfile.Create(tenantId, "EMP-001", "John", "Doe", firstOffice.Id, firstDepartment.Id, firstPosition.Id, null, "john.doe@example.com"),
                EmployeeProfile.Create(tenantId, "EMP-002", "Jane", "Smith", firstOffice.Id, firstDepartment.Id, firstPosition.Id, null, "jane.smith@example.com"),
                EmployeeProfile.Create(tenantId, "EMP-003", "Carlos", "Garcia", firstOffice.Id, firstDepartment.Id, firstPosition.Id, null, "carlos.garcia@example.com"),
                EmployeeProfile.Create(tenantId, "EMP-004", "Aisha", "Khan", firstOffice.Id, firstDepartment.Id, firstPosition.Id, null, "aisha.khan@example.com"),
                EmployeeProfile.Create(tenantId, "EMP-005", "Liu", "Wang", firstOffice.Id, firstDepartment.Id, firstPosition.Id, null, "liu.wang@example.com"),
            };

            await context.Employees.AddRangeAsync(employees, cancellationToken).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        logger.LogInformation("[{Tenant}] seeded master data.", context.TenantInfo?.Identifier);
    }
}


