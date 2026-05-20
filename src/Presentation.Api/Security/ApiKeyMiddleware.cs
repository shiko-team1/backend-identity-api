using Microsoft.Extensions.Options;

namespace Identity.Api.Security;

public sealed class ApiKeyMiddleware(
    IOptions<ApiKeyOptions> options,
    ILogger<ApiKeyMiddleware> logger) : IEndpointFilter
{

    private readonly ApiKeyOptions _options = options.Value;

    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (string.IsNullOrWhiteSpace(_options.Value))
        {
            logger.LogWarning(
                "API key rejected because ApiKey:Value is not configured. HeaderName={HeaderName}",
                _options.HeaderName);

            AddDiagnosticHeaders(context, "missing-config");
            return ValueTask.FromResult<object?>(Results.Unauthorized());
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(_options.HeaderName, out var providedApiKey))
        {
            logger.LogWarning(
                "API key rejected because request header is missing. HeaderName={HeaderName}",
                _options.HeaderName);

            AddDiagnosticHeaders(context, "missing-header");
            return ValueTask.FromResult<object?>(Results.Unauthorized());
        }

        if (!string.Equals(providedApiKey, _options.Value, StringComparison.Ordinal))
        {
            logger.LogWarning(
                "API key rejected because request header value does not match configured value. HeaderName={HeaderName}",
                _options.HeaderName);

            AddDiagnosticHeaders(context, "value-mismatch");
            return ValueTask.FromResult<object?>(Results.Unauthorized());
        }

        return next(context);
    }

    private void AddDiagnosticHeaders(EndpointFilterInvocationContext context, string reason)
    {
        context.HttpContext.Response.Headers["X-Api-Key-Diagnostic"] = reason;
        context.HttpContext.Response.Headers["X-Api-Key-Header-Name"] = _options.HeaderName;
    }
}
