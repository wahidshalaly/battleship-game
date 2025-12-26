using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Application.Services;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Commands;

public record CheckGameStatusCommand(GameId GameId) : IRequest<GameStatus>;

public class CheckGameStatusHandler(IGameRepository gameRepository)
    : IRequestHandler<CheckGameStatusCommand, GameStatus>
{
    public async Task<GameStatus> Handle(CheckGameStatusCommand request, CancellationToken ct)
    {
        var game =
            await gameRepository.GetByIdAsync(request.GameId, ct)
            ?? throw new GameNotFoundException(request.GameId);

        var isGameOver = game.State == GameState.GameOver;

        var winner =
            game.IsGameOver(BoardSide.Player) ? BoardSide.Opponent
            : game.IsGameOver(BoardSide.Opponent) ? BoardSide.Player
            : BoardSide.None;

        return new GameStatus(game.Id, winner, game.State, isGameOver);
    }
}
