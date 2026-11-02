using ConnectionFactory;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ConnectionFactoryTests;

public class CommandTests
{
    [Fact]
    public void ScalarOpensProvidedConnectionAndAllowsOmittedParameters()
    {
        using var database = new SqliteConnection("Data Source=:memory:");
        using var connection = new CfConnection(database);
        using var command = connection.CreateCfCommand();

        Assert.Equal(42L, command.ExecuteScalar<long>(CfCommandType.Text, "SELECT 42"));
        Assert.Equal("Data Source=:memory:", database.ConnectionString);
    }

    [Fact]
    public void NonQueryAndReaderPreserveParameterizedValuesWithLogging()
    {
        using var database = new SqliteConnection("Data Source=:memory:");
        using var connection = new CfConnection(database);
        using var command = connection.CreateCfCommand();
        var provider = new RecordingLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        command.Logger = loggerFactory.CreateLogger("ConnectionFactoryTests");

        command.ExecuteNonQuery(CfCommandType.Text, "CREATE TABLE items (name TEXT)");
        const string value = "quote ' ; SELECT 1 --";
        Assert.Equal(1, command.ExecuteNonQuery(CfCommandType.Text,
            "INSERT INTO items VALUES (@name)", new { name = value }));

        using var reader = command.ExecuteReader(CfCommandType.Text, "SELECT name FROM items");
        Assert.True(reader.Read());
        Assert.Equal(value, reader.GetString(0));
        Assert.False(reader.Read());
        Assert.Contains(provider.Messages, message => message.Contains("ExecuteNonQuery"));
        Assert.Contains(provider.Messages, message => message.Contains("ExecuteReader"));
        Assert.DoesNotContain(provider.Messages, message => message.Contains(value));
    }

    [Fact]
    public void LoggingDoesNotEnumerateADeferredParameterSequence()
    {
        using var database = new SqliteConnection("Data Source=:memory:");
        using var connection = new CfConnection(database);
        using var command = connection.CreateCfCommand();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        command.Logger = loggerFactory.CreateLogger("DeferredParameters");
        var enumerations = 0;
        IEnumerable<CfParameter> Parameters()
        {
            if (++enumerations > 1) throw new InvalidOperationException("Parameters were consumed twice.");
            yield return new CfParameter("value", 42L);
        }

        Assert.Equal(42L, command.ExecuteScalar<long>(CfCommandType.Text, "SELECT @value", Parameters()));
        Assert.Equal(1, enumerations);
    }

    [Fact]
    public void ScalarMapsDatabaseNullToDefault()
    {
        using var database = new SqliteConnection("Data Source=:memory:");
        using var connection = new CfConnection(database);
        using var command = connection.CreateCfCommand();
        Assert.Null(command.ExecuteScalar<string>(CfCommandType.Text, "SELECT NULL"));
    }

    [Theory]
    [InlineData("nonquery")]
    [InlineData("scalar")]
    [InlineData("reader")]
    public void ErrorLoggingPreservesDatabaseErrorsWithDeferredParameters(string operation)
    {
        using var database = new SqliteConnection("Data Source=:memory:");
        using var connection = new CfConnection(database);
        using var command = connection.CreateCfCommand();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new RecordingLoggerProvider()));
        command.Logger = loggerFactory.CreateLogger("FailingDeferredParameters");
        var enumerations = 0;
        IEnumerable<CfParameter> Parameters()
        {
            if (++enumerations > 1) throw new InvalidOperationException("Parameters were consumed twice.");
            yield return new CfParameter("value", 42L);
        }

        const string sql = "SELECT @value FROM missing";
        var error = Assert.Throws<CfException>(() =>
        {
            switch (operation)
            {
                case "nonquery": command.ExecuteNonQuery(CfCommandType.Text, sql, Parameters()); break;
                case "scalar": command.ExecuteScalar<long>(CfCommandType.Text, sql, Parameters()); break;
                case "reader": using (command.ExecuteReader(CfCommandType.Text, sql, Parameters())) { } break;
            }
        });
        Assert.IsType<SqliteException>(error.InnerException);
        Assert.Equal(1, enumerations);
    }

    [Fact]
    public void QueryMapsNamedColumnsToObjects()
    {
        using var database = new SqliteConnection("Data Source=:memory:");
        using var connection = new CfConnection(database);
        using var command = connection.CreateCfCommand();
        var rows = command.QueryForList<Item>("SELECT 7 AS Id, 'example' AS Name");
        var item = Assert.Single(rows);
        Assert.Equal(7L, item.Id);
        Assert.Equal("example", item.Name);
    }

    [Fact]
    public void InvalidSqlRetainsProviderException()
    {
        using var database = new SqliteConnection("Data Source=:memory:");
        using var connection = new CfConnection(database);
        using var command = connection.CreateCfCommand();
        var error = Assert.Throws<CfException>(() =>
            command.ExecuteNonQuery(CfCommandType.Text, "INSERT INTO missing VALUES (1)"));
        Assert.IsType<SqliteException>(error.InnerException);
    }

    public class Item
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    private sealed class RecordingLoggerProvider : ILoggerProvider
    {
        public List<string> Messages { get; } = new();
        public ILogger CreateLogger(string categoryName) => new RecordingLogger(Messages);
        public void Dispose() { }

        private sealed class RecordingLogger(List<string> messages) : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception exception, Func<TState, Exception, string> formatter) =>
                messages.Add(formatter(state, exception));
        }
    }
}
