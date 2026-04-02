namespace SmartWMS.Application.Common.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartWMS.Domain.Entities;

public interface IAnomalyRepository
{
    Task AddAsync(AnomalyAlert alert, CancellationToken cancellationToken = default);
    Task<AnomalyAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AnomalyAlert>> GetActiveAlertsByShelfIdAsync(Guid shelfId, CancellationToken cancellationToken = default);
}
