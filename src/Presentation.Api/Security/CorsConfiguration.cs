namespace Presentation.Api.Security;

public static class CorsConfiguration
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AuthGateway", policy =>
            {
                policy
                    .WithOrigins("https://shiko-team1-backend-auth-gateway.azurewebsites.net", "https://127.0.0.1:5500", "https://localhost:3000", "https://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });


        return services;
    }
}
