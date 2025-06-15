using System.Threading.Tasks;
using BattleshipGame.Domain.Common;
using Microsoft.Extensions.Logging;

namespace BattleshipGame.Domain.GameAggregate;

internal class GameStartedHandler : IDomainEventHandler<GameStarted>
{
    private readonly ILogger<GameStartedHandler> _logger;

    public GameStartedHandler(ILogger<GameStartedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(GameStarted evt)
    {
        _logger.LogWarning("Did not implement GameStartedHandler.HandleAsync yet!");
        throw new System.NotImplementedException(
            "Did not implement GameStartedHandler.HandleAsync yet!"
        );
    }
}
