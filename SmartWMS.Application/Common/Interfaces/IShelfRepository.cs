namespace SmartWMS.Application.Common.Interfaces;

using System;
using System.Threading;
using System.Threading.Tasks;
using SmartWMS.Domain.Entities;

public interface IShelfRepository
{
    Task<Shelf?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Shelf shelf, CancellationToken cancellationToken = default);
    Task UpdateAsync(Shelf shelf, CancellationToken cancellationToken = default);
}
