namespace SmartWMS.Infrastructure.Persistence;

using System;
using System.Threading;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.DataProviders;
using SmartWMS.Domain.ValueObjects;

public class AnomalyDataProvider : IAnomalyDataProvider
{
    // Staff-Level Note: Gerçek implementasyonda burası DbContext üzerinden 
    // SensorSnapshots tablosuna Time-Window-Query atacak şekilde güncellenecek (Phase 2.0).
    
    public Task<Mass> GetBaselineMassAsync(Guid shelfId, TimeSpan window, CancellationToken cancellationToken = default)
    {
        // Şimdilik test amaçlı 10.0kg baseline dönüyoruz.
        return Task.FromResult(new Mass(10.0));
    }

    public Task<double> GetMassVarianceAsync(Guid shelfId, TimeSpan window, CancellationToken cancellationToken = default)
    {
        // Düşük varyans (Kararlı sistem simülasyonu)
        return Task.FromResult(0.05);
    }
}
