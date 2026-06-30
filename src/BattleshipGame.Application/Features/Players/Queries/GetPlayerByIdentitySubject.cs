using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using MediatR;

namespace BattleshipGame.Application.Features.Players.Queries;

public record GetPlayerByIdentitySubjectQuery(string IdentitySubject) : IRequest<Player?>;

public class GetPlayerByIdentitySubjectHandler(IPlayerRepository playerRepository)
    : IRequestHandler<GetPlayerByIdentitySubjectQuery, Player?>
{
    public async Task<Player?> Handle(
        GetPlayerByIdentitySubjectQuery request,
        CancellationToken ct
    ) => await playerRepository.GetByIdentitySubjectAsync(request.IdentitySubject, ct);
}
