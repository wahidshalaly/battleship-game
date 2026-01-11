using System.Text.Json.Serialization;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

/// <summary>
/// Represents a player in the game.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BoardSide
{
    None = 0,
    Player = 1,
    Opponent = 2,
}
