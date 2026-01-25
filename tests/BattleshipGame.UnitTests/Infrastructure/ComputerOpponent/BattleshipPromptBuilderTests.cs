using System.Collections.Generic;
using System.Linq;
using BattleshipGame.Application.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Infrastructure.ComputerOpponent;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Infrastructure.ComputerOpponent;

public class BattleshipPromptBuilderTests
{
    private readonly BattleshipPromptBuilder _promptBuilder = new();

    [Fact]
    public void BuildSystemPrompt_ShouldReturnValidPrompt()
    {
        // Act
        var prompt = _promptBuilder.BuildSystemPrompt();

        // Assert
        prompt.Should().NotBeNullOrWhiteSpace();
        prompt.Should().Contain("Battleship strategist");
        prompt.Should().Contain("JSON");
        prompt.Should().Contain("cell");
        prompt.Should().Contain("reasoning");
    }

    [Fact]
    public void BuildStrategicPrompt_ShouldIncludeAllAvailableTargets()
    {
        // Arrange
        var availableTargets = new List<string> { "A1", "A2", "B1", "B2", "C3" };
        var snapshot = new GameSnapshot
        {
            BoardSize = 10,
            GameState = GameState.Started,
            BoardDescription = "A1 to J10 (10x10 grid)",
            AvailableTargets = availableTargets,
            Hits = new List<string> { "E3", "E5" },
            Misses = new List<string> { "E1", "E2", "E4" },
        };

        // Act
        var prompt = _promptBuilder.BuildStrategicPrompt(snapshot);

        // Assert
        prompt.Should().Contain("VALID TARGETS");
        prompt.Should().Contain("5 cells available"); // Count of available targets

        // Verify each available target is in the prompt
        foreach (var target in availableTargets)
        {
            prompt.Should().Contain(target);
        }
    }

    [Fact]
    public void BuildStrategicPrompt_ShouldNotIncludeAttackedCellsInValidTargets()
    {
        // Arrange
        var availableTargets = new List<string> { "A1", "B1", "C1" };
        var snapshot = new GameSnapshot
        {
            BoardSize = 10,
            GameState = GameState.Started,
            BoardDescription = "A1 to J10 (10x10 grid)",
            AvailableTargets = availableTargets,
            Hits = new List<string> { "E3", "E5" },
            Misses = new List<string> { "E1", "E2", "E4" },
        };

        // Act
        var prompt = _promptBuilder.BuildStrategicPrompt(snapshot);

        // Assert
        var validTargetsSection = ExtractValidTargetsSection(prompt);

        // Attacked cells should NOT be in valid targets section
        validTargetsSection.Should().NotContain("E3");
        validTargetsSection.Should().NotContain("E5");
        validTargetsSection.Should().NotContain("E1");
        validTargetsSection.Should().NotContain("E2");
        validTargetsSection.Should().NotContain("E4");

        // Available targets should be in valid targets section
        validTargetsSection.Should().Contain("A1");
        validTargetsSection.Should().Contain("B1");
        validTargetsSection.Should().Contain("C1");
    }

    [Fact]
    public void BuildStrategicPrompt_ShouldIncludeCriticalRules()
    {
        // Arrange
        var snapshot = new GameSnapshot
        {
            BoardSize = 10,
            GameState = GameState.Started,
            BoardDescription = "A1 to J10 (10x10 grid)",
            AvailableTargets = new List<string> { "A1", "B1" },
            Hits = new List<string>(),
            Misses = new List<string>(),
        };

        // Act
        var prompt = _promptBuilder.BuildStrategicPrompt(snapshot);

        // Assert
        prompt.Should().Contain("CRITICAL RULES");
        prompt.Should().Contain("MUST select a cell from the VALID TARGETS list");
        prompt.Should().Contain("NEVER select cells from HITS or MISSES");
        prompt.Should().Contain("Double-check your selection");
    }

    [Fact]
    public void BuildStrategicPrompt_ShouldIncludeAttackHistory()
    {
        // Arrange
        var snapshot = new GameSnapshot
        {
            BoardSize = 10,
            GameState = GameState.Started,
            BoardDescription = "A1 to J10 (10x10 grid)",
            AvailableTargets = new List<string> { "A1" },
            Hits = new List<string> { "E3", "E5" },
            Misses = new List<string> { "E1", "E2", "E4" },
        };

        // Act
        var prompt = _promptBuilder.BuildStrategicPrompt(snapshot);

        // Assert
        prompt.Should().Contain("ATTACK HISTORY");
        prompt.Should().Contain("HITS: E3, E5");
        prompt.Should().Contain("MISSES: E1, E2, E4");
    }

    [Fact]
    public void BuildStrategicPrompt_WhenNoAttacks_ShouldDisplayNone()
    {
        // Arrange
        var snapshot = new GameSnapshot
        {
            BoardSize = 10,
            GameState = GameState.Started,
            BoardDescription = "A1 to J10 (10x10 grid)",
            AvailableTargets = new List<string> { "A1", "A2" },
            Hits = new List<string>(),
            Misses = new List<string>(),
        };

        // Act
        var prompt = _promptBuilder.BuildStrategicPrompt(snapshot);

        // Assert
        prompt.Should().Contain("HITS: None");
        prompt.Should().Contain("MISSES: None");
    }

    [Fact]
    public void BuildStrategicPrompt_ShouldIncludeStrategyTips()
    {
        // Arrange
        var snapshot = new GameSnapshot
        {
            BoardSize = 10,
            GameState = GameState.Started,
            BoardDescription = "A1 to J10 (10x10 grid)",
            AvailableTargets = new List<string> { "A1" },
            Hits = new List<string>(),
            Misses = new List<string>(),
        };

        // Act
        var prompt = _promptBuilder.BuildStrategicPrompt(snapshot);

        // Assert
        prompt.Should().Contain("STRATEGY TIPS");
        prompt.Should().Contain("adjacent cells");
        prompt.Should().Contain("highest probability");
    }

    [Fact]
    public void BuildStrategicPrompt_ShouldSortTargetsAlphabetically()
    {
        // Arrange
        var unsortedTargets = new List<string> { "C3", "A1", "B2", "A2" };
        var snapshot = new GameSnapshot
        {
            BoardSize = 10,
            GameState = GameState.Started,
            BoardDescription = "A1 to J10 (10x10 grid)",
            AvailableTargets = unsortedTargets,
            Hits = new List<string>(),
            Misses = new List<string>(),
        };

        // Act
        var prompt = _promptBuilder.BuildStrategicPrompt(snapshot);

        // Assert
        var validTargetsSection = ExtractValidTargetsSection(prompt);
        var indexA1 = validTargetsSection.IndexOf("A1");
        var indexA2 = validTargetsSection.IndexOf("A2");
        var indexB2 = validTargetsSection.IndexOf("B2");
        var indexC3 = validTargetsSection.IndexOf("C3");

        // Should be in alphabetical order
        indexA1.Should().BeLessThan(indexA2);
        indexA2.Should().BeLessThan(indexB2);
        indexB2.Should().BeLessThan(indexC3);
    }

    private static string ExtractValidTargetsSection(string prompt)
    {
        var startMarker = "VALID TARGETS";
        var endMarker = "CRITICAL RULES";

        var startIndex = prompt.IndexOf(startMarker);
        var endIndex = prompt.IndexOf(endMarker);

        if (startIndex == -1 || endIndex == -1)
            return string.Empty;

        return prompt.Substring(startIndex, endIndex - startIndex);
    }
}
