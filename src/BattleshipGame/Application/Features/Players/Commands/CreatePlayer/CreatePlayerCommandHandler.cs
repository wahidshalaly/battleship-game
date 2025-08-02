using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Players.Commands.CreatePlayer;

/// <summary>
/// Handler for creating a new player.
/// </summary>
public class CreatePlayerCommandHandler : IRequestHandler<CreatePlayerCommand, CreatePlayerResult>
{
    private readonly IPlayerRepository _playerRepository;

    /// <summary>
    /// Initializes a new instance of the CreatePlayerCommandHandler class.
    /// </summary>
    /// <param name="playerRepository">The player repository.</param>
    public CreatePlayerCommandHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    /// <inheritdoc />
    public async Task<CreatePlayerResult> Handle(CreatePlayerCommand request, CancellationToken cancellationToken)
    {
        // Validate username uniqueness
        if (await _playerRepository.UsernameExistsAsync(request.Username, cancellationToken))
        {
            throw new InvalidOperationException($"A player with username '{request.Username}' already exists.");
        }

        // Create new player
        var playerId = new PlayerId(Guid.NewGuid());
        var player = new Player(playerId, request.Username);

        // Save player
        await _playerRepository.SaveAsync(player, cancellationToken);

        return new CreatePlayerResult(playerId, player.Username);
    }
}
