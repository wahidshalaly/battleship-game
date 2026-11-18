using System.Collections.Generic;
using BattleshipGame.Infrastructure.ComputerOpponent;
using BattleshipGame.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BattleshipGame.UnitTests.Infrastructure.ComputerOpponent;

public class OpenAiOptionsTests
{
    private static readonly Dictionary<string, string?> ValidSettings = new()
    {
        ["OpenAi:ModelId"] = "test-model",
        ["OpenAi:Endpoint"] = "http://localhost:11434/v1",
        ["OpenAi:ApiKey"] = "test-key",
    };

    [Fact]
    public void Options_WhenAllSettingsPresent_BindsFromOpenAiSection()
    {
        // Arrange
        using var provider = BuildProvider(ValidSettings);

        // Act
        var options = provider.GetRequiredService<IOptions<OpenAiOptions>>().Value;

        // Assert
        options.ModelId.Should().Be("test-model");
        options.Endpoint.Should().Be("http://localhost:11434/v1");
        options.ApiKey.Should().Be("test-key");
    }

    [Theory]
    [InlineData("OpenAi:ModelId", nameof(OpenAiOptions.ModelId))]
    [InlineData("OpenAi:Endpoint", nameof(OpenAiOptions.Endpoint))]
    [InlineData("OpenAi:ApiKey", nameof(OpenAiOptions.ApiKey))]
    public void Options_WhenSettingMissing_FailsValidationNamingIt(string key, string property)
    {
        // Arrange
        var settings = new Dictionary<string, string?>(ValidSettings);
        settings.Remove(key);
        using var provider = BuildProvider(settings);

        // Act
        var act = () => provider.GetRequiredService<IOptions<OpenAiOptions>>().Value;

        // Assert
        act.Should()
            .Throw<OptionsValidationException>()
            .Which.Failures.Should()
            .ContainSingle(failure => failure.Contains(property));
    }

    [Theory]
    [InlineData("localhost:11434/v1")]
    [InlineData("/v1")]
    [InlineData("not a url")]
    public void Options_WhenEndpointNotAbsoluteHttpUrl_FailsValidation(string endpoint)
    {
        // Arrange
        var settings = new Dictionary<string, string?>(ValidSettings)
        {
            ["OpenAi:Endpoint"] = endpoint,
        };
        using var provider = BuildProvider(settings);

        // Act
        var act = () => provider.GetRequiredService<IOptions<OpenAiOptions>>().Value;

        // Assert
        act.Should()
            .Throw<OptionsValidationException>()
            .Which.Failures.Should()
            .ContainSingle(failure => failure.Contains(nameof(OpenAiOptions.Endpoint)));
    }

    [Fact]
    public void LlmChat_WhenSettingsValid_ResolvesSemanticKernelChat()
    {
        // Arrange
        using var provider = BuildProvider(ValidSettings);

        // Act
        var chat = provider.GetRequiredService<ILlmChat>();

        // Assert
        chat.Should().BeOfType<SemanticKernelChat>();
    }

    [Fact]
    public void AddInfrastructureServices_WhenOpenAiSettingsMissing_DoesNotThrow()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddInfrastructureServices(configuration);

        // Assert
        act.Should().NotThrow("missing settings are reported by options validation at startup");
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructureServices(configuration);
        return services.BuildServiceProvider();
    }
}
