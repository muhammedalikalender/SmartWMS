namespace SmartWMS.Application.Common.Interfaces;

using System.Threading;
using System.Threading.Tasks;
using SmartWMS.Domain.Entities;

public interface ISensorSnapshotRepository
{
    Task AddAsync(SensorSnapshot snapshot, CancellationToken cancellationToken = default);
}
