namespace SmartWMS.Domain.Entities;

public class SensorSnapshot
{
    public Guid Id { get; private set; }
    public Guid ShelfId { get; private set; }

    public Mass TotalMass { get; private set; }
    public StabilityIndex StabilityIndex { get; private set; }

    public DateTime Timestamp { get; private set; }

    public SensorSnapshot(Guid shelfId, Mass mass, StabilityIndex stability)
    {
        Id = Guid.NewGuid();
        ShelfId = shelfId;
        TotalMass = mass;
        StabilityIndex = stability;
        Timestamp = DateTime.UtcNow;
    }
}