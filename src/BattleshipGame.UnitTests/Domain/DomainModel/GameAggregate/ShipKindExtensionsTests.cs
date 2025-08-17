using System.Collections.Generic;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate;

public class ShipKindExtensionsTests
{
    [Theory]
    [InlineData(ShipKind.Destroyer, 2)]
    [InlineData(ShipKind.Cruiser, 3)]
    [InlineData(ShipKind.Submarine, 3)]
    [InlineData(ShipKind.Battleship, 4)]
    [InlineData(ShipKind.Carrier, 5)]
    public void ToSize(ShipKind shipKind, int expectedSize)
    {
        shipKind.ToSize().Should().Be(expectedSize);
    }

    [Fact]
    public void ToSize_WithShipKindNone_ThrowsKeyNotFoundException()
    {
        // Arrange
        var shipKind = ShipKind.None;

        // Act & Assert
        shipKind.Invoking(s => s.ToSize())
            .Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void ToSize_WithInvalidShipKindValue_ThrowsKeyNotFoundException()
    {
        // Arrange
        var invalidShipKind = (ShipKind)999;

        // Act & Assert
        invalidShipKind.Invoking(s => s.ToSize())
            .Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void ToSize_WithNegativeShipKindValue_ThrowsKeyNotFoundException()
    {
        // Arrange
        var negativeShipKind = (ShipKind)(-1);

        // Act & Assert
        negativeShipKind.Invoking(s => s.ToSize())
            .Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void ToSize_DictionaryContainsAllValidShipKinds_ReturnsExpectedCount()
    {
        // Arrange
        var validShipKinds = new[]
        {
            ShipKind.Destroyer,
            ShipKind.Cruiser,
            ShipKind.Submarine,
            ShipKind.Battleship,
            ShipKind.Carrier
        };

        // Act & Assert
        validShipKinds.Should().HaveCount(5);

        foreach (var shipKind in validShipKinds)
        {
            shipKind.Invoking(s => s.ToSize())
                .Should().NotThrow();
        }
    }

    [Fact]
    public void ToSize_CalledMultipleTimesWithSameInput_ReturnsConsistentResults()
    {
        // Arrange
        var shipKind = ShipKind.Battleship;

        // Act
        var firstCall = shipKind.ToSize();
        var secondCall = shipKind.ToSize();
        var thirdCall = shipKind.ToSize();

        // Assert
        firstCall.Should().Be(4);
        secondCall.Should().Be(4);
        thirdCall.Should().Be(4);

        firstCall.Should().Be(secondCall);
        secondCall.Should().Be(thirdCall);
    }
}
