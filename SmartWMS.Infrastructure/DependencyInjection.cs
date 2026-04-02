namespace Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartWMS.Application.Common.Interfaces;
using SmartWMS.Application.Features.Anomaly.DataProviders;
using SmartWMS.Domain.Entities;
using SmartWMS.Infrastructure.Events;
using SmartWMS.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Data Provider Layer (Analytic Access)
        services.AddScoped<IAnomalyDataProvider, AnomalyDataProvider>();

        // Anomaly Persistence (Audit Store)
        services.AddSingleton<IAnomalyRepository, MockAnomalyRepository>();
        services.AddScoped<IDecisionGraphCache, DecisionGraphCache>();

        // Repositories & UoW (Mock Implementations for Runtime Stability)
        services.AddScoped<IShelfRepository, MockShelfRepository>();
        services.AddScoped<ISensorSnapshotRepository, MockSensorSnapshotRepository>();
        services.AddScoped<IUnitOfWork, MockUnitOfWork>();

        return services;
    }
}

public class MockShelfRepository : IShelfRepository
{
    private readonly List<Shelf> _shelves = new() 
    { 
        new Shelf(Guid.Parse("00000000-0000-0000-0000-000000000101"), "Shelf-101") 
    };
    
    public Task<Shelf?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) 
        => Task.FromResult(_shelves.FirstOrDefault(s => s.Id == id));

    public Task AddAsync(Shelf shelf, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateAsync(Shelf shelf, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public class MockSensorSnapshotRepository : ISensorSnapshotRepository
{
    public Task AddAsync(SensorSnapshot snapshot, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<SensorSnapshot?> GetLatestByShelfIdAsync(Guid shelfId, CancellationToken cancellationToken = default) 
        => Task.FromResult<SensorSnapshot?>(new SensorSnapshot(shelfId, new SmartWMS.Domain.ValueObjects.Mass(10), SmartWMS.Domain.ValueObjects.StabilityIndex.Stable));
}

public class MockUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
}

public class MockAnomalyRepository : IAnomalyRepository
{
    public Task AddAsync(AnomalyAlert alert, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<AnomalyAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<AnomalyAlert?>(null);
    public Task<IEnumerable<AnomalyAlert>> GetActiveAlertsByShelfIdAsync(Guid shelfId, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<AnomalyAlert>>(new List<AnomalyAlert>());
}
