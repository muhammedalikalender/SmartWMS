namespace SmartWMS.Application.Features.Anomaly.Models;

using System;
using System.Collections.Generic;
using SmartWMS.Application.Features.Anomaly.Enums;

public class AnomalyEvaluationResult
{
    public bool IsAnomaly { get; init; }
    
    public AnomalyCategory Category { get; init; } = AnomalyCategory.None;
    
    // 0.0 (Temiz) -> 1.0 (Kritik Anomali)
    public double SeverityScore { get; init; }

    // Kuralın kendi kararına olan güven seviyesi (0.0 - 1.0)
    public double ConfidenceScore { get; init; }
    
    public string RuleId { get; init; } = string.Empty;
    public string RuleVersion { get; init; } = string.Empty;
    public string RuleName { get; init; } = string.Empty;

    // Şema bağımlı makinece okunabilir kanıtlar (Signals, Temporal Data vb.)
    public IReadOnlyList<AnomalyEvidence> Evidences { get; init; } = new List<AnomalyEvidence>();

    // ML entegrasyonu için ek metadatalar
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

    public static AnomalyEvaluationResult Healthy(string ruleName, AnomalyCategory category) 
        => new() 
        { 
            IsAnomaly = false, 
            SeverityScore = 0, 
            ConfidenceScore = 1.0,
            RuleName = ruleName, 
            Category = category, 
            Evidences = new List<AnomalyEvidence>() 
        };
}
