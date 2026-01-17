using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Contracts.OpponentStrategy;
using BattleshipGame.Application.Contracts.Persistence;
using BattleshipGame.Infrastructure.OpponentStrategy;
using BattleshipGame.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

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
    /// <param name="configuration">The configuration for AI strategy selection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Register Semantic Kernel with Ollama or Azure OpenAI
        var modelId = configuration.GetValue<string>("SemanticKernel:ModelId");
        var endpoint = configuration.GetValue<string>("SemanticKernel:Endpoint");
        var apiKey = configuration.GetValue<string>("SemanticKernel:ApiKey");

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(modelId))
        {
            throw new InvalidOperationException(
                "SemanticKernel configuration missing. Please, set `SemanticKernel` configuration section."
            );
        }

#pragma warning disable SKEXP0010
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(modelId: modelId, endpoint: new Uri(endpoint), apiKey: apiKey)
            .Build();
#pragma warning restore SKEXP0010

        // Register Semantic Kernel instance
        services.AddSingleton(kernel);

        // Register opponent strategies
        services.AddScoped<IComputerOpponentStrategy, RandomAttackStrategy>();
        services.AddScoped<IComputerOpponentStrategy, SemanticKernelStrategy>();

        // Register AI services
        services.AddScoped<GameStateAnalyzer>();

        // Register repositories (singleton for in-memory, will be scoped when using EF Core)
        services.AddSingleton<IGameRepository, InMemoryGameRepository>();
        services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();
        services.AddSingleton<IBroadcastRepository, InMemoryBroadcastRepository>();
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}
