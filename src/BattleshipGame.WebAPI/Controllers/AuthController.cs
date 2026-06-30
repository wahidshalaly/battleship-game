using BattleshipGame.Application.Common.Security;
using BattleshipGame.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BattleshipGame.WebAPI.Controllers;

/// <summary>
/// Identity façade — proxies Keycloak for register, sign-in, token refresh, and sign-out.
/// All endpoints are anonymous because the caller does not have a token yet.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[Produces("application/json")]
public class AuthController(IIdentityProvider identityProvider, IPlayerService playerService)
    : ControllerBase
{
    /// <summary>
    /// Registers a new user identity and creates their game profile in one step.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthTokenResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct
    )
    {
        var (tokens, subject) = await identityProvider.RegisterAsync(
            request.Username,
            request.Email,
            request.Password,
            ct
        );

        // Create the game profile bound to the new identity.
        await playerService.CreateAsync(request.Username, subject, ct);

        return StatusCode(StatusCodes.Status201Created, ToResponse(tokens));
    }

    /// <summary>Signs in an existing user and returns access and refresh tokens.</summary>
    [HttpPost("signin")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> SignIn(
        [FromBody] SignInRequest request,
        CancellationToken ct
    )
    {
        var tokens = await identityProvider.SignInAsync(request.Username, request.Password, ct);
        return Ok(ToResponse(tokens));
    }

    /// <summary>Exchanges a refresh token for a new token pair.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken ct
    )
    {
        var tokens = await identityProvider.RefreshAsync(request.RefreshToken, ct);
        return Ok(ToResponse(tokens));
    }

    /// <summary>Invalidates the session and signs the user out.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken ct)
    {
        await identityProvider.SignOutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    private static AuthTokenResponse ToResponse(IdentityTokens tokens) =>
        new(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresInSeconds);
}

public record RegisterRequest(string Username, string Email, string Password);

public record SignInRequest(string Username, string Password);

public record RefreshRequest(string RefreshToken);

public record AuthTokenResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds);
