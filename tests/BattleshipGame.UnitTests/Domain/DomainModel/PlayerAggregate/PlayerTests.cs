using System;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;

namespace BattleshipGame.UnitTests.Domain.DomainModel.PlayerAggregate;

public class PlayerTests
{
    private const string ValidUsername = "TestPlayer";

    private readonly PlayerId _playerId = new(Guid.NewGuid());

    [Fact]
    public void Ctor_WhenValidParameters_ShouldCreatePlayer()
    {
        var player = new Player(_playerId, ValidUsername, "auth|subject");

        player.Id.Should().Be(_playerId);
        player.Username.Should().Be(ValidUsername);
        player.IdentitySubject.Should().Be("auth|subject");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_WhenInvalidUsername_ShouldThrowArgumentException(string? invalidUsername)
    {
        var act = () => new Player(_playerId, invalidUsername!, "auth|subject");

        const string exceptionMessage = "Username cannot be null or whitespace.*";
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage(exceptionMessage)
            .And.ParamName.Should()
            .Be("username");
    }
}
