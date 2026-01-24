using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Application.Features.Games.Commands;

/// <summary>
/// Command to start gameplay for a game that is ready.
/// </summary>
/// <remarks>
/// Only games in Ready state (all ships placed) can transition to Started state.
/// This command should be called before any attacks are made.
/// </remarks>
/// <param name="GameId">The game identifier.</param>
public record StartGameplayCommand(GameId GameId) : IRequest;

/// <summary>
/// Handles the StartGameplayCommand by transitioning game state from Ready to Started.
/// </summary>
/// <remarks>
/// This handler enforces the Single Responsibility Principle by handling only game initialization.
/// Attack operations are delegated to PlayerAttackHandler.
/// </remarks>
internal class StartGameplayHandler(
    ILogger<StartGameplayHandler> logger,
    IGameRepository gameRepository,
    IDomainEventDispatcher eventDispatcher
) : IRequestHandler<StartGameplayCommand>
{
    /// <summary>
    /// Handles the start gameplay command.
    /// </summary>
    /// <param name="request">The start gameplay command.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    /// <exception cref="GameNotReadyException">Thrown when game is not in Ready state.</exception>
    public async Task Handle(StartGameplayCommand request, CancellationToken ct)
    {
        // 1. Load aggregate
        var game = await gameRepository.GetByIdOrThrowAsync(request.GameId, ct);

        // 2. Start gameplay - validates game state
        game.StartGameplay();

        logger.LogInformation(
            "Gameplay started. Game `{GameId}`. Initial target side: {TargetSide}",
            request.GameId.Value,
            game.TargetSide
        );

        // 3. Save the aggregate back to repository
        await gameRepository.SaveAsync(game, ct);

        // 4. Dispatch domain events
        await eventDispatcher.DispatchEventsAsync(game, ct);
    }
}
