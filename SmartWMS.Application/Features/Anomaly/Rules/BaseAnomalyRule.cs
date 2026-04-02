namespace SmartWMS.Application.Features.Anomaly.Rules;

using System;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.Enums;
using SmartWMS.Application.Features.Anomaly.Models;

public abstract class BaseAnomalyRule : IAnomalyRule
{
    public abstract string RuleName { get; }
    public abstract string RuleId { get; }
    public abstract string Version { get; }
    public abstract int Priority { get; }
    public abstract AnomalyCategory Category { get; }

    public abstract Task<AnomalyEvaluationResult> EvaluateAsync(AnomalyContext context);

    protected AnomalyEvaluationResult CreateResult(
        bool isAnomaly, 
        double severity, 
        double confidence, 
        IReadOnlyList<AnomalyEvidence> evidences)
    {
        return new AnomalyEvaluationResult
        {
            IsAnomaly = isAnomaly,
            SeverityScore = Math.Clamp(severity, 0.0, 1.0),
            ConfidenceScore = Math.Clamp(confidence, 0.0, 1.0),
            RuleId = RuleId,
            RuleVersion = Version,
            RuleName = RuleName,
            Category = Category,
            Evidences = evidences ?? new List<AnomalyEvidence>()
        };
    }
}
