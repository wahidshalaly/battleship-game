using BattleshipGame.Application.Common.Exceptions;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.Exceptions;

namespace BattleshipGame.Application.Services;

/// <inheritdoc />
public sealed class GameAccessGuard(
    ICurrentPlayerService currentPlayer,
    IGameRepository gameRepository
) : IGameAccessGuard
{
    /// <inheritdoc />
    public async Task EnsureOwnerAsync(GameId gameId, CancellationToken ct)
    {
        var player = await currentPlayer.GetRequiredAsync(ct);

        var game =
            await gameRepository.GetByIdAsync(gameId, ct)
            ?? throw new GameNotFoundException(gameId);

        if (game.PlayerId != player.Id)
            throw new ForbiddenAccessException("You do not have access to this game.");
    }
}
