using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Presentation.Api.OpenApi;

public sealed class OpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Info.Description = """
            ## Introduction

            The Identity API provides endpoints for user authentication and identity management within the system.
            It enables clients to register new users, authenticate existing users, and manage user credentials and profile information.

            With this API, you can:
            - Register new users
            - Authenticate users and validate credentials
            - Retrieve user identity and profile details
            - Manage user account information

            This API serves as the central authority for user authentication and identity data, supporting secure and flexible integration with other services in your application architecture.
            """;

        return Task.CompletedTask;
    }
}
