using Identity.Api.Security;
using Microsoft.Extensions.Options;

namespace Presentation.Api.Endpoints;

public static class DebugEndpoints
{
    public static void MapDebugEndpoints(this WebApplication app)
    {
        app.MapGet("/api/debug/config", (
            IOptions<ApiKeyOptions> apiKeyOptions,
            IConfiguration configuration) =>
        {
            var options = apiKeyOptions.Value;

            return Results.Ok(new
            {
                marker = "api-key-debug-2026-05-20-1",
                apiKeyConfigured = !string.IsNullOrWhiteSpace(options.Value),
                headerName = options.HeaderName,
                seedAdminEmailConfigured = !string.IsNullOrWhiteSpace(configuration["Seed:AdminEmail"]),
                seedAdminPasswordConfigured = !string.IsNullOrWhiteSpace(configuration["Seed:AdminPassword"])
            });
        })
        .WithTags("Debug");
    }
}
