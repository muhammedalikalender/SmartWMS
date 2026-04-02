namespace SmartWMS.Application.Features.Anomaly.Models;

using System;
using System.Collections.Generic;

public record DecisionDiffDto(
    Guid BaseAnomalyId,
    Guid CompareAnomalyId,
    double TotalSeverityDelta,
    double TotalConfidenceDelta,
    bool IsMaterialChange, // Karar sonucunu etkileyen kritik bir fark mı?
    List<RuleDriftDto> RuleDrifts,
    string DiffSummary
);

public record RuleDriftDto(
    string RuleId,
    string RuleName,
    string DriftStatus, // New, Removed, Modified, Unchanged
    double SeverityDelta,
    double ConfidenceDelta,
    List<EvidenceDriftDto> EvidenceDrifts
);

public record EvidenceDriftDto(
    string SignalType,
    double BaseValue,
    double CompareValue,
    double ValueDelta,
    bool IsSignificant // Bu sinyaldeki sapma kritik mi?
);
