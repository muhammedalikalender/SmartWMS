namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.Models;

public interface IDecisionDiffEngine
{
    Task<DecisionDiffDto> CompareAsync(Guid baseId, Guid compareId, CancellationToken cancellationToken = default);
}

public class DecisionDiffEngine : IDecisionDiffEngine
{
    private readonly IAnomalyRepository _anomalyRepository;

    public DecisionDiffEngine(IAnomalyRepository anomalyRepository)
    {
        _anomalyRepository = anomalyRepository;
    }

    public async Task<DecisionDiffDto> CompareAsync(Guid baseId, Guid compareId, CancellationToken cancellationToken = default)
    {
        // 1. FETCH AUDIT REPORTS
        var baseAlert = await _anomalyRepository.GetByIdAsync(baseId, cancellationToken);
        var compareAlert = await _anomalyRepository.GetByIdAsync(compareId, cancellationToken);

        if (baseAlert == null || compareAlert == null)
            throw new InvalidOperationException("Karşılaştırılacak anomali kayıtları bulunamadı.");

        // Not: AnomalyAlert içinde saklanan JSON raporlarını Deserialize etmemiz gerekiyor.
        // Mock implementasyonda basitleştirelim, gerçekte AuditReport'u serileştiriyoruz.
        var baseReport = System.Text.Json.JsonSerializer.Deserialize<AnomalyAuditReport>(baseAlert.AuditReportJson);
        var compareReport = System.Text.Json.JsonSerializer.Deserialize<AnomalyAuditReport>(compareAlert.AuditReportJson);

        if (baseReport == null || compareReport == null)
            throw new InvalidOperationException("Anomali raporları okunamadı.");

        var ruleDrifts = new List<RuleDriftDto>();
        bool isMaterialChange = false;

        // 2. STRUCTURAL & VALUE COMPARISON (Rule Level)
        var baseRules = baseReport.RuleEvaluations.ToDictionary(r => r.RuleId);
        var compareRules = compareReport.RuleEvaluations.ToDictionary(r => r.RuleId);

        var allRuleIds = baseRules.Keys.Union(compareRules.Keys);

        foreach (var ruleId in allRuleIds)
        {
            var hasBase = baseRules.TryGetValue(ruleId, out var baseRule);
            var hasCompare = compareRules.TryGetValue(ruleId, out var compareRule);

            if (hasBase && hasCompare)
            {
                // Drift in existing rule
                var drfit = CalculateRuleDrift(baseRule!, compareRule!);
                ruleDrifts.Add(drfit);
                if (drfit.SeverityDelta > 0.2 || drfit.ConfidenceDelta > 0.2) isMaterialChange = true;
            }
            else if (!hasBase && hasCompare)
            {
                // NEW RULE triggered
                ruleDrifts.Add(new RuleDriftDto(ruleId, compareRule!.RuleName, "New", compareRule.SeverityScore, compareRule.ConfidenceScore, new List<EvidenceDriftDto>()));
                isMaterialChange = true;
            }
            else if (hasBase && !hasCompare)
            {
                // REMOVED RULE (Stop triggering)
                ruleDrifts.Add(new RuleDriftDto(ruleId, baseRule!.RuleName, "Removed", -baseRule.SeverityScore, -baseRule.ConfidenceScore, new List<EvidenceDriftDto>()));
                isMaterialChange = true;
            }
        }

        // 3. AGGREGATE SUMMARY
        double totalSeverityDelta = compareReport.FinalSeverity - baseReport.FinalSeverity;
        double totalConfidenceDelta = compareReport.AggregateConfidence - baseReport.AggregateConfidence;

        if (Math.Abs(totalSeverityDelta) > 0.3) isMaterialChange = true;

        var summary = isMaterialChange 
            ? $"KRİTİK SAPMA SAPTANDI: Ciddiyet farkı {totalSeverityDelta:P1}, Güven farkı {totalConfidenceDelta:P1}."
            : $"Stabil Karar: Kararlar arasındaki farklar 'Material Change' eşiğinin altında.";

        return new DecisionDiffDto(
            baseId, 
            compareId, 
            totalSeverityDelta, 
            totalConfidenceDelta, 
            isMaterialChange, 
            ruleDrifts, 
            summary
        );
    }

    private RuleDriftDto CalculateRuleDrift(AnomalyEvaluationResult baseRE, AnomalyEvaluationResult compareRE)
    {
        var evidenceDrifts = new List<EvidenceDriftDto>();
        
        // Simple Evidence Pairing logic (SignalType'a göre match)
        var baseEvs = baseRE.Evidences.ToDictionary(e => e.SignalType);
        var compareEvs = compareRE.Evidences.ToDictionary(e => e.SignalType);

        var allSignals = baseEvs.Keys.Union(compareEvs.Keys);
        foreach (var signal in allSignals)
        {
            var hasBase = baseEvs.TryGetValue(signal, out var bEv);
            var hasComp = compareEvs.TryGetValue(signal, out var cEv);

            double bVal = hasBase ? bEv!.Value : 0;
            double cVal = hasComp ? cEv!.Value : 0;
            double delta = cVal - bVal;

            evidenceDrifts.Add(new EvidenceDriftDto(signal, bVal, cVal, delta, Math.Abs(delta) > 0.1));
        }

        return new RuleDriftDto(
            baseRE.RuleId,
            baseRE.RuleName,
            "Modified",
            compareRE.SeverityScore - baseRE.SeverityScore,
            compareRE.ConfidenceScore - baseRE.ConfidenceScore,
            evidenceDrifts
        );
    }
}
