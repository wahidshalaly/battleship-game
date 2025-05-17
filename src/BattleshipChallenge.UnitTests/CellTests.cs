using System;
using FluentAssertions;
using Xunit;

namespace BattleshipChallenge.UnitTests;

public class CellTests
{
    [Theory]
    [MemberData(nameof(ValidCells))]
    public void Ctor_WhenCreatedWithValidValue(char letter, int digit)
    {
        var cell = (Cell)$"{letter}{digit}";
        cell.Letter.Should().Be(letter);
        cell.Digit.Should().Be(digit);
    }

    [Theory]
    [MemberData(nameof(InvalidCells))]
    public void Ctor_WhenCreatedWithInvalidValue_ThrowsArgumentException(string code)
    {
        var act = () => (Cell)code;
        act
            .Should()
            .Throw<ArgumentException>()
            .WithMessage(ErrorMessages.InvalidCellCode);
    }

    [Fact]
    public void Ctor_SetsStateToClear()
    {
        var cell = (Cell)"A1";

        cell.State.Should().Be(CellState.Clear);
    }

    [Fact]
    public void Assign_WhenCellIsClear_SetsStateToOccupied()
    {
        var cell = (Cell)"A1";

        cell.Assign(1);

        cell.ShipId.Should().Be(1);
        cell.State.Should().Be(CellState.Occupied);
    }

    [Fact]
    public void Assign_WhenShipIdIsLessThanOrEqualZero_ThrowsException()
    {
        var cell = (Cell)"A1";

        Action act = () => cell.Assign(-2);

        act
            .Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("shipId");
    }

    [Fact]
    public void Assign_WhenCellIsOccupied_ThrowsException()
    {
        var cell = (Cell)"A1";
        cell.Assign(1);

        Action act = () => cell.Assign(2);

        act
            .Should()
            .Throw<ApplicationException>()
            .WithMessage(ErrorMessages.InvalidCellToAssign);
    }


    [Fact]
    public void Attack_WhenCellIsNotHit_SetsStateToHit()
    {
        var cell = (Cell)"A1";

        cell.Attack();

        cell.State.Should().Be(CellState.Hit);
    }

    [Fact]
    public void Attack_WhenCellIsHit_ThrowsException()
    {
        var cell = (Cell)"A1";
        cell.Attack();

        Action act = () => cell.Attack();

        act
            .Should()
            .Throw<ApplicationException>()
            .WithMessage(ErrorMessages.InvalidCellToHit);
    }

    public static TheoryData<char, int> ValidCells
    {
        get
        {
            var data = new TheoryData<char, int>();

            foreach (var letter in Constants.Alphabet)
            {
                for (var digit = 1; digit <= Constants.MaxBoardSize; digit++)
                {
                    data.Add(letter, digit);
                }
            }

            return data;
        }
    }

    public static TheoryData<string> InvalidCells => ["A0", "A27", "a1", "", null];
}