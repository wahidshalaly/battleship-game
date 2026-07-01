using System.ComponentModel.DataAnnotations;

namespace BattleshipGame.Infrastructure.Identity;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    public string Realm { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Keycloak administrator username — used to obtain an admin token for user creation via the
    /// Admin REST API. Typically "admin" in local dev; in production use a dedicated service
    /// account or secrets management.
    /// </summary>
    [Required]
    public string AdminUsername { get; set; } = string.Empty;

    [Required]
    public string AdminPassword { get; set; } = string.Empty;
}
