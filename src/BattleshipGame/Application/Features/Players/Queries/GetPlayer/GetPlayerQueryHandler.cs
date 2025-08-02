using BattleshipGame.Application.Contracts.Persistence;
using MediatR;

namespace BattleshipGame.Application.Features.Players.Queries.GetPlayer;

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

        if (player is null)
            return null;

        return new GetPlayerResult(
            player.Id,
            player.Username,
            player.ActiveGameId?.Value,
            player.TotalGamesPlayed
        );
    }
}
