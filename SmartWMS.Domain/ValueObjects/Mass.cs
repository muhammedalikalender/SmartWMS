namespace SmartWMS.Domain.ValueObjects;

public record Mass(double Kilograms)
{
    public static Mass Zero => new(0);

    public static Mass operator -(Mass a, Mass b)
        => new(a.Kilograms - b.Kilograms);

    public static Mass operator +(Mass a, Mass b)
        => new(a.Kilograms + b.Kilograms);
}