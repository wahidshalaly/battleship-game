using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace BattleshipChallenge.UnitTests;

public class CellLocatorTests
{
    private readonly CellLocator _subject = new();

    [Fact]
    public void FindCells_WhenBowAndSternAreEqual_ReturnsBow()
    {
        var bow = (Cell)"C5";

        var cells = _subject.FindCellsBetween(bow, bow).ToArray();

        cells.Should().BeEquivalentTo([bow]);
    }

    [Theory]
    [InlineData("C5", "C8")]
    [InlineData("C8", "C5")]
    public void FindCells_WhenBowAndSternHasSameLetter_ReturnsCollection(
        string bow,
        string stern
    )
    {
        var cells = _subject.FindCellsBetween((Cell)bow, (Cell)stern).ToArray();

        cells.Should().BeEquivalentTo([(Cell)"C5", (Cell)"C6", (Cell)"C7", (Cell)"C8"]);
    }

    [Theory]
    [InlineData("C10", "C7")]
    [InlineData("C7", "C10")]
    public void FindCells_WhenBowOrSternInColumn10(string bow, string stern)
    {
        var cells = _subject.FindCellsBetween((Cell)bow, (Cell)stern);

        cells.Should().BeEquivalentTo([(Cell)"C7", (Cell)"C8", (Cell)"C9", (Cell)"C10"]);
    }

    [Theory]
    [InlineData("C5", "E5")]
    [InlineData("E5", "C5")]
    public void FindCells_WhenBowAndSternHasSameDigit_ReturnsCollection(
        string bow,
        string stern
    )
    {
        var cells = _subject.FindCellsBetween((Cell)bow, (Cell)stern);

        cells.Should().BeEquivalentTo([(Cell)"C5", (Cell)"D5", (Cell)"E5"]);
    }

    [Fact]
    public void GetAllCellsOnBoardOf_WhenSizeIsValid_ReturnsCorrectCells()
    {
        var cells = _subject.GetAllCellsOnBoardOf(2).ToArray();

        cells.Should().BeEquivalentTo([(Cell)"A1", (Cell)"A2", (Cell)"B1", (Cell)"B2"]);
    }

    [Fact]
    public void FindCells_WhenInputIsNull_ThrowsException()
    {
        _subject.Invoking(s => s.FindCellsBetween(null, (Cell)"A1").ToArray())
            .Should()
            .Throw<ArgumentNullException>();

        _subject.Invoking(s => s.FindCellsBetween((Cell)"A1", null).ToArray())
            .Should()
            .Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(27)]
    public void GetAllCellsOnBoardOf_WhenSizeIsInvalid_ThrowsException(int size)
    {
        _subject.Invoking(s => s.GetAllCellsOnBoardOf(size).ToArray())
            .Should()
            .Throw<ArgumentException>();
    }
}