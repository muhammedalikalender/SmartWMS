namespace SmartWMS.Application.Common.Interfaces;

using System;
using SmartWMS.Application.Features.Anomaly.Models;

public interface IDecisionGraphCache
{
    DecisionGraphDto? Get(Guid anomalyId);
    void Set(Guid anomalyId, DecisionGraphDto graph);
    void Remove(Guid anomalyId);
}
