namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System;
using System.Collections.Generic;
using System.Linq;
using SmartWMS.Application.Features.Anomaly.Models;

public class CorrelationEngine : ICorrelationEngine
{
    public IEnumerable<AnomalyEvidence> CorrelateAndGroup(IEnumerable<AnomalyEvaluationResult> evaluations)
    {
        // 1. Tüm kurallardan gelen 'Evidences' listesini topla
        var allEvidences = evaluations.SelectMany(e => e.Evidences).ToList();

        // 2. SIGNAL GROUPING & CLUSTERING:
        // Aynı SignalType'lı verileri grupla. (Örn: Birden fazla kural 'Mass' sinyali veriyorsa birleşir)
        // Staff-Level Note: Burada temporal alignment (zamansal hizalama) yapılabilir 
        // (örneğin 1 sn içindeki sinyaller tek bir 'Observation' sayılabilir).
        
        var grouped = allEvidences
            .GroupBy(ev => ev.SignalType)
            .Select(g => new AnomalyEvidence(
                g.Key,
                g.Average(v => v.Value),
                g.Average(v => v.BaselineValue),
                g.Average(v => v.Deviation),
                g.Max(v => v.Weight), // En etkili olan sinyalin ağırlığı korunur
                g.Max(v => v.Timestamp)
            ));

        return grouped;
    }
}
