namespace SmartWMS.Domain.Entities;

public class Shelf
{
    public Guid Id { get; private set; }
    public string Code { get; private set; }

    public List<WarehouseItem> Items { get; private set; } = new();

    public StabilityIndex StabilityIndex { get; private set; }

    public Shelf(string code)
    {
        Id = Guid.NewGuid();
        Code = code;
        StabilityIndex = new StabilityIndex(1.0);
    }

    public void AddItem(WarehouseItem item)
    {
        Items.Add(item);
    }

    public void UpdateStability(StabilityIndex index)
    {
        StabilityIndex = index;
    }
}