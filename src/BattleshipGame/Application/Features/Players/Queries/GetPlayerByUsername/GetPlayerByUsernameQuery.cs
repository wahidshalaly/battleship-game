using BattleshipGame.Application.Features.Players.Queries.GetPlayer;
using MediatR;

namespace BattleshipGame.Application.Features.Players.Queries.GetPlayerByUsername;

/// <summary>
/// Query to get a player by username.
/// </summary>
/// <param name="Username">The player's username.</param>
public record GetPlayerByUsernameQuery(string Username) : IRequest<GetPlayerResult>;
