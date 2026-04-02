namespace SmartWMS.Application.Features.Anomaly.Rules;

using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.Models;

public interface IAnomalyRule
{
    string RuleName { get; }
    string RuleId { get; }
    string Version { get; }
    int Priority { get; }
    
    Task<AnomalyEvaluationResult> EvaluateAsync(AnomalyContext context);
}
