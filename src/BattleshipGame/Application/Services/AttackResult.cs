using BattleshipGame.Domain.DomainModel.GameAggregate;

namespace BattleshipGame.Application.Services;

public record AttackResult(
    string CellCode,
    CellState CellState,
    BoardSide Attacker,
    bool IsGameOver,
    BoardSide Winner
);
