namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartWMS.Application.Features.Anomaly.Models;

public class ExplanationMapper : IExplanationMapper
{
    public string MapToDeterministicExplanation(IEnumerable<AnomalyEvidence> correlatedEvidences, bool isAnomaly)
    {
        if (!isAnomaly) return "Sistem normal operasyonel sınırlarda.";

        var sb = new StringBuilder();
        sb.AppendLine("DENETLENEBİLİR KARAR RAPORU:");

        // 1. FIXED TEMPLATES (Deterministik eşleme)
        // Staff-Level Note: Serbest metin yok, sadece sinyal tiplerine atanmış sabit şablonlar.
        foreach (var ev in correlatedEvidences.OrderByDescending(x => x.Weight))
        {
            var message = ev.SignalType switch
            {
                "MassDrop" => $"[KRİTİK] Kütlede belirlenen toleransın ötesinde azalma saptandı. (Sapma: %{ev.Deviation*100:F1})",
                "StabilitySpike" => "[UYARI] Alışılmadık fiziksel sarsıntı/ivmelenme ölçüldü.",
                "TemporalDivergence" => "[BİLGİ] Veri akışında zamansal tutarsızlık izlendi.",
                _ => $"[TEKNİK] Belirlenemeyen sinyal tipi: {ev.SignalType} (Değer: {ev.Value})"
            };
            
            sb.AppendLine($"- {message}");
        }

        return sb.ToString();
    }
}
