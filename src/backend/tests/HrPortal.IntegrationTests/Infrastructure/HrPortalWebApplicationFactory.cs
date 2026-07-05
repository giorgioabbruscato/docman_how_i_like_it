using HrPortal.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HrPortal.IntegrationTests.Infrastructure;

public class HrPortalWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected virtual IReadOnlyDictionary<string, string?>? ConfigOverrides => null;

    public HrPortalWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Testing",
                ["Database:ConnectionString"] = "DataSource=:memory:",
                ["Storage:RootPath"] = Path.Combine(Path.GetTempPath(), $"hrportal-test-{Guid.NewGuid():N}")
            };

            if (ConfigOverrides is not null)
            {
                foreach (var (key, value) in ConfigOverrides)
                    settings[key] = value;
            }

            config.AddInMemoryCollection(settings);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<HrPortalDbContext>>();
            services.RemoveAll<HrPortalDbContext>();
            services.RemoveAll<DbContext>();

            services.AddDbContext<HrPortalDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddScoped<DbContext>(sp => sp.GetRequiredService<HrPortalDbContext>());

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName,
                _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _connection.Dispose();

        base.Dispose(disposing);
    }
}
