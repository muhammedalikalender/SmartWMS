namespace SmartWMS.Application.Features.Telemetry.Commands.IngestTelemetry;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmartWMS.Application.Common.Interfaces;
using SmartWMS.Domain.Entities;
using SmartWMS.Domain.ValueObjects;

public class IngestTelemetryCommandHandler : IRequestHandler<IngestTelemetryCommand, bool>
{
    private readonly IShelfRepository _shelfRepository;
    private readonly ISensorSnapshotRepository _snapshotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IngestTelemetryCommandHandler(
        IShelfRepository shelfRepository,
        ISensorSnapshotRepository snapshotRepository,
        IUnitOfWork unitOfWork)
    {
        _shelfRepository = shelfRepository;
        _snapshotRepository = snapshotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(IngestTelemetryCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Telemetry;

        // 1. MAPPING VERIFICATION (Gelen Dış Veriyi Saf Sınır İhlallerine Karşı Koruma)
        // Burada Value Object'lerin constructor kurallarıyla (Invariant) veriyi kontrol ediyoruz.
        // Hatalı/Bozuk bir değer(örn. negatif kütle) gelirse Domain kendiliğinden koruma sağlayacak.
        var sensedMass = new Mass(dto.TotalMass);
        var sensedStability = new StabilityIndex(dto.StabilityIndex);

        // 2. LOAD AGGREGATE
        var shelf = await _shelfRepository.GetByIdAsync(dto.ShelfId, cancellationToken);
        if (shelf == null)
        {
            // Dijital İkiz platformlarında gelen sensör verisinin raf karşılığı yoksa anomali sayılabilir.
            // Şimdilik demo/mimari uyumluluk adına fırlatıyoruz, production'da 'UnknownSensorRegistry' eventine düşebilir.
            throw new InvalidOperationException($"Şistemde kaydı olmayan bir raftan sensör verisi geldi: {dto.ShelfId}");
        }

        // 3. BEHAVIOR (Sensör durumunu Digital Twin'deki raf eşleniğine bildir)
        // Bu metod içinde DomainEvent (ShelfStabilityChanged) ateşlenmiş olacak.
        shelf.ApplySensorStability(sensedStability);

        // Not: Kütle (Mass) sapmalarını (Divergence = Phantom Inventory) vs. rule tabanlı anomaly engine 
        // daha sonra devreye girip event-based tespit edeceği için burada snapshot'ı tutuyoruz.

        // 4. PERSIST SENSOR LOG
        var snapshot = new SensorSnapshot(shelf.Id, sensedMass, sensedStability);
        await _snapshotRepository.AddAsync(snapshot, cancellationToken);

        // 5. COMMIT & EVENT DISPATCH
        // Değişiklikleri veritabanına yolluyoruz. EF Core tetikleyicisi (Interceptor) tam bu noktada 
        // araya girip Shelf içerisindeki Event'leri MediatR ile publish edecek.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
