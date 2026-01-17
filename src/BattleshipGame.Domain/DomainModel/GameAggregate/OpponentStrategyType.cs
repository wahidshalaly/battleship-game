namespace BattleshipGame.Domain.DomainModel.GameAggregate;

/// <summary>
/// Defines the types of AI opponent strategies available for gameplay.
/// </summary>
public enum OpponentStrategyType
{
    /// <summary>
    /// Random attack strategy - selects cells randomly from available targets.
    /// </summary>
    Random,

    /// <summary>
    /// Semantic Kernel (LLM) based strategy - uses AI reasoning for cell selection.
    /// </summary>
    SemanticKernel,
}
