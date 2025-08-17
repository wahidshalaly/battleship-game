using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Players.Queries;

/// <summary>
/// Query to get a player by ID.
/// </summary>
/// <param name="PlayerId">The player identifier.</param>
public record GetPlayerQuery(PlayerId PlayerId) : IRequest<GetPlayerResult>;

/// <summary>
/// Result of getting a player.
/// </summary>
/// <param name="PlayerId">The player's identifier.</param>
/// <param name="Username">The player's username.</param>
/// <param name="ActiveGameId">The currently active game ID, if any.</param>
/// <param name="TotalGamesPlayed">The total number of games played.</param>
public record GetPlayerResult(
    PlayerId PlayerId,
    string Username,
    Guid? ActiveGameId,
    int TotalGamesPlayed
);

/// <summary>
/// Handler for getting a player by ID.
/// </summary>
public class GetPlayerQueryHandler : IRequestHandler<GetPlayerQuery, GetPlayerResult?>
{
    private readonly IPlayerRepository _playerRepository;

    /// <summary>
    /// Initializes a new instance of the GetPlayerQueryHandler class.
    /// </summary>
    /// <param name="playerRepository">The player repository.</param>
    public GetPlayerQueryHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    /// <inheritdoc />
    public async Task<GetPlayerResult?> Handle(GetPlayerQuery request, CancellationToken cancellationToken)
    {
        var player = await _playerRepository.GetByIdAsync(request.PlayerId, cancellationToken);

        if (player is null) return null;

        return new GetPlayerResult(
            player.Id,
            player.Username,
            player.ActiveGameId?.Value,
            player.TotalGamesPlayed
        );
    }
}
