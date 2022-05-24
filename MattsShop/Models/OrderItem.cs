namespace MattsShop.Models;

public class OrderItem
{
    public OrderItem(int orderId, int itemId)
    {
        OrderId = orderId;
        ItemId = itemId;
    }

    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public int ItemId { get; private set; }

    public virtual Order Order { get; } = null!;
    public virtual Item Item { get; } = null;

}
