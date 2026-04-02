namespace SmartWMS.Core.Controllers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.Application.Features.Anomaly.Orchestrator;

[ApiController]
[Route("api/anomalies")]
public class DecisionGraphController : ControllerBase
{
    private readonly IDecisionGraphBuilder _graphBuilder;

    public DecisionGraphController(IDecisionGraphBuilder graphBuilder)
    {
        _graphBuilder = graphBuilder;
    }

    /// <summary>
    /// Herhangi bir anomalinin nasıl saptandığının ve hangi kanıtların 
    /// karara etki ettiğinin 'Graph' (Çizge) şemasını döner.
    /// </summary>
    [HttpGet("{id}/decision-graph")]
    public async Task<IActionResult> GetDecisionGraph(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var graph = await _graphBuilder.BuildGraphAsync(id, cancellationToken);
            return Ok(graph);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Grafik üretimi sırasında hata oluştu: {ex.Message}");
        }
    }
}
