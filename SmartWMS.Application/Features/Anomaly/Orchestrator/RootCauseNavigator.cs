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
        // 1. BASELINE REPLAY (Original Decision Context)
        var alert = await _anomalyRepository.GetByIdAsync(alertId, cancellationToken);
        if (alert == null) throw new InvalidOperationException("Anomali kaydı bulunamadı.");

        // Context'i canlandırıyoruz
        var replayResult = await _replayService.ReplayDecisionAsync(alertId, cancellationToken);
        var baseReport = replayResult.ReplayedReport;
        var context = replayResult.ReplayedContext;

        var causalNodeIds = new List<string>();
        var ablationInsights = new List<AblationInsightDto>();

        // 🟢 Sadece trigger olan (IsAnomaly: true) kuralları test edersek yeterli
        var activeRules = baseReport.RuleEvaluations.Where(r => r.IsAnomaly).ToList();

        // 2. COUNTERFACTUAL SIMULATION (Ablation Study)
        foreach (var rule in activeRules)
        {
            // "Eğer bu kural olmasaydı ne olurdu?" (Masking)
            var reportWithoutRule = await _engine.EvaluateAllRulesAsync(context, new[] { rule.RuleId });

            double severityDrop = baseReport.FinalSeverity - reportWithoutRule.FinalSeverity;
            bool isFlipping = baseReport.IsConfirmedAnomaly && !reportWithoutRule.IsConfirmedAnomaly;

            var insight = new AblationInsightDto(
                NodeId: $"rule-{rule.RuleId}",
                Label: rule.RuleName,
                ImpactOnScore: severityDrop,
                IsFlippingNode: isFlipping,
                CounterfactualSummary: isFlipping 
                    ? $"[FLIP] Bu kural çıkarıldığında karar 'Healthy'ye dönüyor. Bu bir NECESSARY CAUSE (Gerekli Sebep) düğümüdür." 
                    : $"Kuralın etkisi var ancak tek başına kararı değiştirmiyor."
            );

            ablationInsights.Add(insight);

            // 🎯 Minimal Sufficient Set (Kök Sebep) tespiti
            // Eğer kural çıkarıldığında karar değişiyorsa (flipping) bu bir kök sebeptir.
            if (isFlipping)
            {
                causalNodeIds.Add($"rule-{rule.RuleId}");
                // Kurala bağlı evidence node'larını da Causal Path'e ekleyebiliriz (Görsellik için)
                causalNodeIds.Add($"event-{alertId}"); 
                causalNodeIds.Add("context-freeze");
                causalNodeIds.Add("score-node");
            }
        }

        // 3. FINAL SYNTHESIS
        string primarySummary = causalNodeIds.Any() 
            ? $"ANALİZ TAMAMLANDI: Kararı tetikleyen {causalNodeIds.Count(id => id.StartsWith("rule"))} ana kural '{string.Join(", ", ablationInsights.Where(i => i.IsFlippingNode).Select(i => i.Label))}' kök sebep olarak saptandı."
            : "Kararı tek bir kural kontrol etmiyor, birden fazla kuralın ortak (ensemble) etkisi baskın.";

        return new RootCauseResultDto(
            alertId,
            causalNodeIds.Distinct().ToList(),
            ablationInsights,
            primarySummary,
            ConfidenceOfCausality: causalNodeIds.Any() ? 0.95 : 0.60
        );
    }
}
