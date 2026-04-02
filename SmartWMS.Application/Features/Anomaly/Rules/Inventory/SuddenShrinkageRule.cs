namespace SmartWMS.Application.Features.Anomaly.Rules.Inventory;

using System;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.DataProviders;
using SmartWMS.Application.Features.Anomaly.Enums;
using SmartWMS.Application.Features.Anomaly.Models;
using SmartWMS.Shared.Enums;

public class SuddenShrinkageRule : BaseAnomalyRule
{
    public override string RuleName => "Sudden Shrinkage Detection";
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
            // 6. Severity (Şiddet) Hesaplaması: Düşüş miktarı kütle kaybı oranıyla orantılı
            double dropRatio = massDifference.Kilograms / baselineMass.Kilograms;
            
            // Eğer %100 düşüş olursa severity 1.0 (Critical)
            double severity = dropRatio;

            // 7. Composite Contextual Verification
            // Eğer ortamda sarsıntı/vibrasyon da ölçüldüyse (Stability Index düşükse) hırsızlıktan ziyade palet düşmesi/çarpma riski artar.
            // (Composite Engine ile desteklenecek olsa da burada skor esnekliği katarız)
            string reason = "Kütlede ani ve faturasız azalma tespit edildi. Kritik stok kaçağı!";
            if (context.LastSensorData.StabilityIndex.IsCritical)
            {
                reason += " Olası Devrilme/Düşme Olayı: Sarsıntı indeksi oldukça düşük tespit edildi.";
                severity = Math.Min(severity * 1.5, 1.0); // Şiddeti çarparak max 1.0'a kilitler
            }

            return CreateResult(isAnomaly: true, score: severity, reason: reason);
        }

        return AnomalyEvaluationResult.Healthy(RuleName, Category);
    }
}
