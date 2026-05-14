namespace Application.Outputs;

public sealed record LoginUser(string Id, string Email, IReadOnlyCollection<string> Roles);
