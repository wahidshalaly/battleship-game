using System.ComponentModel.DataAnnotations;

namespace BattleshipGame.Infrastructure.ComputerOpponent;

/// <summary>
/// Settings for the OpenAI-compatible chat endpoint the LLM opponent talks to (Ollama locally,
/// a cloud provider when hosted). Bound from the <see cref="SectionName"/> configuration section,
/// so they can come from <c>appsettings.json</c>, user secrets or <c>OpenAi__*</c> environment
/// variables.
/// </summary>
public sealed class OpenAiOptions
{
    /// <summary>
    /// The configuration section these options are bound from.
    /// </summary>
    public const string SectionName = "OpenAi";

    /// <summary>
    /// The model to request, e.g. <c>gemma4</c>.
    /// </summary>
    [Required]
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// The absolute base URL of the OpenAI-compatible API, e.g. <c>http://localhost:11434/v1</c>.
    /// </summary>
    [Required]
    [Url]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// The API key sent to the endpoint. Keep it out of the repository (user secrets or the
    /// host's secret store); Ollama accepts any value.
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;
}
