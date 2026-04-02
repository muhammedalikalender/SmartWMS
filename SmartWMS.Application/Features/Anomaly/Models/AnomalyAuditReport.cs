public record AnomalyAuditReport(
    bool IsConfirmedAnomaly,
    double FinalSeverity,
    double AggregateConfidence,
    IReadOnlyList<AnomalyEvaluationResult> RuleEvaluations,
    string MappedExplanation,
    DateTime GeneratedAt,
    IReadOnlyList<SemanticRecallResult>? SimilarDecisions = null // 🚀 SEMANTIC MEMORY INSIGHTS
);

public record SemanticRecallResult(
    Guid AnomalyId,
    string Summary,
    double HybridScore,
    double CosineSimilarity,
    double CausalOverlap
);
