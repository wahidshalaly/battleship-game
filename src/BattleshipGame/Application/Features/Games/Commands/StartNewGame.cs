using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Exceptions;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Features.Games.Commands;

public record StartNewGameCommand(PlayerId PlayerId, int BoardSize) : IRequest<Guid>;

internal class StartNewGameHandler(
    ILogger<StartNewGameHandler> logger,
    IGameRepository gameRepository,
    IPlayerRepository playerRepository
) : IRequestHandler<StartNewGameCommand, Guid>
{
    public async Task<Guid> Handle(StartNewGameCommand request, CancellationToken ct)
    {
        var player =
            await playerRepository.GetByIdAsync(request.PlayerId, ct)
            ?? throw new PlayerNotFoundException(request.PlayerId);

        var game = new Game(request.PlayerId, request.BoardSize);
        player.JoinGame(game.Id);

        await gameRepository.SaveAsync(game, ct);
        await playerRepository.SaveAsync(player, ct);

        logger.LogInformation("Created {GameId} for {PlayerId}", game.Id, request.PlayerId);

        return game.Id;
    }
}
