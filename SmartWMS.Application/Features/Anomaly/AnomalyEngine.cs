namespace SmartWMS.Application.Features.Anomaly;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.Models;
using SmartWMS.Application.Features.Anomaly.Rules;
using SmartWMS.Application.Features.Anomaly.Orchestrator;

public class AnomalyEngine : IAnomalyEngine
{
    private readonly IEnumerable<IAnomalyRule> _rules;
    private readonly IAnomalyOrchestrator _orchestrator;

    public AnomalyEngine(IEnumerable<IAnomalyRule> rules, IAnomalyOrchestrator orchestrator)
    {
        _rules = rules;
        _orchestrator = orchestrator;
    }

    public async Task<AnomalyAuditReport> EvaluateAllRulesAsync(AnomalyContext context, IEnumerable<string>? ignoredRuleIds = null)
    {
        if (context == null) 
            throw new ArgumentNullException(nameof(context));

        var results = new List<AnomalyEvaluationResult>();
        var ignoredSet = ignoredRuleIds?.ToHashSet() ?? new HashSet<string>();

        // Rule pipeline koşturuluyor
        foreach (var rule in _rules.OrderBy(r => r.Priority))
        {
            if (ignoredSet.Contains(rule.RuleId))
                continue;

            var result = await rule.EvaluateAsync(context);
            results.Add(result);
        }

        // Nihai Karar ve Açıklama için 3-Aşamalı Deterministik Orkestratörü çalıştır
        return await _orchestrator.ProcessEvaluationsAsync(results);
    }
}
