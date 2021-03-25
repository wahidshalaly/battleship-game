using FluentAssertions;
using NUnit.Framework;

namespace BattleshipChallenge.UnitTests
{
    [TestFixture]
    public class LocationTranslatorTests
    {
        [Test]
        public void FindPositions_WhenBowAndSternAreEqual_ReturnsBow()
        {
            var bow = "C5";
            var subject = new LocationTranslator();

            var positions = subject.FindPositions(bow, bow);

            positions.Should().BeEquivalentTo(bow);
        }

        [TestCase("C5", "C8")]
        [TestCase("C8", "C5")]
        public void FindPositions_WhenBowAndSternHasSameLetter_ReturnsCollection(string bow, string stern)
        {
            var subject = new LocationTranslator();

            var positions = subject.FindPositions(bow, stern);

            positions.Should().BeEquivalentTo("C5", "C6", "C7", "C8");
        }


        [TestCase("C10", "C7")]
        [TestCase("C7", "C10")]
        public void FindPositions_WhenBowOrSternInColumn10(string bow, string stern)
        {
            var subject = new LocationTranslator();

            var positions = subject.FindPositions(bow, stern);

            positions.Should().BeEquivalentTo("C7", "C8", "C9", "C10");
        }

        [TestCase("C5", "E5")]
        [TestCase("E5", "C5")]
        public void FindPositions_WhenBowAndSternHasSameDigit_ReturnsCollection(string bow, string stern)
        {
            var subject = new LocationTranslator();

            var positions = subject.FindPositions(bow, stern);

            positions.Should().BeEquivalentTo("C5", "D5", "E5");
        }
    }
}