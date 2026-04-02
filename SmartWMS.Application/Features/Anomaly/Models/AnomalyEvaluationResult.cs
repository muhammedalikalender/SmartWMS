namespace SmartWMS.Application.Features.Anomaly.Models;

using System;
using System.Collections.Generic;

public class AnomalyEvaluationResult
{
    public bool IsAnomaly { get; init; }
    
    // 0.0 (Temiz) -> 1.0 (Kritik Anomali)
    public double SeverityScore { get; init; }
    
    public string RuleName { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;

    // ML entegrasyonu için confidence veya ek metadatalar (örn: Hızlanma trend verileri)
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

    public static AnomalyEvaluationResult Healthy(string ruleName) 
        => new() { IsAnomaly = false, SeverityScore = 0, RuleName = ruleName, Reason = "Normal" };
}
