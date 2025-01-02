namespace TShort.Api.Tests.Integration;

public sealed class AlwaysPassing
{
    [Test]
    public Task Pass() =>
        Task.CompletedTask;
}
