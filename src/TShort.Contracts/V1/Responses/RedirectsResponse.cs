namespace TShort.Contracts.V1.Responses;

/// <summary>
/// Represents a response containing a list of redirects.
/// </summary>
public class RedirectsResponse
{
    /// <summary>
    /// The list of redirects.
    /// </summary>
    public required ICollection<RedirectResponse> Redirects { get; init; }
}
