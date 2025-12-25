using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Commands;

public record EndGameCommand(GameId GameId) : IRequest;

internal class EndGameHandler(IGameRepository gameRepository) : IRequestHandler<EndGameCommand>
{
    public async Task Handle(EndGameCommand request, CancellationToken cancellationToken)
    {
        var game =
            await gameRepository.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new GameNotFoundException(request.GameId);

        if (game.State == GameState.GameOver)
            return;

        throw new NotImplementedException();

        // Force game to end; winner cannot be derived here (manual termination)
        // game.EndGame(); // or use GameplayService to encapsulate this logic
        // Persist changes
        // await gameRepository.SaveAsync(game, cancellationToken);
    }
}
