namespace OwnDeliveryApiP33.Domain.ValueObjects;

public class Location
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? Accuracy { get; set; }
    public decimal? Altitude { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public decimal? Speed { get; set; }

    public Location() { }

    public Location(decimal latitude, decimal longitude, decimal? accuracy = null, 
        decimal? altitude = null, decimal? speed = null)
    {
        Latitude = latitude;
        Longitude = longitude;
        Accuracy = accuracy;
        Altitude = altitude;
        Speed = speed;
        Timestamp = DateTime.UtcNow;
    }
}
