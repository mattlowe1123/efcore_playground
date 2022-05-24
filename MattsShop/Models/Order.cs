namespace MattsShop.Models;

public class Order
{
    public Order(int customerId, DateTime date, string description)
    {
        CustomerId = customerId;
        Date = date;
        Description = description;
    }

    public int Id { get; private set; }
    public int CustomerId { get; private set; }
    public string Description { get; private set; }
    public DateTime Date { get; private set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; private set; } = null!;
}
