namespace OwnDeliveryApiP33.Domain.ValueObjects;

public class Address
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string BuildingNumber { get; set; } = string.Empty;
    public string? ApartmentNumber { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Description { get; set; }

    public Address() { }

    public Address(string city, string street, string buildingNumber, string postalCode, 
        decimal latitude, decimal longitude, string? apartmentNumber = null, string? description = null)
    {
        City = city;
        Street = street;
        BuildingNumber = buildingNumber;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
        ApartmentNumber = apartmentNumber;
        Description = description;
    }
}
