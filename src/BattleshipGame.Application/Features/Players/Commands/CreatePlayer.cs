using BattleshipGame.Application.Common.Exceptions;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Players.Commands;

/// <summary>
/// Command to create a new player.
/// </summary>
/// <param name="Username">The player's username.</param>
/// <param name="IdentitySubject">The authenticated caller's identity subject (token 'sub').</param>
public record CreatePlayerCommand(string Username, string IdentitySubject) : IRequest<Guid>;

/// <summary>
/// Handler for creating a new player.
/// </summary>
/// <param name="playerRepository">The player repository.</param>
public class CreatePlayerCommandHandler(IPlayerRepository playerRepository)
    : IRequestHandler<CreatePlayerCommand, Guid>
{
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <inheritdoc />
    public async Task<Guid> Handle(CreatePlayerCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException("Username cannot be null or whitespace.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.IdentitySubject))
        {
            throw new ForbiddenAccessException("Authentication is required to create a player.");
        }

        // A single player profile per authenticated identity.
        if (
            await playerRepository.GetByIdentitySubjectAsync(request.IdentitySubject, ct)
            is not null
        )
        {
            throw new InvalidOperationException(
                "A player profile already exists for the current identity."
            );
        }

        // Validate username uniqueness
        if (await playerRepository.UsernameExistsAsync(request.Username, ct))
        {
            throw new InvalidOperationException(
                $"A player with username '{request.Username}' already exists."
            );
        }

        // Create new player
        var playerId = new PlayerId(Guid.NewGuid());
        var player = new Player(playerId, request.Username, request.IdentitySubject);

        // Save player
        await playerRepository.SaveAsync(player, ct);

        return playerId;
    }
}
