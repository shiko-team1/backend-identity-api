namespace Application.Outputs;

public sealed record CreateUserResult(
    CreateUserStatus Status,
    string? UserId = null,
    string? Email = null,
    string? Role = null,
    string? ErrorMessage = null
    );
