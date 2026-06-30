namespace BattleshipGame.Infrastructure.Identity;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    public string BaseUrl { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Keycloak administrator username — used to obtain an admin token for user creation via the
    /// Admin REST API. Typically "admin" in local dev; in production use a dedicated service
    /// account or secrets management.
    /// </summary>
    public string AdminUsername { get; set; } = string.Empty;

    public string AdminPassword { get; set; } = string.Empty;
}
