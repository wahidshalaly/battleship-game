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
public record CreateGameCommand(
    PlayerId PlayerId,
    int? BoardSize = 10)
    : IRequest<CreateGameResult>;

/// <summary>
/// Result of creating a game.
/// </summary>
/// <param name="GameId">The created game's identifier.</param>
public record CreateGameResult(GameId GameId);

/// <summary>
/// Handler for creating a new game.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="gameRepository">The game repository.</param>
/// <param name="eventDispatcher">The domain event dispatcher.</param>
public class CreateGameCommandHandler(
    ILogger<CreateGameCommandHandler> logger,
    IGameRepository gameRepository,
    IDomainEventDispatcher eventDispatcher)
    : IRequestHandler<CreateGameCommand, CreateGameResult>
{

    /// <summary>
    /// Handles <see cref="CreateGameCommand"/> and returns <see cref="CreateGameResult"/>.
    /// </summary>
    /// <param name="request">The create game command.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The attack result.</returns>
    public async Task<CreateGameResult> Handle(CreateGameCommand request, CancellationToken ct)
    {
        logger.LogInformation(
            "Creating new game for player {PlayerId} with board size {BoardSize}",
            request.PlayerId, request.BoardSize ?? 10);

        // 1. Create new game (this may raise domain events)
        var game = new Game(request.PlayerId, request.BoardSize ?? 10);

        // 2. Save aggregate state
        await gameRepository.SaveAsync(game, ct);

        // 3. Dispatch domain events (THIS IS WHERE SIDE EFFECTS HAPPEN)
        await eventDispatcher.DispatchEventsAsync(game, ct);

        // 4. Clear events from aggregate
        game.ClearDomainEvents();

        logger.LogInformation(
            "Successfully created game {GameId} for player {PlayerId}",
            game.Id, request.PlayerId);

        return new CreateGameResult(game.Id);
    }
}
