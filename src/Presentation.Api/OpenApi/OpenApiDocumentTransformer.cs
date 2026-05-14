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

            The JWT Token Generator API provides a minimal and robust endpoint for generating JSON Web Tokens (JWT) for authenticated users. 
            It is designed to be used in a microservice architecture, where authentication and user information are managed by separate services.

            With this API, you can:
            - Generate a JWT token containing user claims such as userId, role, email, firstName, and lastName
            - Integrate secure token generation into your authentication flow
            - Enable stateless authentication for frontend and other services

            This API is intended to be called by an authentication gateway, which collects user information from relevant microservices and requests a signed JWT for downstream use.
            """;

        return Task.CompletedTask;
    }
}
