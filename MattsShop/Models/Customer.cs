namespace MattsShop.Models;

public interface IBasicCustomer
{
    public int Id { get; }
    public string Name { get; }
}

public class Customer : IBasicCustomer
{
    public Customer(string name)
    {
        Name = name;
    }

    public int Id { get; private set; }
    public string Name { get; set; }

    public string? ReallyLargeField { get; private set; } // Represents a field that contains a large amount of data
}
