namespace OwnDeliveryApiP33.Application.DTOs;

public record RegisterCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber);
