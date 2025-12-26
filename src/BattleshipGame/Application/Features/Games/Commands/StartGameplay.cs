using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Features.Games.Commands;

public record StartGameplayCommand(GameId GameId) : IRequest;

internal class StartGameplayHandler(
    ILogger<StartGameplayHandler> logger,
    IGameRepository gameRepository
) : IRequestHandler<StartGameplayCommand>
{
    public async Task Handle(StartGameplayCommand request, CancellationToken ct)
    {
        var game =
            await gameRepository.GetByIdAsync(request.GameId, ct)
            ?? throw new GameNotFoundException(request.GameId);

        game.StartGameplay();

        await gameRepository.SaveAsync(game, ct);

        logger.LogInformation("Gameplay started. {@Payload}", new { GameId = game.Id.Value });
    }
}
