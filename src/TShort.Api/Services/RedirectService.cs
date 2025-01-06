using System.Collections.Frozen;
using Microsoft.EntityFrameworkCore;
using TShort.Api.Data;
using TShort.Api.Data.Models;
using TShort.Api.Errors;

namespace TShort.Api.Services;

public sealed class RedirectService(AppDbContext dbContext, ILogger<RedirectService> logger) : IRedirectService
{
    private static readonly FrozenSet<string> RestrictedShortNames = new HashSet<string> { "admin", "api" }
        .ToFrozenSet();

    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<RedirectService> _logger = logger;

    public async Task<Result<Redirect>> CreateRedirectAsync(Redirect newRedirect, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(newRedirect);

        if (RestrictedShortNames.Contains(newRedirect.ShortName))
        {
            return new PropertyValidationError(nameof(Redirect.ShortName), "The short name is reserved",
                newRedirect.ShortName);
        }

        var existingRedirect = await _dbContext.Redirects.FindAsync([newRedirect.ShortName], cancellationToken);
        if (existingRedirect is not null)
        {
            return new PropertyValidationError(nameof(Redirect.ShortName), "The short name is already in use",
                newRedirect.ShortName);
        }

        try
        {
            _dbContext.Redirects.Add(newRedirect);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save redirect");
            return Result.Fail("Failed to save redirect");
        }

        return Result.Ok(newRedirect);
    }

    public async Task<Result> DeleteRedirectAsync(string shortName, string userId, bool isUserPrivileged,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(shortName);

        var redirectResult = await GetRedirectAsync(shortName, userId, isUserPrivileged, cancellationToken);
        if (redirectResult.IsFailed)
        {
            return Result.Fail(redirectResult.Errors);
        }

        try
        {
            _dbContext.Redirects.Remove(redirectResult.Value);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to remove redirect {ShortName}", shortName);
            return Result.Fail("Failed to remove redirect");
        }

        return Result.Ok();
    }

    public async Task<Result<Redirect>> GetRedirectAsync(string shortName, string userId, bool isUserPrivileged,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(shortName);

        var redirect = await _dbContext.Redirects.FindAsync([shortName], cancellationToken);
        if (redirect is null)
        {
            return Result.Fail(new NotFoundError());
        }

        if (redirect.CreatedBy != userId && !isUserPrivileged)
        {
            return Result.Fail(new InvalidObjectAccessError());
        }

        return Result.Ok(redirect);
    }

    public async Task<List<Redirect>> GetRedirectsAsync(string userId, bool isUserPrivileged,
        CancellationToken cancellationToken)
    {
        var redirects = isUserPrivileged
            ? _dbContext.Redirects
            : _dbContext.Redirects.Where(x => x.CreatedBy == userId);

        return await redirects.ToListAsync(cancellationToken);
    }

    public async Task<Result<Redirect>> UpdateRedirectAsync(Redirect updatedRedirect, string userId,
        bool isUserPrivileged, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(updatedRedirect);

        var redirectResult = await GetRedirectAsync(
            updatedRedirect.ShortName, userId, isUserPrivileged, cancellationToken);

        if (redirectResult.IsFailed)
        {
            return redirectResult;
        }

        var redirect = redirectResult.Value;
        try
        {
            redirect.RedirectTo = updatedRedirect.RedirectTo;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update redirect");
            return Result.Fail("Failed to update redirect");
        }

        return redirect;
    }
}
