using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Interfaces.Broadcasting;
using BattleshipGame.Application.Interfaces.ComputerOpponent;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Infrastructure.Broadcasting;
using BattleshipGame.Infrastructure.ComputerOpponent;
using BattleshipGame.Infrastructure.Persistence;
using BattleshipGame.Infrastructure.Persistence.Repositories;
using BattleshipGame.Infrastructure.Resilience;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Polly;

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
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        RegisterSemanticKernel(services);
        RegisterOpponentStrategies(services);
        RegisterResiliencePolicies(services, configuration);
        RegisterPersistence(services, configuration);

        // Register repositories (singleton for in-memory, will change when using EF Core)
        services.AddSingleton<IBroadcastor, Broadcaster>();
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }

    private static void RegisterSemanticKernel(IServiceCollection services)
    {
        var modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID");
        var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        // Validate OpenAI configuration
        if (
            string.IsNullOrEmpty(modelId)
            || string.IsNullOrEmpty(endpoint)
            || string.IsNullOrEmpty(apiKey)
        )
        {
            throw new InvalidOperationException(
                "OpenAI configuration incomplete. Set environment variables: OPENAI_MODEL_ID, OPENAI_ENDPOINT, and OPENAI_API_KEY."
            );
        }

#pragma warning disable SKEXP0010
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId,
                endpoint: new Uri(endpoint),
                apiKey,
                httpClient: new HttpClient { Timeout = TimeSpan.FromMinutes(5) }
            )
            .Build();
#pragma warning restore SKEXP0010

        // Register Semantic Kernel instance
        services.AddSingleton(kernel);
    }

    private static void RegisterOpponentStrategies(IServiceCollection services)
    {
        // Register prompt builder for LLM-based strategies
        services.AddSingleton<IPromptBuilder, BattleshipPromptBuilder>();

        // Register Basic Opponent (Random strategy)
        services.AddKeyedScoped<IComputerOpponent, RandomAttackOpponent>(OpponentStrategy.Random);

        // Register base SemanticKernelOpponent (needed for decorator)
        services.AddScoped<SemanticKernelOpponent>();

        // Register AI Opponent (LLM-based strategy) with resilience decorator
        services.AddKeyedScoped<IComputerOpponent>(
            OpponentStrategy.SemanticKernel,
            (provider, key) =>
            {
                var baseOpponent = provider.GetRequiredService<SemanticKernelOpponent>();
                var pipeline = provider.GetRequiredService<ResiliencePipeline<string>>();
                // Get fallback opponent directly to avoid circular dependency with factory
                var fallbackOpponent = provider.GetRequiredKeyedService<IComputerOpponent>(
                    OpponentStrategy.Random
                );
                var logger = provider.GetRequiredService<
                    ILogger<ResilientComputerOpponentDecorator>
                >();

                return new ResilientComputerOpponentDecorator(
                    baseOpponent,
                    pipeline,
                    fallbackOpponent,
                    logger
                );
            }
        );

        // Register strategy factory
        services.AddScoped<IComputerOpponentFactory, ComputerOpponentFactory>();
    }

    private static void RegisterResiliencePolicies(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Configure and register resilience options
        services.Configure<AiOpponentResilienceOptions>(
            configuration.GetSection("Resilience:AiOpponent")
        );

        // Register the resilience pipeline
        services.AddSingleton(provider =>
        {
            var options = provider
                .GetRequiredService<IOptions<AiOpponentResilienceOptions>>()
                .Value;
            var logger = provider.GetRequiredService<ILogger<ResiliencePipeline<string>>>();

            return AiOpponentResiliencePolicyFactory.CreateResiliencePipeline<string>(
                options,
                logger
            );
        });
    }

    private static void RegisterPersistence(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<BattleshipGameDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("battleship"))
        );

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
