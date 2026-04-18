namespace OwnDeliveryApiP33.Domain.ValueObjects;

public class Dimensions
{
    public decimal Width { get; set; }  // см
    public decimal Length { get; set; } // см
    public decimal Height { get; set; } // см

    public Dimensions() { }

    public Dimensions(decimal width, decimal length, decimal height)
    {
        Width = width;
        Length = length;
        Height = height;
    }

    public decimal GetVolume() => Width * Length * Height;
}
