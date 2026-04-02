namespace SmartWMS.Application.Features.Anomaly.Models;

using System;
using System.Collections.Generic;

public record AnomalyAuditReport(
    bool IsConfirmedAnomaly,
    double FinalSeverity,
    double AggregateConfidence,
    IReadOnlyList<AnomalyEvaluationResult> RuleEvaluations,
    string MappedExplanation,
    DateTime GeneratedAt
);
