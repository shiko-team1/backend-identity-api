using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Identity.Api.Security;

public sealed class ApiKeyMiddleware(IOptions<ApiKeyOptions> options) : IEndpointFilter
{

    private readonly ApiKeyOptions _options = options.Value;

    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (string.IsNullOrWhiteSpace(_options.Value))
        {
            return ValueTask.FromResult<object?>(Results.Unauthorized());
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(_options.HeaderName, out var providedApiKey))
        {
            return ValueTask.FromResult<object?>(Results.Unauthorized());
        }

        if (!string.Equals(providedApiKey, _options.Value, StringComparison.Ordinal))
        {
            return ValueTask.FromResult<object?>(Results.Unauthorized());
        }

        return next(context);
    }
}
