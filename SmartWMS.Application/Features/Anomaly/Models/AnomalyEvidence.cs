namespace SmartWMS.Application.Features.Anomaly.Models;

using System;

public record AnomalyEvidence(
    string SignalType,
    double Value,
    double BaselineValue,
    double Deviation,
    double Weight,
    DateTime Timestamp
);
