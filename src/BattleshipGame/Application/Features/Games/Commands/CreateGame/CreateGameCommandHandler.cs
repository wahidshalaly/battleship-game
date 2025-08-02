using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Commands.CreateGame;

/// <summary>
/// Handler for creating a new game.
/// </summary>
public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, CreateGameResult>
{
    private readonly IGameRepository _gameRepository;

    /// <summary>
    /// Initializes a new instance of the CreateGameCommandHandler class.
    /// </summary>
    /// <param name="gameRepository">The game repository.</param>
    public CreateGameCommandHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    /// <inheritdoc />
    public async Task<CreateGameResult> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        // Create new game
        var game = new Game(request.PlayerId, request.BoardSize ?? 10);

        // Save game
        await _gameRepository.SaveAsync(game, cancellationToken);

        return new CreateGameResult(game.Id, game.PlayerId, game.BoardSize, game.State.ToString());
    }
}
