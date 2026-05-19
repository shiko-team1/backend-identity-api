using Identity.Api.Security;
using Infrastructure;
using Infrastructure.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Presentation.Api.Endpoints;
using Presentation.Api.OpenApi;
using Presentation.Api.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCorsConfiguration();
builder.Services.AddOpenApiConfiguration();

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddAuthorization();

builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection(ApiKeyOptions.SectionName));
builder.Services.AddScoped<ApiKeyMiddleware>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Identity.Data.AppIdentityDbContext>();
    if (app.Environment.IsEnvironment("Testing"))
    {
        db.Database.EnsureCreated();
    }
    else
    {
        db.Database.Migrate();
    }
}

await IdentityInitializer.SeedAsync(app.Services, builder.Configuration);

app.UseCors("AuthGateway");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApiEndpoints();
app.MapAuthEndpoints();
app.MapAdminEndpoints();

app.Run();

public partial class Program;
