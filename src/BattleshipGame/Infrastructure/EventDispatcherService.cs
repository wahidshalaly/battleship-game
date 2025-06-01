using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace BattleshipGame.Infrastructure;

public class EventDispatcherService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new System.NotImplementedException();
    }
}
