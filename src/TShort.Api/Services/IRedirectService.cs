using TShort.Api.Data.Models;

namespace TShort.Api.Services;

public interface IRedirectService
{
    Task<Result<Redirect>> CreateRedirectAsync(Redirect newRedirect, CancellationToken cancellationToken);

    Task<Result> DeleteRedirectAsync(string shortName, string userId, bool isUserPrivileged,
        CancellationToken cancellationToken);

    Task<Result<Redirect>> GetRedirectAsync(string shortName, string userId, bool isUserPrivileged,
        CancellationToken cancellationToken);

    Task<List<Redirect>> GetRedirectsAsync(string userId, bool isUserPrivileged, CancellationToken cancellationToken);

    Task<Result<Redirect>> UpdateRedirectAsync(Redirect updatedRedirect, string userId, bool isUserPrivileged,
        CancellationToken cancellationToken);
}
