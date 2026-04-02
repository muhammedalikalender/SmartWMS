namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.Models;

public interface IDecisionGraphBuilder
{
    Task<DecisionGraphDto> BuildGraphAsync(Guid alertId, CancellationToken cancellationToken = default);
}

public class DecisionGraphBuilder : IDecisionGraphBuilder
{
    private readonly IAnomalyReplayService _replayService;

    public DecisionGraphBuilder(IAnomalyReplayService replayService)
    {
        _replayService = replayService;
    }

    public async Task<DecisionGraphDto> BuildGraphAsync(Guid alertId, CancellationToken cancellationToken = default)
    {
        // 1. RE-EXECUTE PIPELINE (Decision Replay)
        var replayResult = await _replayService.ReplayDecisionAsync(alertId, cancellationToken);
        var report = replayResult.ReplayedReport;

        var nodes = new List<DecisionNodeDto>();
        var edges = new List<DecisionEdgeDto>();

        // 🟢 LEVEL 1: EVENT NODE
        var eventNodeId = $"event-{replayResult.AlertId}";
        nodes.Add(new DecisionNodeDto(
            Id: eventNodeId,
            Type: "EventNode",
            Label: "Trigger Event",
            Metadata: new Dictionary<string, object> { ["AlertId"] = replayResult.AlertId }
        ));

        // 🔵 LEVEL 2: CONTEXT NODE
        var contextNodeId = "context-freeze";
        nodes.Add(new DecisionNodeDto(
            Id: contextNodeId,
            Type: "ContextNode",
            Label: "Decision Context (Snapshot)",
            Metadata: new Dictionary<string, object> { ["IsReproduced"] = replayResult.IsReproduced }
        ));
        
        edges.Add(new DecisionEdgeDto($"e1", eventNodeId, contextNodeId, "CONTEXTUALIZED", "Freeze Context"));

        // 🟡 LEVEL 3: RULE NODES (Parallel Execution)
        int ruleIndex = 0;
        foreach (var ruleEval in report.RuleEvaluations)
        {
            var ruleNodeId = $"rule-{ruleEval.RuleId}";
            nodes.Add(new DecisionNodeDto(
                Id: ruleNodeId,
                Type: "RuleNode",
                Label: ruleEval.RuleName,
                Metadata: new Dictionary<string, object> { ["Version"] = ruleEval.RuleVersion, ["IsAnomaly"] = ruleEval.IsAnomaly }
            ));

            edges.Add(new DecisionEdgeDto($"e-rule-{ruleIndex}", contextNodeId, ruleNodeId, "EVALUATED_BY", "Execute Rule"));

            // ⚪ LEVEL 4: EVIDENCE NODES
            int evidenceIndex = 0;
            foreach (var evidence in ruleEval.Evidences)
            {
                var evidenceNodeId = $"{ruleNodeId}-ev-{evidenceIndex}";
                nodes.Add(new DecisionNodeDto(
                    Id: evidenceNodeId,
                    Type: "EvidenceNode",
                    Label: $"{evidence.SignalType}: {evidence.Value}",
                    Metadata: new Dictionary<string, object> { ["Deviation"] = evidence.Deviation, ["Weight"] = evidence.Weight }
                ));

                edges.Add(new DecisionEdgeDto($"e-ev-{ruleIndex}-{evidenceIndex}", ruleNodeId, evidenceNodeId, "PRODUCED", "Generate Evidence"));
                evidenceIndex++;
            }
            ruleIndex++;
        }

        // 🔴 LEVEL 5: SCORE & RECONCILIATION
        var scoreNodeId = "final-score";
        nodes.Add(new DecisionNodeDto(
            Id: scoreNodeId,
            Type: "ScoreNode",
            Label: $"Score: {report.FinalSeverity:P1}",
            Metadata: new Dictionary<string, object> { ["Severity"] = report.FinalSeverity, ["Confidence"] = report.AggregateConfidence }
        ));

        // Kurallardan skora bağlantılar (Contribution)
        foreach (var ruleEval in report.RuleEvaluations)
        {
            var ruleNodeId = $"rule-{ruleEval.RuleId}";
            edges.Add(new DecisionEdgeDto($"e-score-{ruleNodeId}", ruleNodeId, scoreNodeId, "CONTRIBUTED_TO", "Reconcile Score"));
        }

        // 🟣 LEVEL 6: EXPLANATION (The Result)
        var explanationNodeId = "explanation-node";
        nodes.Add(new DecisionNodeDto(
            Id: explanationNodeId,
            Type: "ExplanationNode",
            Label: "Final Audit Conclusion",
            Metadata: new Dictionary<string, object> { ["Explanation"] = report.MappedExplanation }
        ));

        edges.Add(new DecisionEdgeDto("e-final", scoreNodeId, explanationNodeId, "FINALIZED_AS", "Map Explanation"));

        return new DecisionGraphDto(nodes, edges);
    }
}
