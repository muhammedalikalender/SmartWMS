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
    private readonly ISemanticMemoryStore _memoryStore;
    private readonly ICausalSignatureGenerator _signatureGenerator;

    public AnomalyOrchestrator(
        ICorrelationEngine correlationEngine,
        IConfidenceReconciler reconciler,
        IExplanationMapper explanationMapper,
        ISemanticMemoryStore memoryStore,
        ICausalSignatureGenerator signatureGenerator)
    {
        _correlationEngine = correlationEngine;
        _reconciler = reconciler;
        _explanationMapper = explanationMapper;
        _memoryStore = memoryStore;
        _signatureGenerator = signatureGenerator;
    }

    public async Task<AnomalyAuditReport> ProcessEvaluationsAsync(IEnumerable<AnomalyEvaluationResult> evaluations)
    {
        var evals = evaluations.ToList();
        
        // 1. STAGE: Correlation
        var correlatedEvidences = _correlationEngine.CorrelateAndGroup(evals);

        // 2. STAGE: Confidence Reconciliation
        var (finalConfidence, finalSeverity) = _reconciler.Reconcile(evals);

        // 3. STAGE: Deterministic Explanation Mapping
        bool isAnyConfirmedAnomaly = evals.Any(e => e.IsAnomaly);
        var explanation = _explanationMapper.MapToDeterministicExplanation(correlatedEvidences, isAnyConfirmedAnomaly);

        // 🧩 4. STAGE (NEW): ACITVE RECALL (Memory Augmentation)
        // Geçici bir RootCauseResultDto simüle ederek embedding üretiyoruz
        // (Gerçek analiz Replay-time'da ama 'Recall' anlık olmalı)
        var tempRootCause = new RootCauseResultDto(Guid.Empty, explanation, new List<string>(), new List<RootCauseEvidenceDto>(), new List<AblationInsightDto>(), finalConfidence);
        
        // Rapor oluşturmadan önce 'Ön-Rapor' ile embedding üret
        var reportBase = new AnomalyAuditReport(isAnyConfirmedAnomaly, finalSeverity, finalConfidence, evals, explanation, DateTime.UtcNow);
        var vector = _signatureGenerator.GenerateReasoningVector(reportBase, tempRootCause);
        var signature = _signatureGenerator.GenerateCausalHash(tempRootCause);

        var similarDecisions = await _memoryStore.RecallSimilarAsync(vector, signature);

        if (similarDecisions.Any(sd => sd.HybridScore > 0.85))
        {
            var match = similarDecisions.First();
            explanation += $" | [Consistency] Bu desen %{match.HybridScore * 100:F0} oranında geçmişteki bir vaka ile örtüşüyor.";
        }

        // FINAL: IMMUTABLE AUDIT REPORT (Memory Augmented)
        return new AnomalyAuditReport(
            IsConfirmedAnomaly: isAnyConfirmedAnomaly,
            FinalSeverity: finalSeverity,
            AggregateConfidence: finalConfidence,
            RuleEvaluations: evals,
            MappedExplanation: explanation,
            GeneratedAt: DateTime.UtcNow,
            SimilarDecisions: similarDecisions.Select(s => new SemanticRecallResult(s.Knowledge.AnomalyId, s.Knowledge.Summary, s.HybridScore, s.CosineSimilarity, s.CausalOverlap)).ToList()
        );
    }
}
