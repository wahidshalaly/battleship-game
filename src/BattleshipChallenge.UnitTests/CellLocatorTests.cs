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
        const string bow = "C5";

        var cells = _subject.FindCells(bow, bow);

        cells.Should().BeEquivalentTo(bow);
    }

    [Theory]
    [InlineData("C5", "C8")]
    [InlineData("C8", "C5")]
    public void FindCells_WhenBowAndSternHasSameLetter_ReturnsCollection(
        string bow,
        string stern
    )
    {
        var cells = _subject.FindCells(bow, stern);

        cells.Should().BeEquivalentTo("C5", "C6", "C7", "C8");
    }

    [Theory]
    [InlineData("C10", "C7")]
    [InlineData("C7", "C10")]
    public void FindCells_WhenBowOrSternInColumn10(string bow, string stern)
    {
        var cells = _subject.FindCells(bow, stern);

        cells.Should().BeEquivalentTo("C7", "C8", "C9", "C10");
    }

    [Theory]
    [InlineData("C5", "E5")]
    [InlineData("E5", "C5")]
    public void FindCells_WhenBowAndSternHasSameDigit_ReturnsCollection(
        string bow,
        string stern
    )
    {
        var cells = _subject.FindCells(bow, stern);

        cells.Should().BeEquivalentTo("C5", "D5", "E5");
    }

    [Fact]
    public void GetAllCellsOnBoardOf_WhenSizeIsValid_ReturnsCorrectCells()
    {
        var cells = _subject.GetAllCellsOnBoardOf(2).ToArray();

        cells.Should().BeEquivalentTo([(Cell)"A1", (Cell)"A2", (Cell)"B1", (Cell)"B2"]);
    }

    //Bug #1
    [Theory]
    [InlineData("C-1")]
    [InlineData("Z99")]
    [InlineData("1-C")]
    public void FindCells_WhenInputIsInvalid_ThrowsException(string cell)
    {
        _subject.Invoking(s => s.FindCells(cell, "A1").ToArray())
            .Should()
            .Throw<ArgumentException>();

        _subject.Invoking(s => s.FindCells("A1", cell).ToArray())
            .Should()
            .Throw<ArgumentException>();
    }

    //Bug #2
    [Theory]
    [InlineData(0)]
    public void GetAllCellsOnBoardOf_WhenSizeIsInvalid_ThrowsException(int size)
    {
        _subject.Invoking(s => s.GetAllCellsOnBoardOf(size).ToArray())
            .Should()
            .Throw<ArgumentException>();
    }
}