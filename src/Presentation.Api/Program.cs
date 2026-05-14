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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Infrastructure.Identity.Data.AppIdentityDbContext>();
    db.Database.Migrate();
}

await IdentityInitializer.SeedAsync(app.Services, builder.Configuration);

app.UseCors("Frontend");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApiEndpoints();
app.MapAuthEndpoints();
app.MapAdminEndpoints();

app.Run();
