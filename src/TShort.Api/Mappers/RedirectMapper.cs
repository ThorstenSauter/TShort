using TShort.Api.Data.Models;
using TShort.Contracts.V1.Responses;

namespace TShort.Api.Mappers;

public static class RedirectMapper
{
    public static RedirectResponse ToResponse(this Redirect redirect) =>
        new() { ShortName = redirect.ShortName, RedirectTo = redirect.RedirectTo, CreatedBy = redirect.CreatedBy };
}
