namespace Application.Inputs;

public record ChangePasswordRequest(
    string UserId,
    string CurrentPassword,
    string NewPassword);