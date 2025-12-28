using BattleshipGame.Application.Features.Players.Commands;
using BattleshipGame.Application.Features.Players.Queries;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Services;

/// <inheritdoc />
public class PlayerService(IMediator mediator) : IPlayerService
{
    /// <inheritdoc />
    public async Task<PlayerId> CreateAsync(string username, CancellationToken ct)
    {
        var guid = await mediator.Send(new CreatePlayerCommand(username), ct);
        return new PlayerId(guid);
    }

    /// <inheritdoc />
    public async Task<GetPlayerQueryResult?> GetByIdAsync(PlayerId id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPlayerByIdQuery(id), ct);
        return result;
    }

    /// <inheritdoc />
    public async Task<GetPlayerQueryResult?> GetByUsernameAsync(
        string username,
        CancellationToken ct
    )
    {
        var result = await mediator.Send(new GetPlayerByUsernameQuery(username), ct);
        return result;
    }
}
