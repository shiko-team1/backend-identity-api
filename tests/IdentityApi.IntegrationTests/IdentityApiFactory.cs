using Infrastructure.Identity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityApi.IntegrationTests;

public sealed class IdentityApiFactory : WebApplicationFactory<Program>
{
    public const string ApiKey = "integration-test-api-key";
    private readonly string _databaseName = $"IdentityApiTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiKey:HeaderName"] = "X-API-KEY",
                ["ApiKey:Value"] = ApiKey,
                ["ConnectionStrings:SqlServer"] = "Server=(localdb)\\mssqllocaldb;Database=IdentityApiTests;Trusted_Connection=True;"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<AppIdentityDbContext>>();
            services.RemoveAll<DbContextOptions<AppIdentityDbContext>>();
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
