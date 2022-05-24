using System.Collections.Generic;
using System.Threading.Tasks;
using MattsShop.Context;
using Microsoft.EntityFrameworkCore;

namespace MattsShop.Tests;

public static class DatabaseHelper
{
    private const string MattsShopDbName = "TestMattsShop";

    public static async Task<ShopContext> CreateCleanMigratedContext(bool withSeededData = false)
    {
        var context = ShopContext.Create(_CreateOptions());

        // Apply migrations
        var key = MattsShopDbName;
        if (Connections.AppliedMigrations.Contains(key) == false)
        {
            await context.Database.MigrateAsync();
            Connections.AppliedMigrations.Add(key);
        }

        await _ClearDatabase(context);

        if (withSeededData)
        {
            var seeder = new DatabaseSeeder();

            await seeder.SeedIfEmpty(context);
        }

        return context;
    }

    private static DbContextOptions<ShopContext> _CreateOptions()
    {
        var builder = new DbContextOptionsBuilder<ShopContext>();
        var connectionString = $"Server=(localdb)\\mssqllocaldb;Database={MattsShopDbName};Trusted_Connection=True;";
        var withSql = builder.UseSqlServer(connectionString);
        return withSql.Options;
    }

    private static async Task _ClearDatabase(ShopContext context)
    {
        context.OrderItems.RemoveRange(await context.OrderItems.ToListAsync());
        context.Orders.RemoveRange(await context.Orders.ToListAsync());
        context.Items.RemoveRange(await context.Items.ToListAsync());
        context.Customers.RemoveRange(await context.Customers.ToListAsync());

        await context.SaveChangesAsync();
    }
}

internal static class Connections
{
    public static readonly HashSet<string> AppliedMigrations = new();
}
