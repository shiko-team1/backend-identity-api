namespace Application.Inputs;

public record VerifyPasswordRequest(
    string UserId,
    string Password);