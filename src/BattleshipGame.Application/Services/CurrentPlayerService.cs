using BattleshipGame.Application.Common.Exceptions;
using BattleshipGame.Application.Common.Security;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.Application.Services;

/// <inheritdoc />
public sealed class CurrentPlayerService(
    ICurrentUser currentUser,
    IPlayerRepository playerRepository
) : ICurrentPlayerService
{
    /// <inheritdoc />
    public async Task<Player?> GetAsync(CancellationToken ct)
    {
        var subject = currentUser.SubjectId;
        return string.IsNullOrEmpty(subject)
            ? null
            : await playerRepository.GetByIdentitySubjectAsync(subject, ct);
    }

    /// <inheritdoc />
    public async Task<Player> GetRequiredAsync(CancellationToken ct) =>
        await GetAsync(ct)
        ?? throw new ForbiddenAccessException(
            "No player profile exists for the current identity. Create a player first."
        );
}
