namespace BattleshipGame.Application.Common.Security;

/// <summary>
/// Tokens returned by register and sign-in operations.
/// </summary>
public sealed record IdentityTokens(string AccessToken, string RefreshToken, int ExpiresInSeconds);

/// <summary>
/// Abstracts the external identity provider so the Application layer stays independent of
/// Keycloak-specific HTTP details.
/// </summary>
public interface IIdentityProvider
{
    /// <summary>
    /// Creates a new identity in the provider and returns tokens for the new user.
    /// The identity's <c>sub</c> claim is embedded in the returned access token.
    /// </summary>
    Task<(IdentityTokens Tokens, string Subject)> RegisterAsync(
        string username,
        string email,
        string password,
        CancellationToken ct
    );

    /// <summary>Signs in an existing user and returns tokens.</summary>
    Task<IdentityTokens> SignInAsync(string username, string password, CancellationToken ct);

    /// <summary>Exchanges a refresh token for a fresh token pair.</summary>
    Task<IdentityTokens> RefreshAsync(string refreshToken, CancellationToken ct);

    /// <summary>Invalidates the session associated with the given refresh token.</summary>
    Task SignOutAsync(string refreshToken, CancellationToken ct);
}
