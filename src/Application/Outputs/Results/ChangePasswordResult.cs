namespace Application.Outputs;

public sealed record ChangePasswordResult(ChangePasswordStatus Status, string? ErrorMessage = null);
