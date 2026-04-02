namespace SmartWMS.Domain.Entities;

using System;
using SmartWMS.Domain.Common;

public class DecisionKnowledge : Entity
{
    public Guid AnomalyId { get; private set; }
    
    // Kararın nedensel yolunu temsil eden vektör (Embedding)
    public float[] ReasoningVector { get; private set; }
    
    // Hızlı karşılaştırma için nedensel hash (Intersection over Union için)
    public string CausalPathSignature { get; private set; }
    
    public string Summary { get; private set; }
    public double Severity { get; private set; }
    public bool IsAnomaly { get; private set; }
    
    public DateTime StoredOn { get; private set; }

    // EF Core
    protected DecisionKnowledge() { }

    public DecisionKnowledge(
        Guid anomalyId, 
        float[] reasoningVector, 
        string causalPathSignature, 
        string summary, 
        double severity, 
        bool isAnomaly)
    {
        Id = Guid.NewGuid();
        AnomalyId = anomalyId;
        ReasoningVector = reasoningVector;
        CausalPathSignature = causalPathSignature;
        Summary = summary;
        Severity = severity;
        IsAnomaly = isAnomaly;
        StoredOn = DateTime.UtcNow;
    }
}
