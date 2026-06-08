using FW.Application;
using System.Security.Claims;

namespace DigitalWallet.API;

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    private ClaimsPrincipal User => accessor.HttpContext!.User;

    public Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public string Email => User.FindFirstValue(ClaimTypes.Email)!;
    public string Role => User.FindFirstValue(ClaimTypes.Role)!;
}
