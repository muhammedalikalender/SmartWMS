namespace SmartWMS.Application.Features.Anomaly.Orchestrator;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SmartWMS.Application.Common.Interfaces;
using SmartWMS.Application.Features.Anomaly.Models;
using SmartWMS.Domain.Common;
using SmartWMS.Domain.Entities;
using SmartWMS.Shared.Enums;

public interface IAnomalyPipelineDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

public class AnomalyPipelineDispatcher : IAnomalyPipelineDispatcher
{
    private readonly IAnomalyEngine _anomalyEngine;
    private readonly IShelfRepository _shelfRepository;
    private readonly ISensorSnapshotRepository _snapshotRepository;
    private readonly IAnomalyRepository _anomalyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AnomalyPipelineDispatcher(
        IAnomalyEngine anomalyEngine,
        IShelfRepository shelfRepository,
        ISensorSnapshotRepository snapshotRepository,
        IAnomalyRepository anomalyRepository,
        IUnitOfWork unitOfWork)
    {
        _anomalyEngine = anomalyEngine;
        _shelfRepository = shelfRepository;
        _snapshotRepository = snapshotRepository;
        _anomalyRepository = anomalyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // 1. DYNAMIC CONTEXT ASSEMBLY
        // Staff-Level Note: Dispatcher sadece veriyi toplar (Extraction). 
        // Gerçek karar Engine katmanında verilir.
        
        Guid shelfId = GetShelfIdFromEvent(domainEvent);
        if (shelfId == Guid.Empty) return;

        var shelf = await _shelfRepository.GetByIdAsync(shelfId, cancellationToken);
        var lastSnapshot = await _snapshotRepository.GetLatestByShelfIdAsync(shelfId, cancellationToken);

        if (shelf == null || lastSnapshot == null) return;

        var context = new AnomalyContext(
            ShelfSnapshot: shelf,
            LastSensorData: lastSnapshot,
            EvaluationTriggerType: InferTransactionType(domainEvent)
        );

        // 2. TRIGGER ENGINE (Pure Stateless Computation)
        var auditReport = await _anomalyEngine.EvaluateAllRulesAsync(context);

        // 3. PERSIST AUDIT & SNAPSHOT (Immutable Replay Store)
        if (auditReport.IsConfirmedAnomaly)
        {
            // SNAPSHOT ASSEMBLY
            var snapshot = new AnomalyContextSnapshot(
                SchemaVersion: "1.0",
                RequestId: Guid.NewGuid(),
                ShelfId: shelfId,
                SensedMass: lastSnapshot.TotalMass.Kilograms,
                SensedStability: lastSnapshot.StabilityIndex.Value,
                TriggerType: context.EvaluationTriggerType,
                CapturedAt: DateTime.UtcNow,
                DeterministicHash: GenerateDeterministicHash(context)
            );

            var alert = new AnomalyAlert(
                shelfId: shelfId,
                sourceEventId: domainEvent.EventId,
                category: "AutomaticDispatch",
                severity: auditReport.FinalSeverity,
                confidence: auditReport.AggregateConfidence,
                auditReportJson: JsonSerializer.Serialize(auditReport),
                contextSnapshotJson: JsonSerializer.Serialize(snapshot),
                deterministicHash: snapshot.DeterministicHash
            );

            await _anomalyRepository.AddAsync(alert, cancellationToken);
            
            // 🧠 4. COGNITIVE STORAGE: Store in Semantic Memory
            if (alert.Severity > 0.1)
            {
                var rootCause = await _navigator.AnalyzeRootCauseAsync(alert.Id, cancellationToken);
                var vector = _signatureGenerator.GenerateReasoningVector(auditReport, rootCause);
                var sig = _signatureGenerator.GenerateCausalHash(rootCause);

                var knowledge = new DecisionKnowledge(
                    alert.Id, vector, sig, alert.Category + ": " + auditReport.MappedExplanation, 
                    alert.Severity, alert.Severity > 0.3
                );

                await _memoryStore.StoreDecisionAsync(knowledge);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private string GenerateDeterministicHash(AnomalyContext context)
    {
        // Staff-Level Note: Gerçekte SHA256 veya benzeri bir algoritma ile 
        // context içeriği (Shelf state + Sensor + Trigger) hash'lenmelidir.
        // Şimdilik basit bir string birleştirme simülasyonu yapıyoruz.
        return $"hash_{context.ShelfSnapshot.Id}_{context.LastSensorData.TotalMass.Kilograms}_{DateTime.UtcNow.Ticks}";
    }

    private Guid GetShelfIdFromEvent(IDomainEvent @event)
    {
        // Yansıma (Reflection) veya pattern matching ile shelfId'yi çekiyoruz
        return @event switch
        {
            SmartWMS.Domain.Events.ShelfStabilityChangedDomainEvent e => e.ShelfId,
            SmartWMS.Domain.Events.ItemAddedDomainEvent e => e.ShelfId,
            SmartWMS.Domain.Events.ItemRemovedDomainEvent e => e.ShelfId,
            _ => Guid.Empty
        };
    }

    private TransactionType InferTransactionType(IDomainEvent @event)
    {
        return @event switch
        {
            SmartWMS.Domain.Events.ItemAddedDomainEvent => TransactionType.Inbound,
            SmartWMS.Domain.Events.ItemRemovedDomainEvent => TransactionType.Outbound,
            _ => TransactionType.Internal
        };
    }
}
