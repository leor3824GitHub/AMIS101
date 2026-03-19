using FSH.Framework.Core.Context;
using FSH.Modules.Expendable.Domain.Cart;
using System.Security.Claims;

namespace FSH.Modules.Expendable.Features.v1.Cart;

internal static class CartAccessGuard
{
    public static void EnsureCanAccessEmployee(ICurrentUser currentUser, string employeeId)
    {
        var currentEmployeeId = ResolveCurrentEmployeeId(currentUser);
        if (!string.Equals(employeeId, currentEmployeeId, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("You are not authorized to access this employee cart.");
        }
    }

    public static void EnsureCanAccessCart(ICurrentUser currentUser, EmployeeShoppingCart cart)
        => EnsureCanAccessEmployee(currentUser, cart.EmployeeId);

    private static string ResolveCurrentEmployeeId(ICurrentUser currentUser)
    {
        var claims = currentUser.GetUserClaims();
        var employeeId = claims?.FirstOrDefault(c => c.Type == "employee_id")?.Value
            ?? claims?.FirstOrDefault(c => c.Type == "employeeId")?.Value
            ?? claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
            ?? claims?.FirstOrDefault(c => c.Type == "sub")?.Value;

        return string.IsNullOrWhiteSpace(employeeId)
            ? currentUser.GetUserId().ToString()
            : employeeId;
    }
}