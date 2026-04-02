namespace SmartWMS.Domain.Entities;

using System;
using System;
using SmartWMS.Domain.Common;
using SmartWMS.Domain.ValueObjects;

public class SensorSnapshot : Entity
{
    public Guid ShelfId { get; private set; }

    public Mass TotalMass { get; private set; }
    public StabilityIndex StabilityIndex { get; private set; }

    public DateTime Timestamp { get; private set; }

    // EF Core
    protected SensorSnapshot() { }

    public SensorSnapshot(Guid shelfId, Mass mass, StabilityIndex stability)
    {
        if (shelfId == Guid.Empty)
            throw new ArgumentException("Geçerli bir Shelf ID gereklidir.", nameof(shelfId));
            
        Id = Guid.NewGuid();
        ShelfId = shelfId;
        TotalMass = mass ?? throw new ArgumentNullException(nameof(mass));
        StabilityIndex = stability ?? throw new ArgumentNullException(nameof(stability));
        Timestamp = DateTime.UtcNow;
    }
}