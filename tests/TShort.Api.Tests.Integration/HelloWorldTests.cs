namespace TShort.Api.Tests.Integration;

public sealed class HelloWorldTests
{
    [ClassDataSource<ApiFactory>(Shared = SharedType.PerTestSession)]
    public required ApiFactory Factory { get; init; }

    [Test]
    public async Task Pass()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var result = await client.GetStringAsync("/");

        // Assert
        result.Should().Be("Hello World!");
    }
}
