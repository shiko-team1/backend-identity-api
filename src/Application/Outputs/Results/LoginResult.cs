namespace Application.Outputs;

public sealed record LoginResult(LoginStatus Status, LoginUser? User = null, string? ErrorMessage = null);
