using BattleshipGame.Application.Exceptions;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate.Events;
using BattleshipGame.SharedKernel;

namespace BattleshipGame.Domain.DomainModel.PlayerAggregate;

/// <summary>
/// Represents the unique identifier for a player.
/// </summary>
/// <remarks>This type encapsulates a  value to uniquely identify a game entity.
/// It inherits from to provide additional context or functionality specific to entity identification.</remarks>
/// <param name="Value"></param>
public record PlayerId(Guid Value) : EntityId(Value);

/// <summary>
/// Represents a Battleship game player, including their identity, active game, and gameplay history.
/// </summary>
/// <remarks>This class encapsulates the player's identity, their currently active game (if any), and a history
/// of games they have participated in. It includes business methods for game participation and validation.</remarks>
public sealed class Player : AggregateRoot<PlayerId>
{
    private readonly List<GameId> _gameHistory = [];

    /// <summary>
    /// Initializes a new instance of the Player class.
    /// </summary>
    /// <param name="id">The unique identifier for the player.</param>
    /// <param name="username">The player's username.</param>
    /// <param name="activeGameId">The currently active game identifier, if any.</param>
    public Player(PlayerId id, string username, GameId? activeGameId = null)
        : base(id.Value)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or whitespace.", nameof(username));

        Username = username;
        ActiveGameId = activeGameId;
    }

    /// <summary>
    /// Gets the player's username.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Gets the currently active game identifier, if any.
    /// </summary>
    public GameId? ActiveGameId { get; private set; }

    /// <summary>
    /// Gets the read-only collection of game history.
    /// </summary>
    public IReadOnlyList<GameId> GameHistory => _gameHistory.AsReadOnly();

    /// <summary>
    /// Checks if the player is currently in an active game.
    /// </summary>
    public bool IsInActiveGame => ActiveGameId is not null;

    /// <summary>
    /// Joins a game by setting the active game identifier.
    /// </summary>
    /// <param name="gameId">The game identifier to join.</param>
    /// <exception cref="InvalidOperationException">Thrown when player is already in an active game.</exception>
    public void JoinGame(GameId gameId)
    {
        if (IsInActiveGame)
        {
            throw new PlayerIsInActiveException(Id);
        }

        ActiveGameId = gameId;
        AddDomainEvent(new PlayerJoinedGameEvent(Id, gameId, Username));
    }

    /// <summary>
    /// Leaves the current active game and adds it to game history.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when player is not in an active game.</exception>
    public void LeaveGame()
    {
        if (!IsInActiveGame)
        {
            throw new PlayerIsNotInActiveException(Id);
        }

        var gameId = ActiveGameId!;
        _gameHistory.Add(gameId);
        ActiveGameId = null;

        AddDomainEvent(new PlayerLeftGameEvent(Id, gameId, Username));
    }

    /// <summary>
    /// Gets the total number of games the player has participated in.
    /// </summary>
    public int TotalGamesPlayed => _gameHistory.Count + (IsInActiveGame ? 1 : 0);
}
