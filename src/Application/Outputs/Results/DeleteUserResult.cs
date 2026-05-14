namespace Application.Outputs;

public sealed record DeleteUserResult(DeleteUserStatus Status, string? ErrorMessage = null);
