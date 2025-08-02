using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Application.Features.Players.Queries.GetPlayer;
using MediatR;

namespace BattleshipGame.Application.Features.Players.Queries.GetPlayerByUsername;

/// <summary>
/// Handler for getting a player by username.
/// </summary>
public class GetPlayerByUsernameQueryHandler : IRequestHandler<GetPlayerByUsernameQuery, GetPlayerResult?>
{
    private readonly IPlayerRepository _playerRepository;

    /// <summary>
    /// Initializes a new instance of the GetPlayerByUsernameQueryHandler class.
    /// </summary>
    /// <param name="playerRepository">The player repository.</param>
    public GetPlayerByUsernameQueryHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    /// <inheritdoc />
    public async Task<GetPlayerResult?> Handle(GetPlayerByUsernameQuery request, CancellationToken cancellationToken)
    {
        var player = await _playerRepository.GetByUsernameAsync(request.Username, cancellationToken);

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
