using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Services;

public record GameStatus(
    GameId Id,
    BoardSide Winner,
    GameState State,
    bool IsGameOver,
    CellState? PlayerState = null,
    CellState? OpponentState = null
);
