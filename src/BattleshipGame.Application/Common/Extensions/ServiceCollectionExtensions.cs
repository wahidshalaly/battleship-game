using System.Reflection;
using BattleshipGame.Application.Common.Behaviors;
using BattleshipGame.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BattleshipGame.Application.Common.Extensions;

/// <summary>
/// Extension methods for registering Application layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Application layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR with handlers from Application assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Register MediatR pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        // Register application services
        services.AddScoped<IGameplayService, GameplayService>();
        services.AddScoped<IPlayerService, PlayerService>();

        return services;
    }
}
