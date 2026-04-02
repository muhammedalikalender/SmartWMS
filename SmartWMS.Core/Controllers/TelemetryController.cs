namespace SmartWMS.Core.Controllers;

using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.Application.Features.Telemetry.Commands.IngestTelemetry;
using SmartWMS.Shared.DTOs;

[ApiController]
[Route("api/gateway")]
public class TelemetryController : ControllerBase
{
    private readonly ISender _mediator;

    public TelemetryController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("telemetry")]
    public async Task<IActionResult> IngestTelemetry([FromBody] SensorTelemetryDTO telemetryDto)
    {
        // 1. Gelen veriyi validasyon borusuna (MediatR Command) koy.
        var command = new IngestTelemetryCommand(telemetryDto);

        // 2. Application Layer'daki iş mantığına havale et.
        var result = await _mediator.Send(command);

        if (result)
        {
            return Ok(new { Message = "Telemetry ingested successfully." });
        }

        return BadRequest(new { Message = "Failed to ingest telemetry." });
    }
}
