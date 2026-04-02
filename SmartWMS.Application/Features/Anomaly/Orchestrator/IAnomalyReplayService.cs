namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System;
using System.Threading;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.Models;

public interface IAnomalyReplayService
{
    /// <summary>
    /// Belirtilen anomaliyi, kaydedilen bağlam (context) snapshot'ı üzerinden 
    /// yeniden koşturur ve sonuçları orijinal raporla karşılaştırır.
    /// </summary>
    Task<AnomalyReplayResult> ReplayDecisionAsync(Guid alertId, CancellationToken cancellationToken = default);
}

public record AnomalyReplayResult(
    Guid AlertId,
    bool IsReproduced, // Yeni sonuç orijinaliyle aynı mı?
    AnomalyAuditReport OriginalReport,
    AnomalyAuditReport ReplayedReport,
    string DivergenceNotes
);
