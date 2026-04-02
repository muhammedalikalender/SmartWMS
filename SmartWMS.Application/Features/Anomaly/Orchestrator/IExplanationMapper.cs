namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System.Collections.Generic;
using SmartWMS.Application.Features.Anomaly.Models;

public interface IExplanationMapper
{
    /// <summary>
    /// Yapılandırılmış kanıtları (Structured Evidence) kullanarak 
    /// tamamen deterministik (sabit şablonlar üzerinden) ve denetlenebilir
    /// bir açıklama metni oluşturur. Serbest akışlı veya AI üretimli metin barındırmaz.
    /// </summary>
    string MapToDeterministicExplanation(IEnumerable<AnomalyEvidence> correlatedEvidences, bool isAnomaly);
}
