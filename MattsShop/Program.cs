using MattsShop.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ShopContext>(x =>
{
    var connectionString = builder.Configuration.GetConnectionString("mattsshop");
    x.UseSqlServer(connectionString);
});

var app = builder.Build();
app.MapGet("/", () => "Hello World!");

app.Run();
