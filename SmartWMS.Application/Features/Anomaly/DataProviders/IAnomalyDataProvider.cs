namespace SmartWMS.Application.Features.Anomaly.DataProviders;

using System;
using System.Threading;
using System.Threading.Tasks;
using SmartWMS.Domain.ValueObjects;

public interface IAnomalyDataProvider
{
    /// <summary>
    /// Verilen zaman penceresindeki Baseline (Ortalama/Kabul Edilebilir) kütleyi döner.
    /// Kural motoruna (Rule) geçmişi hesaplanmış saf sonucu verir (History'i dayatmaz).
    /// </summary>
    Task<Mass> GetBaselineMassAsync(Guid shelfId, TimeSpan window, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Geçmiş kütle dalgalanmalarının standart sapmasını (Varyans) döner.
    /// Dalgalanma şiddetinin tolere edilebilirliği için ML veya istatistiki Baseline metriği sağlar.
    /// </summary>
    Task<double> GetMassVarianceAsync(Guid shelfId, TimeSpan window, CancellationToken cancellationToken = default);
}
