using System;

namespace BattleshipGame.Domain.Events;

internal class GameStarted : DomainEvent<GameStarted>
{
    public Guid GameId { get; init; }

    public GameStarted(Guid gameId)
    {
        GameId = gameId;
    }
}
