using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Store.Application.Common.Interfaces;
using Store.Domain.Entities;
using Store.Domain.Enums;

namespace Store.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(
        StoreDbContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger logger)
    {
        var adminExists = await context.Users.AnyAsync(u => u.Role == UserRole.Admin);
        if (adminExists)
        {
            return;
        }

        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        var fullName = configuration["AdminSeed:FullName"] ?? "Store Admin";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "No admin user found and AdminSeed:Email/AdminSeed:Password are not configured — skipping seed. " +
                "Set them via dotnet user-secrets to create the initial admin.");
            return;
        }

        var admin = new User
        {
            Email = email,
            FullName = fullName,
            PasswordHash = passwordHasher.Hash(password),
            Role = UserRole.Admin
        };

        context.Users.Add(admin);
        context.Carts.Add(new Cart { UserId = admin.Id });

        await context.SaveChangesAsync(CancellationToken.None);

        logger.LogInformation("Seeded initial admin user {Email}", email);
    }
}
