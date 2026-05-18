namespace Application.Inputs;

public record VerifyPasswordRequest(
    string Email,
    string Password);