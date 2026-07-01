using System.Security.Claims;
using BattleshipGame.Application.Common.Security;

namespace BattleshipGame.WebAPI.Authentication;

/// <summary>
/// Resolves the authenticated caller's subject from the current request's JWT claims.
/// </summary>
public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string? SubjectId =>
        httpContextAccessor.HttpContext?.User is { Identity.IsAuthenticated: true } user
            ? user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
}
