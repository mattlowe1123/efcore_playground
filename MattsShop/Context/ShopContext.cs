using MattsShop.Context.Configuration;
using MattsShop.Models;
using Microsoft.EntityFrameworkCore;

namespace MattsShop.Context;

public class ShopContext : DbContext
{
    public static ShopContext Create(DbContextOptions<ShopContext> options) =>
        new ShopContext(options);

    public ShopContext(DbContextOptions options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}
