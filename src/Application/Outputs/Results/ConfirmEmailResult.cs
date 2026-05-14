namespace Application.Outputs;

public sealed record ConfirmEmailResult(ConfirmEmailStatus Status, string? ErrorMessage = null);
