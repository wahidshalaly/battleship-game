using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.OpponentStrategy;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Infrastructure.OpponentStrategy;
using BattleshipGame.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace BattleshipGame.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering Infrastructure layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Infrastructure layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register opponent strategies
        services.AddScoped<IComputerOpponentStrategy, RandomAttackStrategy>();
        services.AddKeyedScoped<IComputerOpponentStrategy, SmartAttackStrategy>("AI-Based");

        // Register repositories (singleton for in-memory, will be scoped when using EF Core)
        services.AddSingleton<IGameRepository, InMemoryGameRepository>();
        services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();
        services.AddSingleton<IBroadcastRepository, InMemoryBroadcastRepository>();
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}
