using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.HostedServices;

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
        private readonly SqliteConnection _connection = new("Data Source=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _connection.Open();
            builder.UseEnvironment(Environments.Development);
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=Unused",
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
                services.AddSingleton(_connection);
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(_connection));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection.Dispose();
            }
        }
    }
}
