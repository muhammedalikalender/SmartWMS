namespace SmartWMS.Application.Features.Telemetry.Commands.IngestTelemetry;

using System;
using MediatR;
using SmartWMS.Shared.DTOs;

public class IngestTelemetryCommand : IRequest<bool>
{
    public SensorTelemetryDTO Telemetry { get; }

    public IngestTelemetryCommand(SensorTelemetryDTO telemetry)
    {
        Telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
    }
}
