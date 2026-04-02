namespace SmartWMS.Application.Features.Anomaly.Models;

using System;
using System.Collections.Generic;

public record RootCauseResultDto(
    Guid AnomalyId,
    List<string> CausalNodeIds, // Karar grafiği üzerindeki 'Vazgeçilmez' düğümler
    List<AblationInsightDto> AblationInsights, // Ablation testi detayları
    string PrimaryCauseExplanation,
    double ConfidenceOfCausality // 0.0 - 1.0 (Nedensellik güveni)
);

public record AblationInsightDto(
    string NodeId,
    string Label,
    double ImpactOnScore, // Bu kural çıkarıldığında skordaki değişim
    bool IsFlippingNode, // Kararı anomali -> sağlıklıya çeviren düğüm mü?
    string CounterfactualSummary
);
