using BattleshipGame.Domain.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Domain.Exceptions;
using BattleshipGame.Domain.SharedKernel;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

/// <summary>
/// Represents the unique identifier for a game.
/// </summary>
/// <remarks>This type encapsulates a <see cref="Guid"/> value to uniquely identify a game entity. It inherits
/// from <see cref="EntityId"/> to provide additional context or functionality specific to entity
/// identification.</remarks>
/// <param name="Value">The underlying <see cref="Guid"/> value of the game identifier.</param>
public record GameId(Guid Value) : EntityId(Value);

/// <summary>
/// This represents an instance of the Battleship game, and it tracks the state of the game.
/// </summary>
public sealed class Game(
    PlayerId playerId,
    int boardSize = DefaultBoardSize,
    OpponentStrategy strategy = OpponentStrategy.Random
) : AggregateRoot<GameId>
{
    private readonly Board _ownBoard = new(boardSize);
    private readonly Board _oppBoard = new(boardSize);

    public PlayerId PlayerId { get; } = playerId;

    public int BoardSize { get; } = boardSize;

    /// <summary>
    /// Gets the computer opponent strategy configured for this game.
    /// </summary>
    public OpponentStrategy OpponentStrategy { get; } = strategy;

    public GameState State { get; private set; } = GameState.New;

    public BoardSide TargetSide { get; private set; } = BoardSide.None;

    /// <summary>
    /// Gets the side that won the game. Returns <see cref="BoardSide.None"/> if the game is not over.
    /// </summary>
    /// <remarks>
    /// This property is set when the game transitions to <see cref="GameState.GameOver"/> state.
    /// The winner is determined by which side has ships remaining.
    /// </remarks>
    public BoardSide WinnerSide { get; private set; } = BoardSide.None;

    /// <summary>
    /// Gets the UTC timestamp when the game was created.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the UTC timestamp when the game was last updated.
    /// </summary>
    /// <remarks>
    /// This property is updated whenever a ship is placed, an attack is made, or gameplay is started.
    /// </remarks>
    public DateTime LastUpdatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Places a ship on the specified boardSide's board
    /// </summary>
    /// <param name="side">The boardSide whose board to place the ship on</param>
    /// <param name="kind">The kind of ship to place</param>
    /// <param name="orientation">The orientation of the ship</param>
    /// <param name="bowCode">The cell code where the bow of the ship will be placed</param>
    /// <returns>The identifier of the placed ship</returns>
    public ShipId PlaceShip(
        BoardSide side,
        ShipKind kind,
        ShipOrientation orientation,
        string bowCode
    )
    {
        var board = BoardSelector(side);
        var shipId = board.PlaceShip(kind, orientation, bowCode);
        LastUpdatedAt = DateTime.UtcNow;

        if (BoardSelector(side).IsReady)
        {
            AddDomainEvent(new BoardReadyEvent(Id, side));
        }

        // Check if both boards are now ready and raise event
        if (AreBoardsReady)
        {
            State = GameState.Ready;
            AddDomainEvent(new GameReadyEvent(Id));
        }

        return shipId;
    }

    /// <summary>
    /// Attacks a cell on the specified boardSide's board
    /// </summary>
    /// <param name="targetSide">The boardSide to be attacked</param>
    /// <param name="cellCode">The cell to attack</param>
    /// <returns>True if the attack hit a ship, false otherwise</returns>
    public CellState Attack(BoardSide targetSide, string cellCode)
    {
        ValidateBeforeAttack(targetSide);

        var board = BoardSelector(targetSide);
        var (cellState, shipId, shipSunk) = board.Attack(cellCode);
        LastUpdatedAt = DateTime.UtcNow;

        // Raise domain event for cell attack
        AddDomainEvent(new UnderAttackEvent(Id, targetSide, cellCode, cellState));

        // Switch to the opposite board being under attack
        TargetSide = targetSide.OppositeSide();

        if (cellState != CellState.Hit)
        {
            return cellState;
        }

        // Check if the cell belongs to a ship that was sunk
        if (shipId is not null && shipSunk)
        {
            // Raise domain event if a ship was sunk
            AddDomainEvent(new ShipSunkEvent(Id, shipId, targetSide));
        }

        // Check if the game is over for the attacked boardSide
        if (IsGameOver(targetSide))
        {
            // Raise domain event if a game is over
            State = GameState.GameOver;
            WinnerSide = targetSide.OppositeSide();
            AddDomainEvent(new GameOverEvent(Id, WinnerSide));
        }

        return cellState;
    }

    /// <summary>
    /// Checks if the specified boardSide has lost the game
    /// </summary>
    /// <param name="boardSide">The boardSide to check</param>
    /// <returns>True if all boardSide's ships have been sunk, false otherwise</returns>
    public bool IsGameOver(BoardSide boardSide) => BoardSelector(boardSide).IsGameOver;

    /// <summary>
    /// Checks if the specified boardSide is ready
    /// </summary>
    /// <param name="boardSide">The boardSide to check</param>
    /// <returns>True if the boardSide is ready, false otherwise</returns>
    public bool IsBoardReady(BoardSide boardSide) => BoardSelector(boardSide).IsReady;

    /// <summary>
    /// Checks if both boards are ready
    /// </summary>
    /// <returns>True if both boards are ready, false otherwise</returns>
    public bool AreBoardsReady =>
        IsBoardReady(BoardSide.Player) && IsBoardReady(BoardSide.Opponent);

    /// <summary>
    /// Gets the available cell codes for the specified boardSide
    /// </summary>
    /// <param name="boardSide">The boardSide whose available cell codes to get</param>
    /// <returns>The available cell codes for the specified boardSide</returns>
    public IReadOnlyCollection<string> GetNextTargets(BoardSide boardSide)
    {
        return BoardSelector(boardSide)
            .Cells.Where(s => s.State is CellState.Clear or CellState.Occupied)
            .Select(s => s.Code)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the ships placed on the specified boardSide
    /// </summary>
    /// <param name="boardSide">The boardSide whose ships to get</param>
    /// <returns>The ships placed on the specified boardSide</returns>
    public IReadOnlyCollection<ShipId> GetShips(BoardSide boardSide) =>
        BoardSelector(boardSide).Ships.Select(s => s.Id).ToList().AsReadOnly();

    /// <summary>
    /// Gets the position of the specified ship on the specified boardSide
    /// </summary>
    /// <param name="boardSide">The boardSide whose ship position to get</param>
    /// <param name="shipId">The identifier of the ship whose position to get</param>
    /// <returns>The position of the specified ship on the specified boardSide</returns>
    public IReadOnlyCollection<string> GetShipPosition(BoardSide boardSide, ShipId shipId) =>
        BoardSelector(boardSide).Ships.First(s => s.Id == shipId).Position;

    /// <summary>
    /// Gets the kind of the specified ship on the specified boardSide
    /// </summary>
    /// <param name="boardSide">The boardSide whose ship kind to get</param>
    /// <param name="shipId">The identifier of the ship whose kind to get</param>
    /// <returns>The kind of the specified ship on the specified boardSide</returns>
    public ShipKind GetShipKind(BoardSide boardSide, ShipId shipId) =>
        BoardSelector(boardSide).Ships.First(s => s.Id == shipId).Kind;

    /// <summary>
    /// Gets all cells that were hit on the specified boardSide.
    /// </summary>
    /// <param name="boardSide">The boardSide to get hit cells from</param>
    /// <returns>List of cell codes that were hit</returns>
    public IReadOnlyCollection<string> GetHits(BoardSide boardSide)
    {
        return BoardSelector(boardSide)
            .Cells.Where(c => c.State == CellState.Hit)
            .Select(c => c.Code)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets all cells that were missed on the specified boardSide.
    /// </summary>
    /// <param name="boardSide">The boardSide to get missed cells from</param>
    /// <returns>List of cell codes that were missed</returns>
    public IReadOnlyCollection<string> GetMisseds(BoardSide boardSide)
    {
        return BoardSelector(boardSide)
            .Cells.Where(c => c.State == CellState.Missed)
            .Select(c => c.Code)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Starts the gameplay for a game that is ready.
    /// </summary>
    /// <returns>A completed task.</returns>
    /// <exception cref="GameNotReadyException">Thrown when the game is not ready to start gameplay.</exception>
    /// <remarks> Initializes gameplay by transitioning the game state to 'Started' and raising a
    /// <see cref="GameStartedEvent"/> domain event. </remarks>
    public void StartGameplay()
    {
        if (State != GameState.Ready)
        {
            throw new GameNotReadyException(Id);
        }

        State = GameState.Started;
        TargetSide = BoardSide.Opponent;
        LastUpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new GameStartedEvent(Id));
    }

    /// <summary>
    /// Gets the board coordinate range as a human-readable string.
    /// </summary>
    /// <remarks>
    /// For a 10×10 board, returns "A1 to J10".
    /// Useful for prompts and UI display.
    /// </remarks>
    /// <returns>A string representing the board range (e.g., "A1 to J10")</returns>
    public string GetBoardRange()
    {
        var lastColumn = ColumnHeaders[BoardSize - 1];
        return $"A1 to {lastColumn}{BoardSize}";
    }

    /// <summary>
    /// Gets the board for the specified side
    /// </summary>
    /// <param name="side">The side whose board to get</param>
    /// <returns>The side's board</returns>
    private Board BoardSelector(BoardSide side)
    {
        return side switch
        {
            BoardSide.Player => _ownBoard,
            BoardSide.Opponent => _oppBoard,
            BoardSide.None => throw new InvalidOperationException(ErrorMessages.InvalidBoardSide),
            _ => throw new ArgumentOutOfRangeException(
                nameof(side),
                side,
                ErrorMessages.InvalidBoardSide
            ),
        };
    }

    /// <summary>
    /// Validates that the game is in a valid state to perform an attack on the specified target side.
    /// </summary>
    /// <param name="targetSide">The side to be attacked</param>
    /// <exception cref="GameOverException">Thrown when the game is already over.</exception>
    /// <exception cref="GameNotStartedException">Thrown when the game is not in Started state.</exception>
    /// <exception cref="InvalidTargetSideException">Thrown when the target side does not match the expected side.</exception>
    /// </summary>
    private void ValidateBeforeAttack(BoardSide targetSide)
    {
        if (State == GameState.GameOver)
        {
            throw new GameOverException(Id);
        }

        if (State != GameState.Started)
        {
            throw new GameNotStartedException(Id, State);
        }

        if (TargetSide != targetSide)
        {
            throw new InvalidTargetSideException(Id.Value, TargetSide, targetSide);
        }
    }
}
