namespace SmartWMS.Application.Features.Anomaly;

using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.Models;

public interface IAnomalyEngine
{
    Task<IEnumerable<AnomalyEvaluationResult>> EvaluateAllRulesAsync(AnomalyContext context);
}
