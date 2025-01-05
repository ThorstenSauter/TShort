using FastEndpoints;

namespace TShort.Api.Tests.Integration;

[NotInParallel(DatabaseTest)]
public abstract class AlbaTestBase
{
    private readonly AlbaBootstrap _albaBootstrap;

    protected AlbaTestBase(AlbaBootstrap albaBootstrap)
    {
        _albaBootstrap = albaBootstrap;
        VerifySettings.ScrubMember<ProblemDetails>(nameof(ProblemDetails.TraceId));
    }

    private const string DatabaseTest = nameof(DatabaseTest);

    protected VerifySettings VerifySettings { get; } = new();

    protected IAlbaHost Host => _albaBootstrap.Host;

    [After(Test)]
    public async Task ResetDatabaseAsync() =>
        await _albaBootstrap.ResetDatabaseAsync();
}
