namespace SmartWMS.Application.Features.Anomaly.Models;

using System.Collections.Generic;

public record DecisionGraphDto(
    List<DecisionNodeDto> Nodes,
    List<DecisionEdgeDto> Edges
);

public record DecisionNodeDto(
    string Id,
    string Type, // Event, Context, Rule, Evidence, Score, Explanation
    string Label,
    Dictionary<string, object> Metadata
);

public record DecisionEdgeDto(
    string Id,
    string SourceId,
    string TargetId,
    string Type, // TRIGGERED, CONTEXTUALIZED, EVALUATED_BY, PRODUCED, CONTRIBUTED_TO, AGGREGATED_IN, FINALIZED_AS
    string Label
);
