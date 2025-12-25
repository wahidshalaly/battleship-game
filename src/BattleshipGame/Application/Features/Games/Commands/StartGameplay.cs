using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Commands;

public record StartGameplayCommand(GameId GameId) : IRequest;

internal class StartGameplayHandler(IGameRepository gameRepository)
    : IRequestHandler<StartGameplayCommand>
{
    public async Task Handle(StartGameplayCommand request, CancellationToken ct)
    {
        var game =
            await gameRepository.GetByIdAsync(request.GameId, ct)
            ?? throw new GameNotFoundException(request.GameId);
        if (!game.IsReady)
            throw new InvalidOperationException(
                "Cannot start gameplay until both boards are ready."
            );
        // Domain may transition state implicitly after readiness; persist to be safe
        await gameRepository.SaveAsync(game, ct);
    }
}
