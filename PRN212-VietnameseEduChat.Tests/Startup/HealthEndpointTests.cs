using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Tests.TestInfrastructure;

namespace PRN212_VietnameseEduChat.Tests.Startup;

public sealed class HealthEndpointTests : IClassFixture<HealthEndpointTests.Factory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(Factory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_endpoint_returns_success(string path)
    {
        var response = await _client.GetAsync(path);

        response.EnsureSuccessStatusCode();
    }

    public sealed class Factory : WebApplicationFactory<Program>
    {
        private readonly SqlServerTestDatabase _database =
            SqlServerTestDatabase.Create();

        private readonly Dictionary<string, string?> _previousEnvironment =
            new();

        public Factory()
        {
            _database
                .CreateEmptyDatabaseAsync()
                .GetAwaiter()
                .GetResult();

            SetEnvironment(
                "ConnectionStrings__DefaultConnection",
                _database.ConnectionString);
            SetEnvironment("Database__AutoMigrate", "false");
            SetEnvironment("DemoData__Enabled", "false");
            SetEnvironment("VnPay__PaymentUrl", "https://sandbox.example/pay");
            SetEnvironment("VnPay__PublicBaseUrl", "https://app.example");
            SetEnvironment("VnPay__TmnCode", "TEST");
            SetEnvironment("VnPay__HashSecret", "test-secret");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(Environments.Development);
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        _database.ConnectionString,
                    ["Database:AutoMigrate"] = "false",
                    ["DemoData:Enabled"] = "false",
                    ["VnPay:PaymentUrl"] = "https://sandbox.example/pay",
                    ["VnPay:PublicBaseUrl"] = "https://app.example",
                    ["VnPay:TmnCode"] = "TEST",
                    ["VnPay:HashSecret"] = "test-secret"
                });
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.RemoveAll<IHostedService>();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(_database.ConnectionString));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                RestoreEnvironment();
                _database.Dispose();
            }
        }

        private void SetEnvironment(string key, string value)
        {
            _previousEnvironment[key] =
                Environment.GetEnvironmentVariable(key);
            Environment.SetEnvironmentVariable(key, value);
        }

        private void RestoreEnvironment()
        {
            foreach (var item in _previousEnvironment)
            {
                Environment.SetEnvironmentVariable(
                    item.Key,
                    item.Value);
            }
        }
    }
}
