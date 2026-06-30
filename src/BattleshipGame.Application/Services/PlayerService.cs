using BattleshipGame.Application.Common.Exceptions;
using BattleshipGame.Application.Common.Security;
using BattleshipGame.Application.Features.Players.Commands;
using BattleshipGame.Application.Features.Players.Queries;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Services;

/// <inheritdoc />
public class PlayerService(IMediator mediator, ICurrentUser currentUser) : IPlayerService
{
    /// <inheritdoc />
    public async Task<PlayerId> CreateAsync(
        string username,
        string identitySubject,
        CancellationToken ct
    )
    {
        var guid = await mediator.Send(new CreatePlayerCommand(username, identitySubject), ct);
        return new PlayerId(guid);
    }

    /// <inheritdoc />
    public async Task<GetPlayerQueryResult?> GetByIdAsync(PlayerId id, CancellationToken ct) =>
        await mediator.Send(new GetPlayerByIdQuery(id), ct);

    /// <inheritdoc />
    public async Task<GetPlayerQueryResult?> GetByUsernameAsync(
        string username,
        CancellationToken ct
    ) => await mediator.Send(new GetPlayerByUsernameQuery(username), ct);

    /// <inheritdoc />
    public async Task<Player?> GetCurrentAsync(CancellationToken ct)
    {
        var subject = currentUser.SubjectId;
        return string.IsNullOrEmpty(subject)
            ? null
            : await mediator.Send(new GetPlayerByIdentitySubjectQuery(subject), ct);
    }

    /// <inheritdoc />
    public async Task<Player> GetCurrentRequiredAsync(CancellationToken ct) =>
        await GetCurrentAsync(ct)
        ?? throw new ForbiddenAccessException(
            "No game profile found for the current identity. Register via POST /api/auth/register."
        );
}
