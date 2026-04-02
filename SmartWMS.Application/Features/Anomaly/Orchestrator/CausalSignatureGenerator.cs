namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using SmartWMS.Application.Features.Anomaly.Models;

public interface ICausalSignatureGenerator
{
    float[] GenerateReasoningVector(AnomalyAuditReport report, RootCauseResultDto rootCause);
    string GenerateCausalHash(RootCauseResultDto rootCause);
}

public class CausalSignatureGenerator : ICausalSignatureGenerator
{
    // Kararı 16-boyutlu bir nedensel vektöre indirger (Simülasyon amaçlı Staff-Level model)
    public float[] GenerateReasoningVector(AnomalyAuditReport report, RootCauseResultDto rootCause)
    {
        var vector = new float[16];
        
        // 1. Severity & Confidence Base (Slots 0-1)
        vector[0] = (float)report.FinalSeverity;
        vector[1] = (float)report.AggregateConfidence;

        // 2. Rule Activation Map (Slots 2-10)
        // Kural ID'lerine göre ağırlıklandırma (Deterministic Encoding)
        foreach (var evaluation in report.RuleEvaluations.Take(8))
        {
            int index = Math.Abs(evaluation.RuleId.GetHashCode()) % 8 + 2;
            vector[index] = (float)evaluation.SeverityScore;
        }

        // 3. Evidence Consistency (Slots 11-15)
        foreach (var evidence in rootCause.CriticalEvidences.Take(5))
        {
            int index = Math.Abs(evidence.SignalType.GetHashCode()) % 5 + 11;
            vector[index] = (float)evidence.ContributionScore;
        }

        return Normalize(vector);
    }

    public string GenerateCausalHash(RootCauseResultDto rootCause)
    {
        // Intersection over Union (IoU) hesaplamaları için 
        // sıralı ve mühürlü bir kural yolu imzası üretir.
        var sb = new StringBuilder();
        foreach (var nodeId in rootCause.CausalNodeIds.OrderBy(id => id))
        {
            sb.Append(nodeId).Append("|");
        }
        return sb.ToString();
    }

    private float[] Normalize(float[] v)
    {
        float norm = (float)Math.Sqrt(v.Sum(x => x * x));
        if (norm < 0.0001) return v;
        return v.Select(x => x / norm).ToArray();
    }
}
