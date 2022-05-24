using System;
using System.Linq;
using System.Threading.Tasks;
using MattsShop.Context;
using MattsShop.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MattsShop.Tests;

public class EfCore_PartOne
{
    private ShopContext _context = null!;
    [SetUp]
    public async Task Setup()
    {
        _context = await DatabaseHelper.CreateCleanMigratedContext(false);
    }

    [Test]
    public void Check_Expected_Sql()
    {
        var query = _context.Customers;
        var queryClean = _CleanSql(query.ToQueryString());
        Assert.That(queryClean, Is.EqualTo("SELECT [c].[Id], [c].[Name], [c].[ReallyLargeField] FROM [Customers] AS [c]"));
    }

    [Test]
    public async Task Where_Clause()
    {
        // Setup
        await _AddCustomer("Some customer");
        var expectedSql = "SELECT [c].[Id], [c].[Name], [c].[ReallyLargeField] FROM [Customers] AS [c] WHERE [c].[Name] = N'Some customer'";

        // TODO - Write a query that fetches a customer with the name "A new customer"
        var query = Answers.WhereClause(_context);

        // Assert
        var queryClean = _CleanSql(query.ToQueryString());
        Assert.That(queryClean, Is.EqualTo(expectedSql));
    }

    [Test]
    public async Task Select_Clause_Using_Anonymous()
    {
        // Setup
        var expectedSql = "SELECT [c].[Id], [c].[Name] FROM [Customers] AS [c]";

        // TODO - Write a query that fetches just the Id and Name for all customers
        // using anonymous types
        var query = Answers.SelectClauseAnonymous(_context);

        // Assert
        var queryClean = _CleanSql(query.ToQueryString());
        Assert.That(queryClean, Is.EqualTo(expectedSql));
    }

    [Test]
    public async Task Select_Clause_Using_Record()
    {
        // Setup
        var expectedSql = "SELECT [c].[Id], [c].[Name] FROM [Customers] AS [c]";


        // TODO - Write a query that fetches just the Id and Name for all customers
        // using BasicCustomer record defined below
        var query = Answers.SelectClauseRecord(_context);

        // Assert
        var queryClean = _CleanSql(query.ToQueryString());
        Assert.That(queryClean, Is.EqualTo(expectedSql));
    }

    private record BasicCustomer(int Id, string Name) : IBasicCustomer;

    [Test]
    public async Task Select_And_Where_Clause()
    {
        // Setup
        await _AddCustomer("Another customer");
        var expectedSql = "SELECT [c].[Id], [c].[Name] FROM [Customers] AS [c] WHERE [c].[Name] = N'Another customer'";

        // TODO - Write a query that fetches just the Id and Name for any customers
        // with the name "Another customer" using BasicCustomer record defined below
        var query = Answers.SelectWhereClause(_context);

        // Assert
        var queryClean = _CleanSql(query.ToQueryString());
        Assert.That(queryClean, Is.EqualTo(expectedSql));
    }

    [Test]
    public async Task Include_InnerJoin_Clause()
    {
        // Setup
        var customer = await _AddCustomer("InnerJoin customer");
        var item = await _AddItem("InnerJoin Item");
        await _AddOrderWithItem(customer.Id, item.Id, "InnerJoin order");
        var expectedSql =
            "SELECT [o].[Id], [o].[CustomerId], [o].[Date], [o].[Description], [c].[Id], [c].[Name], [c].[ReallyLargeField] FROM [Orders] AS [o] INNER JOIN [Customers] AS [c] ON [o].[CustomerId] = [c].[Id] WHERE [o].[Description] = N'InnerJoin order'";

        // Todo write a query that fetches an order with description "InnerJoin order" and the customer on the order
        var query = Answers.InnerJoinIncludeClause(_context);

        // Assert
        var queryClean = _CleanSql(query.ToQueryString());
        Assert.That(queryClean, Is.EqualTo(expectedSql));
        var orderWithCustomer = await query.SingleAsync();
        Assert.That(orderWithCustomer.Customer.Name, Is.EqualTo("InnerJoin customer"));
    }

    [Test]
    public async Task Include_LeftJoin_Clause()
    {
        // Setup
        var customer = await _AddCustomer("LeftJoin customer");
        var item = await _AddItem("LeftJoin Item");
        await _AddOrderWithItem(customer.Id, item.Id, "LeftJoin order");
        var expectedSql =
            "SELECT [o].[Id], [o].[CustomerId], [o].[Date], [o].[Description], [o0].[Id], [o0].[ItemId], [o0].[OrderId] FROM [Orders] AS [o] LEFT JOIN [OrderItems] AS [o0] ON [o].[Id] = [o0].[OrderId] WHERE [o].[Description] = N'LeftJoin order' ORDER BY [o].[Id]";

        // Todo write a query that fetches an order with description "LeftJoin order" and the order items
        var query = Answers.LeftJoinIncludeClause(_context);

        // Assert
        var queryClean = _CleanSql(query.ToQueryString());
        Assert.That(queryClean, Is.EqualTo(expectedSql));
        var orderWithOrderItems = await query.SingleAsync();
        Assert.That(orderWithOrderItems.OrderItems.SingleOrDefault(), Is.Not.Null);
    }

    [Test]
    public async Task Insert()
    {
        // Test to show that IDs are not populated until SaveChanges is ']
        var name = "test;";

        var customer = new Customer(name);
        _context.Customers.Add(customer);
        Assert.That(customer.Id, Is.EqualTo(0));
        await _context.SaveChangesAsync();
        Assert.That(customer.Id, Is.Not.EqualTo(0));
    }

    [Test]
    public async Task Insert_ByReference([Values] bool useReference)
    {
        var customer = new Customer("Some new customer");
        _context.Customers.Add(customer);
        Assert.That(customer.Id, Is.EqualTo(0));
        var order = new Order(customer.Id, DateTime.Now, "Test");

        _context.Orders.Add(order);

        if (!useReference)
        {
            Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
        }
        else
        {
            order.Customer = customer;
            await _context.SaveChangesAsync();
            Assert.That(order.Id, Is.Not.EqualTo(0));
        }
    }

    [Test]
    public async Task Update()
    {
        await _AddCustomer("Update test");

        var customer = await _context.Customers.SingleAsync(x => x.Name == "Update test");
        customer.Name = "New name";
        await _context.SaveChangesAsync();

        var latestCustomer = await _context.Customers.SingleAsync(x => x.Id == customer.Id);
        Assert.That(latestCustomer.Name, Is.EqualTo("New name"));
    }

    [Test]
    public async Task Delete()
    {
        var name = "Delete test";
        await _AddCustomer(name);

        var customer = await _context.Customers.SingleAsync(x => x.Name == name);
        _context.Remove(customer);
        await _context.SaveChangesAsync();

        var latestCustomer = await _context.Customers.SingleOrDefaultAsync(x => x.Id == customer.Id);
        Assert.That(latestCustomer, Is.Null);
    }

    [Test]
    public async Task Delete_bulk()
    {
        var name = "Delete bulk test";
        await _AddCustomer(name);

        var customer = await _context.Customers.Where(x => x.Name == name).ToListAsync();
        _context.RemoveRange(customer);
        await _context.SaveChangesAsync();

        var latestCustomer = await _context.Customers.SingleOrDefaultAsync(x => x.Name == name);
        Assert.That(latestCustomer, Is.Null);
    }

    private async Task<Customer> _AddCustomer(string name)
    {
        var customer = new Customer(name);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    private async Task<Item> _AddItem(string name)
    {
        var item = new Item(name, 1m);
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    private async Task _AddOrderWithItem(int customerId, int itemId, string description)
    {
        var order = new Order(customerId, DateTime.Now, description);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        var orderItem = new OrderItem(order.Id, itemId);
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();
    }

    private string _CleanSql(string sql) =>
        sql.Replace("\r\n", " ");


    // No peaking!!
    private static class Answers
    {
        public static IQueryable<Customer> WhereClause(ShopContext context) => context.Customers.Where(x => x.Name == "Some customer");
        public static IQueryable<Customer> WhereClauseQuerySyntax(ShopContext context) => (from c in context.Customers where c.Name == "Some customer" select c);

        public static IQueryable<dynamic> SelectClauseAnonymous(ShopContext context) => context.Customers.Select(c => new { c.Id, c.Name });
        public static IQueryable<IBasicCustomer> SelectClauseRecord(ShopContext context) => context.Customers.Select(c => new BasicCustomer(c.Id, c.Name));
        public static IQueryable<IBasicCustomer> SelectWhereClause(ShopContext context) => context.Customers.Where(c => c.Name == "Another customer").Select(c => new BasicCustomer(c.Id, c.Name));

        public static IQueryable<Order> InnerJoinIncludeClause(ShopContext context) =>
            context.Orders.Where(o => o.Description == "InnerJoin order").Include(o => o.Customer);

        public static IQueryable<Order> LeftJoinIncludeClause(ShopContext context) =>
            context.Orders.Where(o => o.Description == "LeftJoin order").Include(o => o.OrderItems);

    }

}
