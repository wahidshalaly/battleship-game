using System;
using BattleshipGame.Common;
using BattleshipGame.Domain;
using BattleshipGame.Domain.AggregateRoots;
using BattleshipGame.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BattleshipGame.UnitTests.Domain;

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

        cell.Assign(1);

        cell.ShipId.Should().Be(1);
        cell.State.Should().Be(CellState.Occupied);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Assign_WhenShipIdIsLessThanOrEqualZero_ThrowsException(int id)
    {
        var cell = new Cell('A', 1);

        Action act = () => cell.Assign(id);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("shipId");
    }

    [Fact]
    public void Assign_WhenCellIsOccupied_ThrowsException()
    {
        var cell = new Cell('A', 1);
        cell.Assign(1);

        Action act = () => cell.Assign(2);

        act.Should().Throw<ApplicationException>().WithMessage(ErrorMessages.InvalidCellToAssign);
    }

    [Fact]
    public void Attack_WhenCellIsNotHit_SetsStateToHit()
    {
        var cell = new Cell('A', 1);

        cell.Attack();

        cell.State.Should().Be(CellState.Hit);
    }

    [Fact]
    public void Attack_WhenCellIsHit_ThrowsException()
    {
        var cell = new Cell('A', 1);
        cell.Attack();

        Action act = () => cell.Attack();

        act.Should().Throw<ApplicationException>().WithMessage(ErrorMessages.InvalidCellToAttack);
    }

    public static TheoryData<char, int, string> ValidCells
    {
        get
        {
            var data = new TheoryData<char, int, string>();

            foreach (var letter in Constants.ColumnHeaders)
            {
                for (var digit = 1; digit <= Board.MaximumSize; digit++)
                {
                    var code = $"{letter}{digit}";
                    data.Add(letter, digit, code);
                }
            }

            return data;
        }
    }

    public static TheoryData<string> InvalidCells => ["A0", "A27", "a1", "", null];
}
