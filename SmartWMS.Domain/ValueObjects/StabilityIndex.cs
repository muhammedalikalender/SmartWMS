namespace SmartWMS.Domain.ValueObjects;

public record StabilityIndex(double Value)
{
    public static StabilityIndex Critical => new(0);
    public static StabilityIndex Stable => new(1);

    public bool IsCritical => Value < 0.3;
}