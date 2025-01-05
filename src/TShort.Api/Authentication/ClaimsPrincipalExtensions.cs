using System.Security.Claims;
using Microsoft.Identity.Web;

namespace TShort.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal claimsPrincipal) =>
        claimsPrincipal.GetObjectId() ?? Guid.Empty.ToString();
}
