using System.Threading;
using System.Threading.Tasks;
using BattleshipGame.Domain.Entities;
using BattleshipGame.Domain.Events;
using BattleshipGame.Infrastructure;

namespace BattleshipGame.Domain.Services;

public class GameService(IEventBus eventBus)
{
    private readonly CancellationTokenSource _tokenSource = new();

    public ValueTask StartNew()
    {
        var game = new Game();
        return eventBus.PublishAsync(new GameStarted(game.Id), _tokenSource.Token);
    }
}
