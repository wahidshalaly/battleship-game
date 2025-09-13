using System;
using BattleshipGame.Domain.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using FluentAssertions;
using Xunit;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.UnitTests.Domain.DomainModel.GameAggregate;

public class CellTests
{
    [Theory]
    [MemberData(nameof(ValidCells))]
    public void Ctor_WhenCreatedWithValidValue(char letter, int digit, string expectedCode)
    {
        var cell = new Cell(letter, digit);
        cell.Letter.Should().Be(letter);
        cell.Digit.Should().Be(digit);
        cell.Code.Should().Be(expectedCode);
    }

    [Theory]
    [MemberData(nameof(InvalidCells))]
    public void Ctor_WhenCreatedWithInvalidValue_ThrowsException(string code)
    {
        var act = () => Cell.FromCode(code);

        act.Should().Throw<ArgumentException>().WithMessage(ErrorMessages.InvalidCellCode);
    }

    [Fact]
    public void Ctor_SetsStateToClear()
    {
        var cell = new Cell('A', 1);

        cell.State.Should().Be(CellState.Clear);
    }

    [Fact]
    public void Assign_WhenCellIsClear_SetsStateToOccupied()
    {
        var cell = new Cell('A', 1);
        var shipId = Guid.NewGuid();

        cell.Assign(new ShipId(shipId));

        cell.ShipId.Should().NotBe(Guid.Empty);
        cell.State.Should().Be(CellState.Occupied);
    }

    [Fact]
    public void Assign_WhenCellIsOccupied_ThrowsException()
    {
        var cell = new Cell('A', 1);
        var shipId = Guid.NewGuid();
        cell.Assign(new ShipId(shipId));

        var shipId2 = Guid.NewGuid();
        Action act = () => cell.Assign(new ShipId(shipId2));

        act.Should().Throw<InvalidOperationException>().WithMessage(ErrorMessages.InvalidCellToAssign);
    }

    [Fact]
    public void Attack_WhenCellIsClear_SetsStateToMissed()
    {
        var cell = new Cell('A', 1);

        cell.Attack();

        cell.State.Should().Be(CellState.Missed);
    }

    [Fact]
    public void Attack_WhenCellIsOccupied_SetsStateToHit()
    {
        var cell = new Cell('A', 1);
        var shipId = new ShipId(Guid.NewGuid());
        cell.Assign(shipId);

        cell.Attack();

        cell.State.Should().Be(CellState.Hit);
    }

    [Fact]
    public void Attack_WhenCellIsHit_ThrowsException()
    {
        var cell = new Cell('A', 1);
        cell.Attack();

        Action act = () => cell.Attack();

        act.Should().Throw<InvalidOperationException>().WithMessage(ErrorMessages.InvalidCellToAttack);
    }

    public static TheoryData<char, int, string> ValidCells
    {
        get
        {
            var data = new TheoryData<char, int, string>();

            foreach (var letter in ColumnHeaders)
            {
                for (var digit = 1; digit <= MaximumBoardSize; digit++)
                {
                    var code = $"{letter}{digit}";
                    data.Add(letter, digit, code);
                }
            }

            return data;
        }
    }

    public static TheoryData<string> InvalidCells => ["A0", "A27", "a1", "", null!];
}
