using BattleshipGame.Domain.SharedKernel;

namespace BattleshipGame.Domain.DomainModel.PlayerAggregate;

/// <summary>
/// Represents the unique identifier for a player.
/// </summary>
/// <remarks>This type encapsulates a Guid value to uniquely identify a game entity.
/// It inherits from EntityId to provide additional context or functionality specific to entity identification.</remarks>
/// <param name="Value"></param>
public record PlayerId(Guid Value) : EntityId(Value);

/// <summary>
/// Represents a Battleship game player's identity.
/// </summary>
/// <remarks>A player's active game and history are derived from the games that reference it.</remarks>
public sealed class Player : AggregateRoot<PlayerId>
{
    /// <summary>
    /// Initializes a new instance of the Player class.
    /// </summary>
    /// <param name="id">The unique identifier for the player.</param>
    /// <param name="username">The player's username.</param>
    /// <param name="identitySubject">The external identity provider subject (the token 'sub')
    /// that this player belongs to.</param>
    public Player(PlayerId id, string username, string identitySubject)
        : base(id.Value)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or whitespace.", nameof(username));

        if (string.IsNullOrWhiteSpace(identitySubject))
            throw new ArgumentException(
                "Identity subject cannot be null or whitespace.",
                nameof(identitySubject)
            );

        Username = username;
        IdentitySubject = identitySubject;
    }

    /// <summary>
    /// Gets the player's username.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Gets the external identity provider subject (the JWT 'sub' claim) this player belongs to.
    /// Correlates the authenticated caller to their game-domain profile.
    /// </summary>
    public string IdentitySubject { get; }
}
