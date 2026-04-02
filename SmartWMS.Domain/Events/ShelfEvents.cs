namespace SmartWMS.Domain.Events;

using System;
using SmartWMS.Domain.Common;
using SmartWMS.Domain.ValueObjects;

public record ItemAddedDomainEvent(Guid ShelfId, Guid ItemId, Mass AddedMass) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public record ItemRemovedDomainEvent(Guid ShelfId, Guid ItemId, Mass RemovedMass) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

public record ShelfStabilityChangedDomainEvent(Guid ShelfId, StabilityIndex OldStability, StabilityIndex NewStability) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
