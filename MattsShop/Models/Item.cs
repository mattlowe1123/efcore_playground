
namespace MattsShop.Models;

public interface IItem
{
    public int Id { get; }
    public string Name { get; }
    public decimal Price { get; }
}

public class Item : IItem
{
    public Item(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
}
