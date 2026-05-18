using Application.Outputs.Statuses;

namespace Application.Outputs;

public sealed record VerifyPasswordResult(VerifyPasswordStatus Status, string? ErrorMessage = null);
