using FastEndpoints;

namespace TShort.Api.Tests.Integration;

[NotInParallel(DatabaseTest)]
public abstract class EndpointTestBase
{
    private readonly ApiFactory _apiFactory;

    protected EndpointTestBase(ApiFactory apiFactory)
    {
        _apiFactory = apiFactory;
        VerifySettings.ScrubMember<ProblemDetails>(nameof(ProblemDetails.TraceId));
    }

    private const string DatabaseTest = nameof(DatabaseTest);

    protected VerifySettings VerifySettings { get; } = new();

    protected IAlbaHost Host => _apiFactory.Host;

    [After(Test)]
    public async Task ResetDatabaseAsync() =>
        await _apiFactory.ResetDatabaseAsync();
}
