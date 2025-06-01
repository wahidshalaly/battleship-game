using System;
using System.Collections.Generic;
using BattleshipGame.Common;
using BattleshipGame.Domain;
using BattleshipGame.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain;

public class ShipTests
{
    private const int ShipId = 3;

    [Theory]
    [MemberData(nameof(ShipKinds))]
    public void Ctor_WhenCellsCountMatchShipKind(ShipKind kind)
    {
        List<string> position = kind.ToSize() switch
        {
            2 => ["A1", "A2"],
            3 => ["A1", "A2", "A3"],
            4 => ["A1", "A2", "A3", "A4"],
            5 => ["A1", "A2", "A3", "A4", "A5"],
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };

        var ship = new Ship(ShipId, kind, position);
        ship.Id.Should().Be(ShipId);
        ship.Kind.Should().Be(kind);
        ship.Position.Should().BeEquivalentTo(position);
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
        var ship = new Ship(ShipId, ShipKind.Battleship, ["A1", "A2", "A3", "A4"]);

        ship.Attack("A1");
        ship.Attack("A2");
        ship.Attack("A3");

        ship.Sunk.Should().BeFalse();
    }

    [Fact]
    public void Attack_WhenAllCellsAttacked_SunkIsTrue()
    {
        var ship = new Ship(ShipId, ShipKind.Battleship, ["A1", "A2", "A3", "A4"]);

        ship.Attack("A1");
        ship.Attack("A2");
        ship.Attack("A3");
        ship.Attack("A4");

        ship.Sunk.Should().BeTrue();
    }

    [Fact]
    public void Attack_WhenCellDoesNotBelogTofShip_ThrowsException()
    {
        var ship = new Ship(ShipId, ShipKind.Battleship, ["A1", "A2", "A3", "A4"]);

        var act = () => ship.Attack("C5");

        act.Should().Throw<ApplicationException>().WithMessage(ErrorMessages.InvalidShipAttack);
    }

    [Fact]
    public void Attack_WhenCellIsAlreadyHit_ThrowsException()
    {
        var ship = new Ship(ShipId, ShipKind.Battleship, ["A1", "A2", "A3", "A4"]);
        ship.Attack("A1");

        var act = () => ship.Attack("A1");

        act.Should().Throw<ApplicationException>().WithMessage(ErrorMessages.InvalidCellToAttack);
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
