namespace SmartWMS.Domain.Common;

using System;

// MediatR uyumluluğu için INotification eklenebilir, fakat altyapı bağımsız kalmak adına pure C# interface.
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
