namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.Models;

public interface IAnomalyOrchestrator
{
    Task<AnomalyAuditReport> ProcessEvaluationsAsync(IEnumerable<AnomalyEvaluationResult> evaluations);
}

public class AnomalyOrchestrator : IAnomalyOrchestrator
{
    private readonly ICorrelationEngine _correlationEngine;
    private readonly IConfidenceReconciler _reconciler;
    private readonly IExplanationMapper _explanationMapper;

    public AnomalyOrchestrator(
        ICorrelationEngine correlationEngine,
        IConfidenceReconciler reconciler,
        IExplanationMapper explanationMapper)
    {
        _correlationEngine = correlationEngine;
        _reconciler = reconciler;
        _explanationMapper = explanationMapper;
    }

    public Task<AnomalyAuditReport> ProcessEvaluationsAsync(IEnumerable<AnomalyEvaluationResult> evaluations)
    {
        var evals = evaluations.ToList();
        
        // 1. STAGE: Correlation (Sinyal Gruplama ve Hizalama)
        var correlatedEvidences = _correlationEngine.CorrelateAndGroup(evals);

        // 2. STAGE: Confidence Reconciliation (Ağırlıklı Skor Uzlaştırma)
        var (finalConfidence, finalSeverity) = _reconciler.Reconcile(evals);

        // 3. STAGE: Deterministic Explanation Mapping (Şeblon Tabanlı Denetlenebilir Raporlama)
        bool isAnyConfirmedAnomaly = evals.Any(e => e.IsAnomaly);
        var explanation = _explanationMapper.MapToDeterministicExplanation(correlatedEvidences, isAnyConfirmedAnomaly);

        // FINAL: IMMUTABLE AUDIT REPORT (Final Output)
        var report = new AnomalyAuditReport(
            IsConfirmedAnomaly: isAnyConfirmedAnomaly,
            FinalSeverity: finalSeverity,
            AggregateConfidence: finalConfidence,
            RuleEvaluations: evals,
            MappedExplanation: explanation,
            GeneratedAt: DateTime.UtcNow
        );

        return Task.FromResult(report);
    }
}
