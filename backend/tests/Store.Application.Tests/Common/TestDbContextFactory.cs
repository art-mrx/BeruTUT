using Microsoft.EntityFrameworkCore;
using Store.Infrastructure.Persistence;

namespace Store.Application.Tests.Common;

public static class TestDbContextFactory
{
    public static StoreDbContext Create()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new StoreDbContext(options);
    }
}
