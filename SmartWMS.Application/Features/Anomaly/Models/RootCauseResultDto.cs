namespace SmartWMS.Application.Features.Anomaly.Models;

using System;
using System.Collections.Generic;

public record RootCauseResultDto(
    Guid AnomalyId,
    string PrimaryCause, // "Mass drop + instability" gibi ana neden özeti
    List<string> CausalNodeIds, // Karar grafiği üzerindeki 'Vazgeçilmez' düğümler
    List<RootCauseEvidenceDto> CriticalEvidences, // En kritik sinyallerin listesi
    List<AblationInsightDto> AblationInsights, // Guided Ablation testi detayları
    double Confidence // %94 vb. Nedensellik güveni
);

public record RootCauseEvidenceDto(
    string SignalType,
    double ContributionScore,
    bool IsCritical
);

public record AblationInsightDto(
    string NodeId,
    string Label,
    double ContributionScore, // Başlangıçtaki etki puanı
    double ImpactOnScore, // Ablasyon sonrası skor değişimi (Delta)
    bool IsFlippingNode, // Kararı flip eden Necessary Cause mu?
    string CounterfactualSummary
);
