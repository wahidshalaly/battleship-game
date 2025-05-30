using BattleshipChallenge.Domain;
using FluentAssertions;
using Xunit;

namespace BattleshipChallenge.UnitTests.Domain;

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
}
