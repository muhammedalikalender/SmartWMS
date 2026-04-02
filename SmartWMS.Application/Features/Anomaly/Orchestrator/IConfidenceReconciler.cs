namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System.Collections.Generic;
using SmartWMS.Application.Features.Anomaly.Models;

public interface IConfidenceReconciler
{
    /// <summary>
    /// Kurallardan gelen farklı güven skorlarını (Confidence) ve ağırlıkları (Weight) 
    /// uzlaştırarak tek bir nihai güven ve şiddet skoru üretir.
    /// Çelişen kurallar arasındaki çatışmaları çözer (Conflict Resolution).
    /// </summary>
    (double AggregateConfidence, double FinalSeverity) Reconcile(IEnumerable<AnomalyEvaluationResult> evaluations);
}
