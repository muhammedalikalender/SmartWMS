namespace SmartWMS.Shared.Enums;

public enum TransactionType
{
    Normal = 0,
    Inbound = 1,
    Outbound = 2,
    Relocation = 3,
    Internal = 4,
    AnomalyInjection = 99 
}
