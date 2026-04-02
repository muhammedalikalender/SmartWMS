namespace SmartWMS.Application.Features.Anomaly.Rules;

using System;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.Enums;
using SmartWMS.Application.Features.Anomaly.Models;

public abstract class BaseAnomalyRule : IAnomalyRule
{
    public abstract string RuleName { get; }
    public abstract int Priority { get; }
    public abstract AnomalyCategory Category { get; }

    public abstract Task<AnomalyEvaluationResult> EvaluateAsync(AnomalyContext context);

    protected AnomalyEvaluationResult CreateResult(bool isAnomaly, double score, string reason)
    {
        return new AnomalyEvaluationResult
        {
            IsAnomaly = isAnomaly,
            SeverityScore = Math.Clamp(score, 0.0, 1.0),
            RuleName = RuleName,
            Category = Category,
            Reason = reason
        };
    }
}
