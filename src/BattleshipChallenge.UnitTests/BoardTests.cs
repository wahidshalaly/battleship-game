using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace BattleshipChallenge.UnitTests
{
    [TestFixture]
    public class BoardTests
    {
        private readonly LocationTranslator _translator = new LocationTranslator();

        [Test]
        public void Ctor_InitiatesAllPositionsOnBoard()
        {
            var subject = CreateSubject();

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
            var expectedPositions = _translator.FindPositions(bow, stern).ToArray();
            var subject = CreateSubject();

            subject.AddShip(bow, stern);

            // validate that ship has been successfully created
            subject.Ships.Should().HaveCount(1);
            subject.Ships[0].Id.Should().Be(0);
            subject.Ships[0].Positions.Should().BeEquivalentTo(expectedPositions);

            // validate that board has marked the ship location by ship-id (0)
            subject.Positions
                .Count(p => expectedPositions.Contains(p.Key) && p.Value == 0)
                .Should().Be(expectedPositions.Length);
        }

        [Test]
        public void AddShip_WhenAddingMultipleShips()
        {
            var shipId = 0;
            var ships = new[]
            {
                new Tuple<string, string>("C4", "C6"),
                new Tuple<string, string>("J10", "G10"),
                new Tuple<string, string>("F8", "G8")
            };
            var subject = CreateSubject();

            foreach (var (bow, stern) in ships)
            {
                subject.AddShip(bow , stern);

                var expectedPositions = _translator.FindPositions(bow, stern).ToArray();

                // validate that ship has been successfully created
                subject.Ships[shipId].Id.Should().Be(shipId);
                subject.Ships[shipId].Positions.Should().BeEquivalentTo(expectedPositions);

                // validate that board has marked the ship location
                subject.Positions
                    .Count(p => expectedPositions.Contains(p.Key) && p.Value == shipId)
                    .Should().Be(expectedPositions.Length);

                shipId++;
            }

            // validate that ship has been successfully created
            subject.Ships.Should().HaveCount(3);
            subject.Positions.Where(p => p.Value == 0).Should().HaveCount(3);
            subject.Positions.Where(p => p.Value == 1).Should().HaveCount(4);
            subject.Positions.Where(p => p.Value == 2).Should().HaveCount(2);
            subject.Positions.Values.Where(v => v != null).Should().HaveCount(9);
        }

        [TestCase("A1", "J10")]
        [TestCase("D4", "C7")]
        public void AddShip_WhenLocationIsInvalid_ThrowsException(string bow, string stern)
        {
            var subject = CreateSubject();

            subject.Invoking(s => s.AddShip(bow, stern))
                .Should()
                .Throw<ArgumentException>()
                .WithMessage(Board.Constants.ErrorMsg_InvalidShipPosition);
        }

        [TestCase("D40", "C40")]
        public void AddShip_WhenLocationOutsideOfBoard_ThrowsException(string bow, string stern)
        {
            var subject = CreateSubject();

            subject.Invoking(s => s.AddShip(bow, stern))
                .Should()
                .Throw<ArgumentException>()
                .WithMessage(Board.Constants.ErrorMsg_InvalidPositionOutOfRange);
        }

        [Test]
        public void Attack_WhenPositionIsInvalid()
        {
            var subject = CreateSubject();

            subject.Invoking(s => s.Attack("C40"))
                .Should()
                .Throw<ArgumentException>()
                .WithMessage(Board.Constants.ErrorMsg_InvalidPositionOutOfRange);
        }

        [Test]
        public void Attack_WhenPositionIsValidButNotOccupied_ReturnFalse()
        {
            var subject = CreateSubject();

            var result = subject.Attack("C4");

            result.Should().BeFalse();
        }

        [Test]
        public void Attack_WhenPositionIsValidAndOccupied_ReturnTrue()
        {
            var subject = CreateSubject();
            subject.AddShip("C3", "C5");

            var result = subject.Attack("C4");

            result.Should().BeTrue();
            subject.Attacks.Should().HaveCount(1);
            subject.Ships[0].Damages.Should().HaveCount(1);
            subject.Ships[0].Sunk.Should().BeFalse();
        }

        [Test]
        public void Attack_WhenAllShipAttack_ShouldSink()
        {
            var subject = CreateSubject();
            subject.AddShip("C3", "C5");

            _ = subject.Attack("C3");
            _ = subject.Attack("C4");
            _ = subject.Attack("C5");

            subject.Attacks.Should().HaveCount(3);
            subject.Ships[0].Damages.Should().HaveCount(3);
            subject.Ships[0].Sunk.Should().BeTrue();
        }

        private Board CreateSubject()
        {
            return new Board(_translator);
        }
    }
}