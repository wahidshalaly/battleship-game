using BattleshipGame.Application.Common.Security;
using BattleshipGame.Application.Common.Services;
using BattleshipGame.Application.Interfaces.Broadcasting;
using BattleshipGame.Application.Interfaces.ComputerOpponent;
using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Infrastructure.Broadcasting;
using BattleshipGame.Infrastructure.ComputerOpponent;
using BattleshipGame.Infrastructure.Identity;
using BattleshipGame.Infrastructure.Persistence;
using BattleshipGame.Infrastructure.Persistence.Repositories;
using BattleshipGame.Infrastructure.Resilience;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Connectors.OpenAI;
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
        RegisterIdentity(services, configuration);
        RegisterLlmChat(services, configuration);
        RegisterOpponentStrategies(services);
        RegisterResiliencePolicies(services, configuration);
        RegisterPersistence(services, configuration);

        services.AddSingleton<IBroadcastor, Broadcaster>();

        // Scoped so event handlers resolve from the request scope and share its DbContext.
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }

    private static void RegisterIdentity(IServiceCollection services, IConfiguration configuration)
    {
        // Fail fast at startup if any Keycloak setting is missing rather than on the first
        // register/sign-in call.
        services
            .AddOptions<KeycloakOptions>()
            .Bind(configuration.GetSection(KeycloakOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services
            .AddHttpClient<IIdentityProvider, KeycloakIdentityProvider>()
            .ConfigureHttpClient(c => c.Timeout = TimeSpan.FromSeconds(30));
    }

    private static void RegisterLlmChat(IServiceCollection services, IConfiguration configuration)
    {
        // Fail fast at startup if any LLM setting is missing rather than on the first AI move.
        services
            .AddOptions<OpenAiOptions>()
            .Bind(configuration.GetSection(OpenAiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // The opponent depends on ILlmChat; Semantic Kernel stays behind this adapter.
        services.AddSingleton<ILlmChat>(CreateLlmChat);
    }

    private static SemanticKernelChat CreateLlmChat(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<OpenAiOptions>>().Value;

#pragma warning disable SKEXP0010
        var chatCompletion = new OpenAIChatCompletionService(
            options.ModelId,
            new Uri(options.Endpoint),
            options.ApiKey,
            httpClient: new HttpClient { Timeout = TimeSpan.FromMinutes(5) }
        );
#pragma warning restore SKEXP0010

        return new SemanticKernelChat(chatCompletion);
    }

    private static void RegisterOpponentStrategies(IServiceCollection services)
    {
        // Register prompt builder for LLM-based strategies
        services.AddSingleton<IPromptBuilder, BattleshipPromptBuilder>();

        // Register Basic Opponent (Random strategy)
        services.AddKeyedScoped<IComputerOpponent, RandomAttackOpponent>(OpponentStrategy.Random);

        // Register base LlmOpponent (needed for decorator)
        services.AddScoped<LlmOpponent>();

        // Register AI Opponent (LLM-based strategy) with resilience decorator
        services.AddKeyedScoped<IComputerOpponent>(
            OpponentStrategy.SemanticKernel,
            (provider, key) =>
            {
                var baseOpponent = provider.GetRequiredService<LlmOpponent>();
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
