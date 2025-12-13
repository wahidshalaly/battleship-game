using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Infrastructure.OpponentStrategy;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Commands;

public record OpponentAttackCommand(GameId GameId) : IRequest<OpponentAttackResult>;

public record OpponentAttackResult(
    string CellCode,
    CellState CellState,
    bool IsGameOver,
    BoardSide Winner
);

internal class OpponentAttackHandler(
    IGameRepository gameRepository,
    IComputerOpponentStrategy opponentStrategy
) : IRequestHandler<OpponentAttackCommand, OpponentAttackResult>
{
    public async Task<OpponentAttackResult> Handle(
        OpponentAttackCommand request,
        CancellationToken cancellationToken
    )
    {
        var game =
            await gameRepository.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new GameNotFoundException(request.GameId);
        if (game.State == GameState.GameOver)
            throw new InvalidOperationException("Game already over.");
        if (!game.IsReady)
            throw new InvalidOperationException("Setup incomplete.");
        var targetCell = await opponentStrategy.SelectNextAttack(request.GameId);
        var cellState = game.Attack(BoardSide.Player, targetCell);
        await gameRepository.SaveAsync(game, cancellationToken);
        return new OpponentAttackResult(
            targetCell,
            cellState,
            game.State == GameState.GameOver,
            game.State == GameState.GameOver ? BoardSide.Opponent : BoardSide.None
        );
    }
}
