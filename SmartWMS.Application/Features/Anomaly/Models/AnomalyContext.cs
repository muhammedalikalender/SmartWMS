namespace SmartWMS.Application.Features.Anomaly.Models;

using System;
using SmartWMS.Domain.Entities;
using SmartWMS.Shared.Enums;

/// <summary>
/// AnomalyContext is completely immutable. Rules cannot modify the state, 
/// preventing race conditions and keeping execution purely deterministic.
/// </summary>
public record AnomalyContext(
    Shelf ShelfSnapshot, 
    SensorSnapshot LastSensorData,
    TransactionType EvaluationTriggerType
);
