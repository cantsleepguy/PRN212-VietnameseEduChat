using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.DataAccess.Context;

namespace PRN212_VietnameseEduChat.Tests.TestInfrastructure;

internal sealed class SqlServerTestDatabase : IDisposable, IAsyncDisposable
{
    public const string ConnectionStringEnvironmentVariable =
        "PRN212_TEST_SQLSERVER_CONNECTION_STRING";

    private readonly string _masterConnectionString;
    private bool _disposed;

    private SqlServerTestDatabase(
        string databaseName,
        string connectionString,
        string masterConnectionString)
    {
        DatabaseName = databaseName;
        ConnectionString = connectionString;
        _masterConnectionString = masterConnectionString;
    }

    public string DatabaseName { get; }

    public string ConnectionString { get; }

    public static SqlServerTestDatabase Create()
    {
        var databaseName =
            $"VietnameseEduChatTests_{Guid.NewGuid():N}";
        var baseConnectionString =
            GetBaseConnectionString();
        var databaseConnection =
            new SqlConnectionStringBuilder(baseConnectionString)
            {
                InitialCatalog = databaseName
            };
        var masterConnection =
            new SqlConnectionStringBuilder(baseConnectionString)
            {
                InitialCatalog = "master"
            };

        return new SqlServerTestDatabase(
            databaseName,
            databaseConnection.ConnectionString,
            masterConnection.ConnectionString);
    }

    public DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
    }

    public async Task CreateEmptyDatabaseAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await OpenMasterConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            $"CREATE DATABASE {QuoteIdentifier(DatabaseName)};";

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await DropDatabaseAsync();
    }

    public void Dispose()
    {
        DisposeAsync()
            .AsTask()
            .GetAwaiter()
            .GetResult();
    }

    private static string GetBaseConnectionString()
    {
        var configured = Environment.GetEnvironmentVariable(
            ConnectionStringEnvironmentVariable);

        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        return "Server=(localdb)\\MSSQLLocalDB;" +
               "Integrated Security=True;" +
               "Encrypt=False;" +
               "TrustServerCertificate=True;" +
               "MultipleActiveResultSets=True";
    }

    private async Task DropDatabaseAsync()
    {
        try
        {
            await using var connection =
                await OpenMasterConnectionAsync();

            await using var command = connection.CreateCommand();
            var databaseName = QuoteIdentifier(DatabaseName);
            command.CommandText =
                $"""
                IF DB_ID(N'{EscapeSqlLiteral(DatabaseName)}') IS NOT NULL
                BEGIN
                    ALTER DATABASE {databaseName}
                        SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE {databaseName};
                END
                """;

            await command.ExecuteNonQueryAsync();
        }
        catch (SqlException)
        {
            // Cleanup should not hide the test result.
        }
    }

    private async Task<SqlConnection> OpenMasterConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(90);
        Exception? lastError = null;

        while (DateTimeOffset.UtcNow < deadline)
        {
            var connection =
                new SqlConnection(_masterConnectionString);

            try
            {
                await connection.OpenAsync(cancellationToken);
                return connection;
            }
            catch (SqlException exception)
            {
                lastError = exception;
                await connection.DisposeAsync();
                await Task.Delay(
                    TimeSpan.FromSeconds(2),
                    cancellationToken);
            }
        }

        throw new InvalidOperationException(
            "SQL Server test database is not available.",
            lastError);
    }

    private static string QuoteIdentifier(string identifier)
    {
        return "[" + identifier.Replace("]", "]]") + "]";
    }

    private static string EscapeSqlLiteral(string value)
    {
        return value.Replace("'", "''");
    }
}
