using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Commands;

public record CheckGameStatusCommand(GameId GameId) : IRequest<GameResult?>;

public class CheckGameStatusHandler(IGameRepository gameRepository)
    : IRequestHandler<CheckGameStatusCommand, GameResult?>
{
    public async Task<GameResult?> Handle(
        CheckGameStatusCommand request,
        CancellationToken cancellationToken
    )
    {
        var game =
            await gameRepository.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new GameNotFoundException(request.GameId);

        if (game.State != GameState.GameOver)
            return null;

        var winner =
            game.IsGameOver(BoardSide.Player) ? BoardSide.Opponent
            : game.IsGameOver(BoardSide.Opponent) ? BoardSide.Player
            : BoardSide.None;

        return new GameResult(game.Id, winner, game.State, true);
    }
}
