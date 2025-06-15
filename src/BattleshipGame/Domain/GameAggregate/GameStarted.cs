using System;
using BattleshipGame.Domain.Common;

namespace BattleshipGame.Domain.GameAggregate;

internal class GameStarted : DomainEvent<GameStarted>
{
    public Guid GameId { get; init; }

    public GameStarted(Guid gameId)
    {
        GameId = gameId;
    }
}
