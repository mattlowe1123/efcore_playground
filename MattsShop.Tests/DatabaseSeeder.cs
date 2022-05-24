using System;
using System.Linq;
using System.Threading.Tasks;
using MattsShop.Context;
using MattsShop.Models;
using Microsoft.EntityFrameworkCore;

namespace MattsShop.Tests
{
    public class DatabaseSeeder
    {
        public int NumberCustomers { get; } = 100;
        public int NumberItems { get; } = 50;
        public int OrdersPerCustomer { get; } = 10;
        public int MaxItemsPerOrder { get; } = 10;

        public async Task SeedIfEmpty(ShopContext context)
        {
            var existingCustomers = await context.Customers.FirstOrDefaultAsync();
            if (existingCustomers != null)
            {
                return;
            }

            for (var i = 0; i < NumberCustomers; i++)
            {
                var customer = new Customer($"Customer_{i}");
                context.Customers.Add(customer);
            }

            await context.SaveChangesAsync();

            for (var i = 0; i < NumberItems; i++)
            {
                var item = new Item($"Item_{i}", (i + 1) * 1.03m);
                context.Items.Add(item);
            }

            await context.SaveChangesAsync();

            var customers = await context.Customers.ToListAsync();
            for (var i = 0; i < OrdersPerCustomer; i++)
            {
                foreach (var customer in customers)
                {
                    var order = new Order(customer.Id, DateTime.Now.AddDays(-i), $"description_{i}");
                    context.Orders.Add(order);
                }
            }

            await context.SaveChangesAsync();

            var random = new Random();
            var orders = await context.Orders.ToListAsync();
            foreach (var order in orders)
            {
                for (var i = 0; i < random.Next(MaxItemsPerOrder); i++)
                {
                    var item = await context.Items.Skip(random.Next(NumberItems - 1)).FirstAsync();
                    var orderItem = new OrderItem(order.Id, item.Id);
                    context.OrderItems.Add(orderItem);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
