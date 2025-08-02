using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Players.Commands.CreatePlayer;

/// <summary>
/// Command to create a new player.
/// </summary>
/// <param name="Username">The player's username.</param>
public record CreatePlayerCommand(string Username) : IRequest<CreatePlayerResult>;

/// <summary>
/// Result of creating a player.
/// </summary>
/// <param name="PlayerId">The created player's identifier.</param>
/// <param name="Username">The player's username.</param>
public record CreatePlayerResult(PlayerId PlayerId, string Username);
