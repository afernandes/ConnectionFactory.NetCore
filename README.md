# ConnectionFactory.NetCore

ADO.NET command helpers targeting .NET Standard 2.0. Microsoft.Extensions dependencies use the compatible 8.0 line.

Pass a configured `DbConnection` to `CfConnection`; it opens a closed connection when executing a command. Commands accept optional parameters and an `ILogger` (the default is `NullLogger`). Dispose commands and connections after use.

Parameter logging records only whether parameters were supplied. It does not log their values or enumerate deferred parameter sequences. SQL command text is logged, so use parameterized queries for sensitive values.

## Validation

Install the .NET 8 SDK, then run:

```sh
dotnet test test/ConnectionFactory.NetCore.Tests/ConnectionFactory.NetCore.Tests.csproj -c Release
dotnet pack src/ConnectionFactory.csproj -c Release -o artifacts
```

The current tests use only SQLite in memory. They cover opening supplied connections, optional parameters, parameterized values, logging, scalar/null results, object mapping, and provider exceptions. CI runs on Linux and Windows and creates a package locally; it does not publish packages.

`test/ConnectionFactoryTest` is a historical .NET Framework 4.5/MSTest project referencing the removed `ConnectionFactoryNET45` project and an external SQL Server. It is retained as historical source and is not the runnable .NET Standard test suite.
