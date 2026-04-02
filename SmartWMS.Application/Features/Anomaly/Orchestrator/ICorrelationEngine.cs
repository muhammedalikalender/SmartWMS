namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System.Collections.Generic;
using SmartWMS.Application.Features.Anomaly.Models;

public interface ICorrelationEngine
{
    /// <summary>
    /// Farklı kurallardan gelen kanıtları (Evidence) gruplar, 
    /// zamansal hizalamayı (Temporal Alignment) yapar ve sinyalleri kümeleyerek
    /// ortak kök nedenleri (Clustering) ortaya çıkarır.
    /// </summary>
    IEnumerable<AnomalyEvidence> CorrelateAndGroup(IEnumerable<AnomalyEvaluationResult> evaluations);
}
