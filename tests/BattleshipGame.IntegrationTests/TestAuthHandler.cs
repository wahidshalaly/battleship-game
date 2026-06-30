using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BattleshipGame.IntegrationTests;

/// <summary>
/// A provider-neutral authentication handler for integration tests. It stands in for the real
/// JWT bearer / Keycloak flow so tests never need a live identity provider: when the request
/// carries the <see cref="SubjectHeader"/>, the caller is authenticated with that value as the
/// subject; otherwise the request is treated as anonymous (so authorization yields 401).
/// </summary>
public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    /// <summary>Header carrying the desired subject (token <c>sub</c>) for the request.</summary>
    public const string SubjectHeader = "X-Test-Sub";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (
            !Request.Headers.TryGetValue(SubjectHeader, out var subject)
            || string.IsNullOrEmpty(subject)
        )
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, subject!),
            new Claim("sub", subject!),
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
