namespace SmartWMS.Shared.Enums;

public enum TransactionType
{
    Normal = 0,
    Inbound = 1,
    Outbound = 2,
    Relocation = 3,
    AnomalyInjection = 99 // Simulator'ün kasıtlı zehirli veri ürettiğini belirtebilir
}
