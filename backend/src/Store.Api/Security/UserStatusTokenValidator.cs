using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Interfaces;

namespace Store.Api.Security;

/// <summary>
/// A JWT is valid until it expires, so blocking a user would otherwise take up to AccessTokenMinutes
/// to bite. This rejects the token immediately when the account is blocked or no longer exists.
/// Cost: one indexed primary-key lookup per authenticated request — fine at this scale.
/// </summary>
public static class UserStatusTokenValidator
{
    public static async Task ValidateAsync(TokenValidatedContext context)
    {
        if (!Guid.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            context.Fail("Token has no valid subject.");
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<IApplicationDbContext>();

        var isBlocked = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => (bool?)u.IsBlocked)
            .FirstOrDefaultAsync(context.HttpContext.RequestAborted);

        if (isBlocked is null or true)
        {
            context.Fail("User is blocked or no longer exists.");
        }
    }
}
