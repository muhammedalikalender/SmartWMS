namespace SmartWMS.Domain.Entities;

public class WarehouseItem
{
    public Guid Id { get; private set; }
    public string Sku { get; private set; }
    public string Name { get; private set; }

    public Mass Mass { get; private set; }
    public Guid ShelfId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public WarehouseItem(string sku, string name, Mass mass, Guid shelfId)
    {
        Id = Guid.NewGuid();
        Sku = sku;
        Name = name;
        Mass = mass;
        ShelfId = shelfId;
        CreatedAt = DateTime.UtcNow;
    }
}