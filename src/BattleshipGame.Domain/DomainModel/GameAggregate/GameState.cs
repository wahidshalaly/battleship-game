using System.Text.Json.Serialization;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

/// <summary>
/// This is the outcome of a hit on a cell.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameState
{
    None = 0,
    New = 1,
    Ready = 2,
    Started = 3,
    GameOver = 4,
}
