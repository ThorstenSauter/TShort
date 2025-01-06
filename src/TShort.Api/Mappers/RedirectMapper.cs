using TShort.Api.Data.Models;
using TShort.Contracts.V1.Requests;
using TShort.Contracts.V1.Responses;

namespace TShort.Api.Mappers;

public static class RedirectMapper
{
    public static RedirectResponse ToResponse(this Redirect redirect) =>
        new() { ShortName = redirect.ShortName, RedirectTo = redirect.RedirectTo, CreatedBy = redirect.CreatedBy };

    public static Redirect ToRedirect(this CreateRedirectRequest request, string userId) =>
        new() { ShortName = request.ShortName, RedirectTo = request.RedirectTo, CreatedBy = userId };

    public static Redirect ToRedirect(this UpdateRedirectRequest request) =>
        new() { ShortName = request.ShortName, RedirectTo = request.RedirectTo, CreatedBy = Guid.Empty.ToString() };
}
