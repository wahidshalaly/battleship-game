using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace BattleshipChallenge.UnitTests;

public class ShipTests
{
    private const int ShipId = 3;

    [Theory]
    [MemberData(nameof(ShipKinds))]
    public void Ctor_WhenCellsCountMatchShipKind(ShipKind kind)
    {
        List<string> position = (int)kind switch
        {
            2 => ["A1", "A2"],
            3 => ["A1", "A2", "A3"],
            4 => ["A1", "A2", "A3", "A4"],
            5 => ["A1", "A2", "A3", "A4", "A5"],
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };

        var subject = new Ship(ShipId, kind, position);
        subject.Id.Should().Be(ShipId);
        subject.Kind.Should().Be(kind);
        subject.Position.Should().BeEquivalentTo(position);
    }

    [Fact]
    public void Ctor_WhenPositionsCountDoesNotMatchShipSize_ThrowsException()
    {
        List<string> position = ["A1"];

        var act = () => new Ship(ShipId, ShipKind.Cruiser, position);

        act.Should()
            .Throw<ApplicationException>()
            .WithMessage(ErrorMessages.InvalidShipPosition_Count);
    }

    [Fact]
    public void Ctor_WhenCellsDoesNotHaveTheSameColumnOrRow_ThrowsException()
    {
        List<string> position = ["A1", "B2", "C3", "A4"];
        var act = () => new Ship(ShipId, ShipKind.Battleship, position);
        act.Should()
            .Throw<ApplicationException>()
            .WithMessage(ErrorMessages.InvalidShipPosition_Alignment);
    }

    [Theory]
    [MemberData(nameof(NonadjacentPosition))]
    public void Ctor_WhenCellsAreNonadjacent_ThrowsException(List<string> position)
    {
        var act = () => new Ship(ShipId, ShipKind.Cruiser, position);
        act.Should()
            .Throw<ApplicationException>()
            .WithMessage(ErrorMessages.InvalidShipPosition_Alignment);
    }

    [Fact]
    public void Attack_WhenNotAllCellsAttacked_SunkIsFalse()
    {
        var subject = new Ship(ShipId, ShipKind.Battleship, ["A1", "A2", "A3", "A4"]);

        subject.Attack("A1");
        subject.Attack("A2");
        subject.Attack("A3");

        subject.Sunk.Should().BeFalse();
    }

    [Fact]
    public void Attack_WhenAllCellsAttacked_SunkIsTrue()
    {
        var subject = new Ship(ShipId, ShipKind.Battleship, ["A1", "A2", "A3", "A4"]);

        subject.Attack("A1");
        subject.Attack("A2");
        subject.Attack("A3");
        subject.Attack("A4");

        subject.Sunk.Should().BeTrue();
    }

    public static TheoryData<ShipKind> ShipKinds =>
        [
            ShipKind.Destroyer,
            ShipKind.Cruiser,
            ShipKind.Submarine,
            ShipKind.Battleship,
            ShipKind.Carrier,
        ];

    public static TheoryData<List<string>> NonadjacentPosition =>
        [
            ["A1", "A3", "A4"],
            ["A1", "B1", "D1"],
        ];
}
