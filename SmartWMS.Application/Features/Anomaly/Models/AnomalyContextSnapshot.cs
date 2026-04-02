namespace SmartWMS.Application.Features.Anomaly.Models;

using System;
using SmartWMS.Shared.Enums;

public record AnomalyContextSnapshot(
    string SchemaVersion,
    Guid RequestId,
    Guid ShelfId,
    double SensedMass,
    double SensedStability,
    TransactionType TriggerType,
    DateTime CapturedAt,
    string DeterministicHash
);
