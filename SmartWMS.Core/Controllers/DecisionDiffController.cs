namespace SmartWMS.Core.Controllers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.Application.Features.Anomaly.Orchestrator;

[ApiController]
[Route("api/anomalies")]
public class DecisionDiffController : ControllerBase
{
    private readonly IDecisionDiffEngine _diffEngine;

    public DecisionDiffController(IDecisionDiffEngine diffEngine)
    {
        _diffEngine = diffEngine;
    }

    /// <summary>
    /// İki anomali kararını (Geçmiş vs Şimdi) karşılaştırarak sapmaları (Drift) döner.
    /// Hangi kuralın fikrinin değiştiğini ve hangi kanıtın saptığını raporlar.
    /// </summary>
    [HttpGet("compare")]
    public async Task<IActionResult> CompareDecisions(
        [FromQuery] Guid baseId, 
        [FromQuery] Guid compareId, 
        CancellationToken cancellationToken)
    {
        try
        {
            var diff = await _diffEngine.CompareAsync(baseId, compareId, cancellationToken);
            return Ok(diff);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Karşılaştırma sırasında hata oluştu: {ex.Message}");
        }
    }
}
