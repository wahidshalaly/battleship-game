using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Players.Commands;

/// <summary>
/// Command to create a new player.
/// </summary>
/// <param name="Username">The player's username.</param>
public record CreatePlayerCommand(string Username) : IRequest<CreatePlayerResult>;

/// <summary>
/// Result of creating a player.
/// </summary>
/// <param name="PlayerId">The created player's identifier.</param>
public record CreatePlayerResult(PlayerId PlayerId);

/// <summary>
/// Handler for creating a new player.
/// </summary>
/// <param name="playerRepository">The player repository.</param>
public class CreatePlayerCommandHandler(IPlayerRepository playerRepository)
    : IRequestHandler<CreatePlayerCommand, CreatePlayerResult>
{
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <inheritdoc />
    public async Task<CreatePlayerResult> Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException("Username cannot be null or whitespace.", nameof(request));
        }

        // Validate username uniqueness
        if (await playerRepository.UsernameExistsAsync(request.Username, cancellationToken))
        {
            throw new InvalidOperationException($"A player with username '{request.Username}' already exists.");
        }

        // Create new player
        var playerId = new PlayerId(Guid.NewGuid());
        var player = new Player(playerId, request.Username);

        // Save player
        await playerRepository.SaveAsync(player, cancellationToken);

        return new CreatePlayerResult(playerId);
    }
}
