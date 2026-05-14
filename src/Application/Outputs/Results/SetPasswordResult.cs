namespace Application.Outputs;

public sealed record SetPasswordResult(SetPasswordStatus Status, string? ErrorMessage = null);
