using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Features.Games.Commands;

/// <summary>
/// Command to create a new game.
/// </summary>
/// <param name="PlayerId">The player creating the game.</param>
/// <param name="BoardSize">The size of the game board (optional, defaults to 10).</param>
public record CreateGameCommand(PlayerId PlayerId, int? BoardSize = 10) : IRequest<Guid>;

/// <summary>
/// Handler for creating a new game.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="gameRepository">The game repository.</param>
/// <param name="eventDispatcher">The domain event dispatcher.</param>
public class CreateGameHandler(
    ILogger<CreateGameHandler> logger,
    IGameRepository gameRepository,
    IDomainEventDispatcher eventDispatcher
) : IRequestHandler<CreateGameCommand, Guid>
{
    /// <summary>
    /// Handles <see cref="CreateGameCommand"/> and returns <see cref="CreateGameResult"/>.
    /// </summary>
    /// <param name="request">The create game command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The attack result.</returns>
    public async Task<Guid> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Creating new game for player {PlayerId} with board size {BoardSize}",
            request.PlayerId,
            request.BoardSize ?? 10
        );

        // 1. Create new game (this may raise domain events)
        var game = new Game(request.PlayerId, request.BoardSize ?? 10);

        // 2. Save aggregate state
        await gameRepository.SaveAsync(game, cancellationToken);

        // 3. Dispatch domain events (THIS IS WHERE SIDE EFFECTS HAPPEN)
        await eventDispatcher.DispatchEventsAsync(game, cancellationToken);

        // 4. Clear events from aggregate
        game.ClearDomainEvents();

        logger.LogInformation(
            "Successfully created game {GameId} for player {PlayerId}",
            game.Id,
            request.PlayerId
        );

        return game.Id;
    }
}
