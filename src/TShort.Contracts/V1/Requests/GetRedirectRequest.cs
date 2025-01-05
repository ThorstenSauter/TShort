namespace TShort.Contracts.V1.Requests;

/// <summary>
///     Request for getting a single redirect.
/// </summary>
public sealed class GetRedirectRequest
{
    /// <summary>
    ///     The short path to get the redirect for.
    /// </summary>
    public string ShortName { get; set; } = null!;
}
