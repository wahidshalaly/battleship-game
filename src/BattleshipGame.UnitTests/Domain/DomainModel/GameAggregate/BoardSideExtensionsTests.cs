using System;
using BattleshipGame.Domain.DomainModel.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate;

public class BoardSideExtensionsTests
{
    [Theory]
    [InlineData(BoardSide.Own, BoardSide.Opp)]
    [InlineData(BoardSide.Opp, BoardSide.Own)]
    public void OppositeSide_WhenValid_ReturnsOpposite(BoardSide boardSide, BoardSide oppositeSide)
    {
        boardSide.OppositeSide().Should().Be(oppositeSide);
    }

    [Fact]
    public void OppositeSide_WhenInvalid_ThrowsException()
    {
        var act = () => BoardSide.None.OppositeSide();

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("side");
    }
}
