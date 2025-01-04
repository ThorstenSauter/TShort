using System.Data.Common;
using Alba.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Web;
using Respawn;
using Testcontainers.MsSql;
using TShort.Api.Tests.Integration.Extensions;
using TUnit.Core.Interfaces;

namespace TShort.Api.Tests.Integration;

public sealed class AlbaBootstrap : IAsyncInitializer, IAsyncDisposable
{
    internal const string DefaultUserId = "bf620beb-36c3-4bfb-b6d0-41fcca20d4b0";

    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private DbConnection _dbConnection = null!;
    private Respawner _respawner = null!;

    public IAlbaHost Host { get; private set; } = null!;

    public async ValueTask DisposeAsync()
    {
        await _sqlContainer.StopAsync();
        await Host.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        var connectionString = _sqlContainer.GetConnectionString();

        var authenticationStub = new AuthenticationStub().With(ClaimConstants.Oid, DefaultUserId);
        Host = await AlbaHost.For<IApiAssemblyMarker>(
            authenticationStub,
            new DataExtension(connectionString));

        _dbConnection = new SqlConnection(connectionString);
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            connectionString,
            new RespawnerOptions { DbAdapter = DbAdapter.SqlServer, TablesToIgnore = ["__EFMigrationsHistory"] });
    }

    public async Task ResetDatabaseAsync() =>
        await _respawner.ResetAsync(_dbConnection);
}
