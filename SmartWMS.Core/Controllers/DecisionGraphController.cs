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
    private readonly IAnomalyReplayService _replayService;
    private readonly IDecisionGraphCache _cache;

    public DecisionGraphController(
        IDecisionGraphBuilder graphBuilder, 
        IAnomalyReplayService replayService,
        IDecisionGraphCache cache)
    {
        _graphBuilder = graphBuilder;
        _replayService = replayService;
        _cache = cache;
    }

    /// <summary>
    /// Herhangi bir anomalinin nasıl saptandığının ve hangi kanıtların 
    /// karara etki ettiğinin 'Graph' (Çizge) şemasını döner.
    /// </summary>
    [HttpGet("{id}/decision-graph")]
    public async Task<IActionResult> GetDecisionGraph(Guid id, CancellationToken cancellationToken)
    {
        // 1. CACHE-FIRST CHECK (Production Performance)
        var cachedGraph = _cache.Get(id);
        if (cachedGraph != null) return Ok(cachedGraph);

        try
        {
            // 2. RE-EXECUTE REPLAY (If not in cache)
            var replayResult = await _replayService.ReplayDecisionAsync(id, cancellationToken);
            
            // 3. BUILD GRAPH (Passing Historical Memory Insights)
            var graph = await _graphBuilder.BuildGraphAsync(id, replayResult.OriginalReport.SimilarDecisions, cancellationToken);

            // 4. PERSIST TO CACHE
            _cache.Set(id, graph);

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
