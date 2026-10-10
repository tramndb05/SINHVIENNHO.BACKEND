using System.Security.Claims;

namespace SinhVienNho.Api.Services;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(value ?? "0");
    }
}
