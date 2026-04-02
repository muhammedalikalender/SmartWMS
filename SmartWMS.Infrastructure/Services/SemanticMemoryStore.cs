namespace SmartWMS.Infrastructure.Services;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartWMS.Application.Common.Interfaces;
using SmartWMS.Application.Features.Anomaly.Models;
using SmartWMS.Domain.Entities;

public interface ISemanticMemoryStore
{
    Task StoreDecisionAsync(DecisionKnowledge knowledge);
    Task<List<SemanticRecallResult>> RecallSimilarAsync(float[] queryVector, string causalSignature, int topK = 3);
}

public class SemanticMemoryStore : ISemanticMemoryStore
{
    private static readonly List<DecisionKnowledge> _store = new(); // In-Memory Semantic Store

    public Task StoreDecisionAsync(DecisionKnowledge knowledge)
    {
        _store.Add(knowledge);
        return Task.CompletedTask;
    }

    public Task<List<SemanticRecallResult>> RecallSimilarAsync(float[] queryVector, string causalSignature, int topK = 3)
    {
        var results = new List<SemanticRecallResult>();

        foreach (var entry in _store)
        {
            double similarity = CosineSimilarity(queryVector, entry.ReasoningVector);
            double overlap = CalculateCausalOverlap(causalSignature, entry.CausalPathSignature);

            // 🚀 HYBRID SCORING: Similarity * 40% + Causal Overlap * 60%
            double hybridScore = (similarity * 0.4) + (overlap * 0.6);

            if (hybridScore > 0.5) // Eşik değer
            {
                results.Add(new SemanticRecallResult(
                    AnomalyId: entry.AnomalyId,
                    Summary: entry.Summary,
                    HybridScore: hybridScore,
                    CosineSimilarity: similarity,
                    CausalOverlap: overlap
                ));
            }
        }

        return Task.FromResult(results.OrderByDescending(r => r.HybridScore).Take(topK).ToList());
    }

    private double CosineSimilarity(float[] v1, float[] v2)
    {
        float dot = v1.Zip(v2, (a, b) => a * b).Sum();
        return dot; // Normalize edildiği varsayıldı
    }

    private double CalculateCausalOverlap(string sig1, string sig2)
    {
        // Intersection over Union (IoU) of Rule/Causal Path nodes
        var set1 = sig1.Split('|', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var set2 = sig2.Split('|', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        if (!set1.Any() || !set2.Any()) return 0;

        int intersection = set1.Intersect(set2).Count();
        int union = set1.Union(set2).Count();

        return (double)intersection / union;
    }
}
