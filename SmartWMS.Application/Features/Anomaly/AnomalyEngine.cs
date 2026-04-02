namespace SmartWMS.Application.Features.Anomaly;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartWMS.Application.Features.Anomaly.Models;
using SmartWMS.Application.Features.Anomaly.Rules;

public class AnomalyEngine : IAnomalyEngine
{
    private readonly IEnumerable<IAnomalyRule> _rules;

    public AnomalyEngine(IEnumerable<IAnomalyRule> rules)
    {
        _rules = rules;
    }

    public async Task<IEnumerable<AnomalyEvaluationResult>> EvaluateAllRulesAsync(AnomalyContext context)
    {
        if (context == null) 
            throw new ArgumentNullException(nameof(context));

        var results = new List<AnomalyEvaluationResult>();

        // Rule pipeline koşturuluyor (Priority sıralamasına göre çalışır)
        foreach (var rule in _rules.OrderBy(r => r.Priority))
        {
            var result = await rule.EvaluateAsync(context);
            results.Add(result);
            
            // Opsiyonel: Priority 1 olan bir kural 'Kritik Sistem Hatası (örn: Sensör Offline)' 
            // verirse pipeline kırılabilir (Chain of Responsibility), ancak Digital Twin'de 
            // Composite skor istendiği için tüm raporları toplamak genelde daha iyidir.
        }

        return results;
    }
}
