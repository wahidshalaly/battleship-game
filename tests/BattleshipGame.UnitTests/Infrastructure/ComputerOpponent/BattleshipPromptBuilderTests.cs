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
        var snapshot = new GameSnapshot
        {
            BoardSize = 10,
            GameState = GameState.Started,
            BoardDescription = "A1 to J10 (10x10 grid)",
            AvailableTargets = ["A1", "A2", "B1", "B2", "C3"],
            Hits = ["E3", "E5"],
            Misses = ["E1", "E2", "E4"],
        };

        // Act
        var prompt = _promptBuilder.BuildStrategicPrompt(snapshot);

        // Assert
        prompt.Should().Contain("VALID TARGETS");
        prompt.Should().Contain("5 cells available"); // Count of available targets

        // Verify each available target is in the prompt
        foreach (var target in snapshot.AvailableTargets)
        {
            prompt.Should().Contain(target);
        }
    }

    [Fact]
    public void BuildStrategicPrompt_ShouldNotIncludeAttackedCellsInValidTargets()
    {
        // Arrange
        var snapshot = new GameSnapshot
        {
            BoardSize = 10,
            GameState = GameState.Started,
            BoardDescription = "A1 to J10 (10x10 grid)",
            AvailableTargets = ["A1", "B1", "C1"],
            Hits = ["E3", "E5"],
            Misses = ["E1", "E2", "E4"],
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
            AvailableTargets = ["A1", "B1"],
            Hits = [],
            Misses = [],
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
            AvailableTargets = ["A1"],
            Hits = ["E3", "E5"],
            Misses = ["E1", "E2", "E4"],
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
            AvailableTargets = ["A1", "A2"],
            Hits = [],
            Misses = [],
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
            AvailableTargets = ["A1"],
            Hits = [],
            Misses = [],
        };

        // Act
        var prompt = _promptBuilder.BuildStrategicPrompt(snapshot);

        // Assert
        prompt.Should().Contain("STRATEGY TIPS");
        prompt.Should().Contain("adjacent cells");
        prompt.Should().Contain("highest probability");
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
