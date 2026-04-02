namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.Models;

public interface IRootCauseNavigator
{
    Task<RootCauseResultDto> AnalyzeRootCauseAsync(Guid alertId, CancellationToken cancellationToken = default);
}

public class RootCauseNavigator : IRootCauseNavigator
{
    private readonly IAnomalyRepository _anomalyRepository;
    private readonly IAnomalyEngine _engine;
    private readonly IAnomalyReplayService _replayService;

    public RootCauseNavigator(
        IAnomalyRepository anomalyRepository, 
        IAnomalyEngine engine, 
        IAnomalyReplayService replayService)
    {
        _anomalyRepository = anomalyRepository;
        _engine = engine;
        _replayService = replayService;
    }

    public async Task<RootCauseResultDto> AnalyzeRootCauseAsync(Guid alertId, CancellationToken cancellationToken = default)
    {
        // 1. BASELINE REPLAY (Full Decision Context)
        var replayResult = await _replayService.ReplayDecisionAsync(alertId, cancellationToken);
        var baseReport = replayResult.ReplayedReport;
        var context = replayResult.ReplayedContext;

        if (!baseReport.IsConfirmedAnomaly)
            throw new InvalidOperationException("Anomali olmayan bir kayıt için kök sebep analizi yapılamaz.");

        var activeRules = baseReport.RuleEvaluations.Where(r => r.IsAnomaly).ToList();
        
        // 🚀 GUIDED ABLATION: STEP 1 - RANKING (Heuristic Pruning)
        // Etki puanı (Severity * Confidence) yüksekten düşüğe doğru sıralıyoruz.
        // Ama eleme yaparken EN DÜŞÜK etkiden başlayacağız (Greedy Elimination).
        var rankedItems = activeRules
            .OrderBy(r => r.SeverityScore * r.ConfidenceScore) // En düşük etkiliden başla
            .ToList();

        var currentIgnoredIds = new List<string>();
        var necessaryCauseIds = new List<string>();
        var ablationInsights = new List<AblationInsightDto>();

        // 🚀 GUIDED ABLATION: STEP 2 & 3 - ITERATIVE PRUNING & FLIP CHECK
        foreach (var rule in rankedItems)
        {
            // Bu kuralı ve daha önce elenmiş gereksizleri çıkararak simüle et
            var testIgnoredIds = currentIgnoredIds.Concat(new[] { rule.RuleId }).ToList();
            var reportAfterPruning = await _engine.EvaluateAllRulesAsync(context, testIgnoredIds);

            bool stillAnomaly = reportAfterPruning.IsConfirmedAnomaly;

            if (stillAnomaly)
            {
                // Karar DEĞİŞMEDİ -> Bu kural 'Gürültü' (Noise). Kalıcı olarak eleyebiliriz.
                currentIgnoredIds.Add(rule.RuleId);
                
                ablationInsights.Add(new AblationInsightDto(
                    $"rule-{rule.RuleId}", rule.RuleName, rule.SeverityScore, 0, false, 
                    "Sistemden çıkarıldığında karar DEĞİŞMEDİ. Bu bir 'Side-Effect'tir."
                ));
            }
            else
            {
                // Karar DEĞİŞTİ (Flipped) -> Kararı ayakta tutan asıl sebeplerden biri!
                // Bu kuralı 'Necessary Cause' olarak işaretle ve maskeleme listesine EKLEME!
                necessaryCauseIds.Add(rule.RuleId);

                ablationInsights.Add(new AblationInsightDto(
                    $"rule-{rule.RuleId}", rule.RuleName, rule.SeverityScore, baseReport.FinalSeverity - reportAfterPruning.FinalSeverity, true,
                    "[NECESSARY CAUSE] Bu kural olmasaydı karar 'Healthy'ye dönecekti."
                ));
            }
        }

        // 🚀 STEP 4: MINIMAL SUFFICIENT SET EXTRACTION
        var causalNodeIds = new List<string> { $"event-{alertId}", "context-freeze", "score-node", "explanation-node" };
        foreach (var id in necessaryCauseIds) causalNodeIds.Add($"rule-{id}");

        var criticalEvidences = activeRules
            .Where(r => necessaryCauseIds.Contains(r.RuleId))
            .SelectMany(r => r.Evidences.Select(e => new RootCauseEvidenceDto(e.SignalType, r.SeverityScore, true)))
            .ToList();

        string primaryCauseSummary = necessaryCauseIds.Count switch {
            0 => "Kök sebep belirsiz (karmaşık kural etkileşimi).",
            1 => $"Ana Kök Sebep: {activeRules.First(r => r.RuleId == necessaryCauseIds[0]).RuleName}",
            _ => $"Bileşik Nedensellik: {string.Join(" + ", activeRules.Where(r => necessaryCauseIds.Contains(r.RuleId)).Select(r => r.RuleName))}"
        };

        return new RootCauseResultDto(
            alertId,
            primaryCauseSummary,
            causalNodeIds,
            criticalEvidences,
            ablationInsights,
            Confidence: necessaryCauseIds.Any() ? 0.94 : 0.40
        );
    }
}
