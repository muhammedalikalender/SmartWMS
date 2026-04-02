namespace SmartWMS.Core.Controllers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.Application.Features.Anomaly.Orchestrator;

[ApiController]
[Route("api/anomalies")]
public class RootCauseController : ControllerBase
{
    private readonly IRootCauseNavigator _navigator;

    public RootCauseController(IRootCauseNavigator navigator)
    {
        _navigator = navigator;
    }

    /// <summary>
    /// Bir anomalinin 'Kök Sebep' (Root Cause) analizini yapar. 
    /// 'Eğer bu kural/kanıt olmasaydı karar değişir miydi?' simülasyonu 
    /// (Counterfactual) sonucunu döner.
    /// </summary>
    [HttpGet("{id}/root-cause")]
    public async Task<IActionResult> GetRootCause(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _navigator.AnalyzeRootCauseAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Kök sebep analizi sırasında hata oluştu: {ex.Message}");
        }
    }
}
