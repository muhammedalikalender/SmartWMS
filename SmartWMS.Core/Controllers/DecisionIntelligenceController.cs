namespace SmartWMS.Core.Controllers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.Application.Features.Anomaly.Orchestrator;
using SmartWMS.Application.Features.Anomaly.Models;

[ApiController]
[Route("api/intelligence")]
public class DecisionIntelligenceController : ControllerBase
{
    private readonly IDecisionGraphBuilder _graphBuilder;
    private readonly IAnomalyReplayService _replayService;
    private readonly IDecisionDiffEngine _diffEngine;
    private readonly IRootCauseNavigator _navigator;
    private readonly IDecisionGraphCache _cache;

    public DecisionIntelligenceController(
        IDecisionGraphBuilder graphBuilder,
        IAnomalyReplayService replayService,
        IDecisionDiffEngine diffEngine,
        IRootCauseNavigator navigator,
        IDecisionGraphCache cache)
    {
        _graphBuilder = graphBuilder;
        _replayService = replayService;
        _diffEngine = diffEngine;
        _navigator = navigator;
        _cache = cache;
    }

    [HttpGet("mirror/{id}")]
    public async Task<IActionResult> GetDecisionMirror(Guid id, CancellationToken cancellationToken)
    {
        // 🚀 PRODUCT MIRROR: Returns Graph + Root Cause + Memory Insights in one call
        var cachedGraph = _cache.Get(id);
        if (cachedGraph != null) return Ok(cachedGraph);

        try
        {
            var replayResult = await _replayService.ReplayDecisionAsync(id, cancellationToken);
            var graph = await _graphBuilder.BuildGraphAsync(id, replayResult.OriginalReport.SimilarDecisions, cancellationToken);
            
            _cache.Set(id, graph);
            return Ok(graph);
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    [HttpGet("analyze/{id}/root-cause")]
    public async Task<IActionResult> GetRootCause(Guid id, CancellationToken cancellationToken)
    {
        var result = await _navigator.AnalyzeRootCauseAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("compare")]
    public async Task<IActionResult> CompareDecisions([FromQuery] Guid baseId, [FromQuery] Guid compareId, CancellationToken cancellationToken)
    {
        var diff = await _diffEngine.CompareAsync(baseId, compareId, cancellationToken);
        return Ok(diff);
    }

    [HttpGet("stats")]
    public IActionResult GetCognitiveStats()
    {
        // 🚀 KPI HUB: Simulation of real-time stats
        return Ok(new
        {
            TotalAnomaliesProcessed = 142,
            MemoryHitRate = 0.94,
            AverageResolutionTimeMinutes = 8.4,
            BrainHealthScore = 0.98
        });
    }
}
