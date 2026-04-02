namespace SmartWMS.Domain.Entities;

using System;
using SmartWMS.Domain.Common;
using SmartWMS.Domain.ValueObjects;

public class AnomalyAlert : Entity
{
    public Guid ShelfId { get; private set; }
    
    // Anomaliye neden olan kök olay kimliği (Traceability)
    public Guid SourceEventId { get; private set; }
    
    public string Category { get; private set; }
    public double Severity { get; private set; }
    public double Confidence { get; private set; }
    
    // Deterministik açıklama ve kanıtların o anki kopyası (Audit Snapshot)
    public string AuditReportJson { get; private set; }
    
    public DateTime DetectedOn { get; private set; }
    
    public bool IsResolved { get; private set; }
    public DateTime? ResolvedOn { get; private set; }

    // EF Core
    protected AnomalyAlert() { }

    public AnomalyAlert(
        Guid shelfId, 
        Guid sourceEventId, 
        string category, 
        double severity, 
        double confidence, 
        string auditReportJson)
    {
        Id = Guid.NewGuid();
        ShelfId = shelfId;
        SourceEventId = sourceEventId;
        Category = category;
        Severity = severity;
        Confidence = confidence;
        AuditReportJson = auditReportJson;
        DetectedOn = DateTime.UtcNow;
        IsResolved = false;
    }

    public void Resolve()
    {
        if (IsResolved) return;
        
        IsResolved = true;
        ResolvedOn = DateTime.UtcNow;
        
        // Opsiyonel: AddDomainEvent(new AnomalyResolvedDomainEvent(this.Id));
    }
}
