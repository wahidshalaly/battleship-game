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
}
