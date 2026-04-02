namespace SmartWMS.Domain.ValueObjects;

public record Mass
{
    public double Kilograms { get; init; }

    public Mass(double kilograms)
    {
        if (kilograms < 0)
            throw new ArgumentException("Kütle negatif olamaz.", nameof(kilograms));
            
        Kilograms = kilograms;
    }

    public static Mass Zero => new(0);

    public static Mass operator -(Mass a, Mass b)
    {
        var result = a.Kilograms - b.Kilograms;
        return new Mass(result < 0 ? 0 : result);
    }

    public static Mass operator +(Mass a, Mass b)
        => new(a.Kilograms + b.Kilograms);
}