namespace OwnDeliveryApiP33.Application.DTOs;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);
