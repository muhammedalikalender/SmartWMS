namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System;
using System.Collections.Generic;
using System.Linq;
using SmartWMS.Application.Features.Anomaly.Models;

public class ConfidenceReconciler : IConfidenceReconciler
{
    public (double AggregateConfidence, double FinalSeverity) Reconcile(IEnumerable<AnomalyEvaluationResult> evaluations)
    {
        var activeEvaluations = evaluations.Where(e => e.IsAnomaly).ToList();
        
        if (!activeEvaluations.Any())
            return (1.0, 0.0);

        // 1. CONFIDENCE RECONCILIATION:
        // Kurallardan gelen confidence skorlarının ağırlıklı ortalamasını alıyoruz.
        // Düşük confidence'lı kurallar ana skoru aşağı çekerek sistemin 'Emin Değilim' demesini sağlar.
        double totalWeightedConfidence = activeEvaluations.Sum(e => e.ConfidenceScore * e.SeverityScore);
        double totalSeverity = activeEvaluations.Sum(e => e.SeverityScore);

        double aggregateConfidence = totalSeverity > 0 
            ? totalWeightedConfidence / totalSeverity 
            : activeEvaluations.Average(e => e.ConfidenceScore);

        // 2. SEVERITY NORMALIZATION:
        // En yüksek kural şiddeti (Max Severity) ana şiddeti belirler.
        double finalSeverity = activeEvaluations.Max(e => e.SeverityScore);

        return (Math.Clamp(aggregateConfidence, 0, 1), Math.Clamp(finalSeverity, 0, 1));
    }
}
