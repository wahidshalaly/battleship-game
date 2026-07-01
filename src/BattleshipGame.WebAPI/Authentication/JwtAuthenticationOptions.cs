using System.ComponentModel.DataAnnotations;

namespace BattleshipGame.WebAPI.Authentication;

/// <summary>
/// Options for validating incoming JWT bearer tokens. Kept provider-agnostic: the API only
/// needs the OIDC <see cref="Authority"/> and the expected <see cref="Audience"/>, so the token
/// issuer (Keycloak for local dev, Microsoft Entra ID for the cloud, etc.) is a configuration
/// concern rather than a code change.
/// </summary>
public sealed class JwtAuthenticationOptions
{
    public const string SectionName = "Authentication";

    /// <summary>
    /// The OIDC authority (issuer) whose discovery document and signing keys validate tokens.
    /// </summary>
    [Required]
    public string? Authority { get; set; }

    /// <summary>
    /// The expected <c>aud</c> claim value for tokens accepted by this API.
    /// </summary>
    [Required]
    public string? Audience { get; set; }

    /// <summary>
    /// Whether HTTPS metadata is required from the authority. Defaults to <c>true</c>; set to
    /// <c>false</c> for local development against an HTTP authority.
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;
}
