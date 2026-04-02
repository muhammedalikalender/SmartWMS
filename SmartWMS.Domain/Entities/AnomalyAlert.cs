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
    
    // Karar anındaki girdi verileri (Replay Snapshot)
    public string ContextSnapshotJson { get; private set; }

    // Replay bütünlük kontrolü için hash
    public string DeterministicHash { get; private set; }
    
    public DateTime DetectedOn { get; private set; }
    
    public bool IsResolved { get; private set; }
    public DateTime? ResolvedOn { get; private set; }
    
    // Snapshot yapısındaki değişiklikleri yönetmek için (Versioning)
    public string SchemaVersion { get; private set; } = "1.0.0";

    // EF Core
    protected AnomalyAlert() { }

    public AnomalyAlert(
        Guid shelfId, 
        Guid sourceEventId, 
        string category, 
        double severity, 
        double confidence, 
        string auditReportJson,
        string contextSnapshotJson,
        string deterministicHash)
    {
        Id = Guid.NewGuid();
        ShelfId = shelfId;
        SourceEventId = sourceEventId;
        Category = category;
        Severity = severity;
        Confidence = confidence;
        AuditReportJson = auditReportJson;
        ContextSnapshotJson = contextSnapshotJson;
        DeterministicHash = deterministicHash;
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
