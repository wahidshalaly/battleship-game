using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Interfaces.ComputerOpponent;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Infrastructure.ComputerOpponent;
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
    /// <param name="configuration">The configuration for AI opponent.</param>
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

        // Register prompt builder for LLM-based strategies
        services.AddSingleton<IPromptBuilder, BattleshipPromptBuilder>();

        // Register opponent strategies using keyed services for per-game selection
        services.AddKeyedScoped<IComputerOpponent, RandomAttackOpponent>(OpponentStrategy.Random);
        services.AddKeyedScoped<IComputerOpponent, SemanticKernelOpponent>(
            OpponentStrategy.SemanticKernel
        );

        // Register strategy factory
        services.AddScoped<IComputerOpponentFactory, ComputerOpponentFactory>();

        // Register repositories (singleton for in-memory, will be scoped when using EF Core)
        services.AddSingleton<IGameRepository, InMemoryGameRepository>();
        services.AddSingleton<IPlayerRepository, InMemoryPlayerRepository>();
        services.AddSingleton<IBroadcastRepository, InMemoryBroadcastRepository>();
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}
