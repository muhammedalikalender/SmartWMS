namespace SmartWMS.Domain.ValueObjects;

public record StabilityIndex
{
    public double Value { get; init; }

    public StabilityIndex(double value)
    {
        if (value < 0.0 || value > 1.0)
            throw new ArgumentOutOfRangeException(nameof(value), "Stabilite indeksi 0 ile 1 arasında olmalıdır.");
            
        Value = value;
    }

    public static StabilityIndex Critical => new(0);
    public static StabilityIndex Stable => new(1);

    public bool IsCritical => Value < 0.3;
}