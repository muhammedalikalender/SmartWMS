namespace SmartWMS.Application.Features.Anomaly.Rules.Inventory;

using System;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.DataProviders;
using SmartWMS.Application.Features.Anomaly.Enums;
using SmartWMS.Application.Features.Anomaly.Models;
using SmartWMS.Shared.Enums;

public class SuddenShrinkageRule : BaseAnomalyRule
{
    public override string RuleName => "Ani Kütle Kaybı (Sudden Shrinkage)";
    public override string RuleId => "SR-001";
    public override string Version => "1.0.0";
    public override int Priority => 10;
    public override AnomalyCategory Category => AnomalyCategory.Inventory;

    private readonly IAnomalyDataProvider _dataProvider;

    public SuddenShrinkageRule(IAnomalyDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public override async Task<AnomalyEvaluationResult> EvaluateAsync(AnomalyContext context)
    {
        // 1. Guard Clause: Planlı bir ürün çıkarma (Outbound) veya taşıma (Relocation) ise bu beklenen bir kütle kaybıdır.
        if (context.EvaluationTriggerType == TransactionType.Outbound ||
            context.EvaluationTriggerType == TransactionType.Relocation)
        {
            return AnomalyEvaluationResult.Healthy(RuleName, Category);
        }

        var shelfId = context.ShelfSnapshot.Id;
        var currentMass = context.LastSensorData.TotalMass;

        // 2. Zaman penceresini (Tolerans süresi) belirle
        var analysisWindow = TimeSpan.FromMinutes(2);

        // 3. Geçmiş Data Sağlayıcısından Baseline Kütleyi al
        var baselineMass = await _dataProvider.GetBaselineMassAsync(shelfId, analysisWindow);

        // Eğer kayda değer bir baseline kütlesi yoksa kıyaslanacak bir zemin de yoktur
        if (baselineMass.Kilograms <= 0)
        {
            return AnomalyEvaluationResult.Healthy(RuleName, Category);
        }

        // 4. Kütledeki azalma hesabı (Delta P)
        var massDifference = baselineMass - currentMass;

        // Eğer kütle arttıysa veya değişmediyse shrinkage degildir
        if (massDifference.Kilograms <= 0)
        {
            return AnomalyEvaluationResult.Healthy(RuleName, Category);
        }

        // 5. Dinamik Tolerans Eşiği hesaplanıyor (Çevresel ufak ölçüm sapmalarını pas geçmek için)
        // Hard threshold konmamış; ancak gürültü kesici (noise filter) olarak 0.5 kg veya baseline'ın %1'i seçiliyor.
        double noiseThreshold = Math.Max(0.5, baselineMass.Kilograms * 0.01);

        if (massDifference.Kilograms > noiseThreshold)
        {
            // 6. Evidence Generation (Yapılandırılmış Kanıt Üretimi)
            var evidences = new List<AnomalyEvidence>();
            
            // Temel sinyal: Kütle kaybı
            double dropRatio = massDifference.Kilograms / baselineMass.Kilograms;
            evidences.Add(new AnomalyEvidence(
                SignalType: "MassDrop",
                Value: currentMass.Kilograms,
                BaselineValue: baselineMass.Kilograms,
                Deviation: dropRatio,
                Weight: 0.8,
                Timestamp: DateTime.UtcNow
            ));

            // 7. Confidence & Severity Calculation
            // Makas ne kadar büyükse kararın doğruluğuna o kadar güveniriz (Confidence).
            double confidence = Math.Clamp(dropRatio * 1.5, 0.5, 1.0);
            double severity = dropRatio;

            // 8. Composite Evidence: Sarsıntı/Vibrasyon
            if (context.LastSensorData.StabilityIndex.IsCritical)
            {
                evidences.Add(new AnomalyEvidence(
                    SignalType: "StabilitySpike",
                    Value: context.LastSensorData.StabilityIndex.Value,
                    BaselineValue: 1.0, // Stable baseline
                    Deviation: 1.0 - context.LastSensorData.StabilityIndex.Value,
                    Weight: 0.2, // Şiddet arttırıcı yan sinyal
                    Timestamp: DateTime.UtcNow
                ));
                
                // Eğer sarsıntı varsa şiddeti arttırıyoruz (Digital Twin fiziksel doğruluğu)
                severity = Math.Min(severity * 1.5, 1.0);
            }

            return CreateResult(
                isAnomaly: true, 
                severity: severity, 
                confidence: confidence, 
                evidences: evidences);
        }

        return AnomalyEvaluationResult.Healthy(RuleName, Category);
    }
}
