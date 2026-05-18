namespace Application.Inputs;

public record ChangePasswordRequest(
    string Email,
    string CurrentPassword,
    string NewPassword);