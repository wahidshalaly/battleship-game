using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

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
/// <param name="gameRepository">The game repository.</param>
/// <param name="eventDispatcher">The domain event dispatcher.</param>
internal class CreateGameHandler(
    IGameRepository gameRepository,
    IDomainEventDispatcher eventDispatcher
) : IRequestHandler<CreateGameCommand, Guid>
{
    /// <summary>
    /// Handles <see cref="CreateGameCommand"/> and returns the new game ID.
    /// </summary>
    /// <param name="request">The create game command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new game ID.</returns>
    public async Task<Guid> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        // 1. Create new game (this may raise domain events)
        var game = new Game(request.PlayerId, request.BoardSize ?? 10);

        // 2. Save aggregate state
        await gameRepository.SaveAsync(game, cancellationToken);

        // 3. Dispatch domain events (THIS IS WHERE SIDE EFFECTS HAPPEN)
        await eventDispatcher.DispatchEventsAsync(game, cancellationToken);

        // 4. Clear events from aggregate
        game.ClearDomainEvents();

        return game.Id;
    }
}
