using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Games.Queries;

public record GetActiveGameQuery(PlayerId PlayerId) : IRequest<GetGameQueryResult?>;

internal class GetActiveGameHandler(IGameRepository gameRepository)
    : IRequestHandler<GetActiveGameQuery, GetGameQueryResult?>
{
    public async Task<GetGameQueryResult?> Handle(GetActiveGameQuery request, CancellationToken ct)
    {
        var game = await gameRepository.GetActiveGameByPlayerIdAsync(request.PlayerId, ct);
        return game is null ? null : GetGameQueryResult.From(game);
    }
}
