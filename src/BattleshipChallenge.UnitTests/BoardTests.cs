using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace BattleshipChallenge.UnitTests
{
    [TestFixture]
    public class BoardTests
    {
        [Test]
        public void Ctor_InitiatesAllPositionsOnBoard()
        {
            var translator = new LocationTranslator();

            var subject = new Board(translator);

            subject.Positions.Distinct().Should().HaveCount(100);
            subject.Positions.ContainsKey("A1").Should().BeTrue();
            subject.Positions.ContainsKey("J10").Should().BeTrue();
            subject.Ships.Should().BeEmpty();
            subject.Attacks.Should().BeEmpty();
        }

        [TestCase("D4", "D4")]
        [TestCase("D4", "D7")]
        [TestCase("D7", "D4")]
        [TestCase("A4", "D4")]
        public void AddShip_WhenLocationIsValid_AddsShipToBoard(string bow, string stern)
        {
            var translator = new LocationTranslator();
            var expectedPositions = translator.FindPositions(bow, stern).ToArray();
            var subject = new Board(translator);

            subject.AddShip(bow, stern);

            // validate that ship has been successfully created
            subject.Ships.Should().HaveCount(1);
            subject.Ships[0].Id.Should().Be(1);
            subject.Ships[0].Positions.Should().BeEquivalentTo(expectedPositions);

            // validate that board has marked the ship location
            subject.Positions
                .Count(p => expectedPositions.Contains(p.Key) && p.Value == 1)
                .Should().Be(expectedPositions.Length);
        }

        [Test]
        public void AddShip_WhenAddingMultipleShips()
        {
            var ships = new[]
            {
                new Tuple<string, string>("C4", "C6"),
                new Tuple<string, string>("J10", "G10"),
                new Tuple<string, string>("F8", "G8")
            };
            var translator = new LocationTranslator();
            var subject = new Board(translator);
            var count = 0;

            foreach (var (bow, stern) in ships)
            {
                subject.AddShip(bow , stern);

                var expectedPositions = translator.FindPositions(bow, stern).ToArray();
                var shipId = count + 1;

                // validate that ship has been successfully created
                subject.Ships[count].Id.Should().Be(shipId);
                subject.Ships[count].Positions.Should().BeEquivalentTo(expectedPositions);

                // validate that board has marked the ship location
                subject.Positions
                    .Count(p => expectedPositions.Contains(p.Key) && p.Value == shipId)
                    .Should().Be(expectedPositions.Length);

                count++;
            }

            // validate that ship has been successfully created
            subject.Ships.Should().HaveCount(3);
            subject.Positions.Where(p => p.Value == 1).Should().HaveCount(3);
            subject.Positions.Where(p => p.Value == 2).Should().HaveCount(4);
            subject.Positions.Where(p => p.Value == 3).Should().HaveCount(2);
            subject.Positions.Values.Where(v => v != null).Should().HaveCount(9);
        }

        [TestCase("A1", "J10")]
        [TestCase("D4", "C7")]
        public void AddShip_WhenLocationIsInvalid_ThrowsException(string bow, string stern)
        {
            var translator = new LocationTranslator();
            var subject = new Board(translator);

            subject.Invoking(s => s.AddShip(bow, stern))
                .Should()
                .Throw<ArgumentException>()
                .WithMessage(Board.Constants.ErrorMsg_InvalidShipPosition);
        }

        [TestCase("D40", "C40")]
        public void AddShip_WhenLocationOutsideOfBoard_ThrowsException(string bow, string stern)
        {
            var translator = new LocationTranslator();
            var subject = new Board(translator);

            subject.Invoking(s => s.AddShip(bow, stern))
                .Should()
                .Throw<ArgumentException>()
                .WithMessage(Board.Constants.ErrorMsg_InvalidPositionOutOfRange);
        }

    }
}