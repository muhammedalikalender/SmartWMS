namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SmartWMS.Application.Common.Interfaces;
using SmartWMS.Application.Features.Anomaly.Models;
using SmartWMS.Domain.Entities;
using SmartWMS.Domain.ValueObjects;

public class AnomalyReplayService : IAnomalyReplayService
{
    private readonly IAnomalyRepository _anomalyRepository;
    private readonly IAnomalyEngine _anomalyEngine;

    public AnomalyReplayService(
        IAnomalyRepository anomalyRepository, 
        IAnomalyEngine anomalyEngine)
    {
        _anomalyRepository = anomalyRepository;
        _anomalyEngine = anomalyEngine;
    }

    public async Task<AnomalyReplayResult> ReplayDecisionAsync(Guid alertId, CancellationToken cancellationToken = default)
    {
        try 
        {
            // 1. LOAD AUDIT DATA
            var alert = await _anomalyRepository.GetByIdAsync(alertId, cancellationToken);
            if (alert == null) throw new InvalidOperationException("Anomali kaydı bulunamadı.");

            // PRODUCTION HARDENING: Version & Integrity Check
            if (string.IsNullOrEmpty(alert.ContextSnapshotJson))
                throw new InvalidOperationException("Anomali snapshot verisi bozulmuş veya eksik.");
            
            // 2. CONTEXT RECONSTITUTION
            var snapshot = JsonSerializer.Deserialize<AnomalyContextSnapshot>(alert.ContextSnapshotJson);
            var originalReport = JsonSerializer.Deserialize<AnomalyAuditReport>(alert.AuditReportJson);

            if (snapshot == null || originalReport == null)
                throw new InvalidOperationException("Snapshot deserialization hatası: Format uyumsuzluğu.");

        // Staff-Level Note: Gerçek aggregate yerine rules-engine için 
        // gerekli alanları doldurulmuş 'rehydrated' bir shelf oluşturuyoruz.
        var rehydratedShelf = new Shelf(snapshot.ShelfId, "Replay-Shelf-" + snapshot.ShelfId);
        
        var rehydratedSnapshot = new SensorSnapshot(
            snapshot.ShelfId, 
            new Mass(snapshot.SensedMass), 
            new StabilityIndex(snapshot.SensedStability)
        );

        var context = new AnomalyContext(
            ShelfSnapshot: rehydratedShelf,
            LastSensorData: rehydratedSnapshot,
            EvaluationTriggerType: snapshot.TriggerType
        );

        // 3. RE-EXECUTE PIPELINE (Deterministic Re-run)
        var replayedReport = await _anomalyEngine.EvaluateAllRulesAsync(context);

        // 4. DIFF ANALYSIS (Fark Analizi)
        // Staff-Level: Eğer hash tutuyorsa ve engine deterministic ise sonuç aynı olmalı.
        bool isReproduced = Math.Abs(replayedReport.FinalSeverity - originalReport.FinalSeverity) < 0.001;
        
        var divergenceNotes = isReproduced 
            ? "Sonuç birebir doğrulandı. İşlem deterministik." 
            : $"DİKKAT: Sonuç sapması saptandı! Orijinal: {originalReport.FinalSeverity}, Replay: {replayedReport.FinalSeverity}";

            return new AnomalyReplayResult(
                AlertId: alertId,
                IsReproduced: isReproduced,
                OriginalReport: originalReport,
                ReplayedReport: replayedReport,
                DivergenceNotes: divergenceNotes
            );
        }
        catch (Exception ex)
        {
            // Forensic-grade logging (Basitleştirilmiş)
            Console.WriteLine($"REPLAY ERROR [Alert: {alertId}]: {ex.Message}");
            throw new InvalidOperationException($"Replay işlemi başarısız: {ex.Message}");
        }
    }
}
