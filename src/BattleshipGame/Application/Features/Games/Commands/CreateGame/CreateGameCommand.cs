using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Commands.CreateGame;

/// <summary>
/// Command to create a new game.
/// </summary>
/// <param name="PlayerId">The player creating the game.</param>
/// <param name="BoardSize">The size of the game board (optional, defaults to 10).</param>
public record CreateGameCommand(PlayerId PlayerId, int? BoardSize = 10) : IRequest<CreateGameResult>;

/// <summary>
/// Result of creating a game.
/// </summary>
/// <param name="GameId">The created game's identifier.</param>
/// <param name="PlayerId">The player's identifier.</param>
/// <param name="BoardSize">The board size.</param>
/// <param name="State">The game state.</param>
public record CreateGameResult(GameId GameId, PlayerId PlayerId, int BoardSize, string State);
