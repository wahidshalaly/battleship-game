using BattleshipGame.Application.Contracts.Persistence;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Queries.GetGame;

/// <summary>
/// Handler for getting a game by ID.
/// </summary>
public class GetGameQueryHandler : IRequestHandler<GetGameQuery, GetGameResult?>
{
    private readonly IGameRepository _gameRepository;

    /// <summary>
    /// Initializes a new instance of the GetGameQueryHandler class.
    /// </summary>
    /// <param name="gameRepository">The game repository.</param>
    public GetGameQueryHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    /// <inheritdoc />
    public async Task<GetGameResult?> Handle(GetGameQuery request, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.GameId, cancellationToken);

        if (game is null)
            return null;

        return new GetGameResult(
            game.Id,
            game.PlayerId.Value,
            game.BoardSize,
            game.State.ToString()
        );
    }
}
