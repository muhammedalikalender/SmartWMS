namespace SmartWMS.Shared.DTOs;

using System;
using SmartWMS.Shared.Enums;

public class SensorTelemetryDTO
{
    public Guid ShelfId { get; set; }
    public double TotalMass { get; set; }
    public double StabilityIndex { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime Timestamp { get; set; }
}
