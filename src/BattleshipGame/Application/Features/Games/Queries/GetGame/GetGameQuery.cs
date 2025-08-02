using BattleshipGame.Domain.DomainModel.GameAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Queries.GetGame;

/// <summary>
/// Query to get a game by ID.
/// </summary>
/// <param name="GameId">The game identifier.</param>
public record GetGameQuery(GameId GameId) : IRequest<GetGameResult>;

/// <summary>
/// Result of getting a game.
/// </summary>
/// <param name="GameId">The game's identifier.</param>
/// <param name="PlayerId">The player's identifier.</param>
/// <param name="BoardSize">The board size.</param>
/// <param name="State">The game state.</param>
public record GetGameResult(GameId GameId, Guid PlayerId, int BoardSize, string State);
