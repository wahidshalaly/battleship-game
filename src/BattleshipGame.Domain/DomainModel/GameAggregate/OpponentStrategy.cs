using System.Text.Json.Serialization;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

/// <summary>
/// Defines the types of computer opponents available for gameplay.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OpponentStrategy
{
    /// <summary>
    /// No opponent type specified (default/uninitialized state).
    /// </summary>
    None = 0,

    /// <summary>
    /// Random attack strategy - selects cells randomly from available targets.
    /// </summary>
    Random = 1,

    /// <summary>
    /// Semantic Kernel (LLM) based strategy - uses AI reasoning for cell selection.
    /// </summary>
    SemanticKernel = 2,
}
