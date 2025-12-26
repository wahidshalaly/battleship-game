using BattleshipGame.Application.Exceptions;
using BattleshipGame.Domain.Common;
using BattleshipGame.Domain.DomainModel.GameAggregate.Events;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.SharedKernel;
using static BattleshipGame.Domain.Common.Constants;

namespace BattleshipGame.Domain.DomainModel.GameAggregate;

/// <summary>
/// Represents the unique identifier for a game.
/// </summary>
/// <remarks>This type encapsulates a <see cref="Guid"/> value to uniquely identify a game entity. It inherits
/// from <see code="EntityId"/> to provide additional context or functionality specific to entity
/// identification.</remarks>
/// <param name="Value"></param>
public record GameId(Guid Value) : EntityId(Value);

/// <summary>
/// This represents an instance of the Battleship game, and it tracks the state of the game.
/// </summary>
public sealed class Game(PlayerId playerId, int boardSize = DefaultBoardSize)
    : AggregateRoot<GameId>
{
    private readonly Board _ownBoard = new(boardSize);
    private readonly Board _oppBoard = new(boardSize);

    public PlayerId PlayerId { get; } = playerId;

    public int BoardSize { get; } = boardSize;

    public GameState State { get; private set; } = GameState.Started;

    public BoardSide CurrentTurn { get; private set; } = BoardSide.None;

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

        if (BoardSelector(side).IsReady)
        {
            AddDomainEvent(new BoardReadyEvent(Id, side));
        }

        // Check if both boards are now ready and raise event
        if (IsReady)
        {
            State = GameState.Ready;
            AddDomainEvent(new GameReadyEvent(Id));
        }

        return shipId;
    }

    /// <summary>
    /// Attacks a cell on the specified boardSide's board
    /// </summary>
    /// <param name="boardSide">The boardSide whose board to attack</param>
    /// <param name="cellCode">The cell to attack</param>
    /// <returns>True if the attack hit a ship, false otherwise</returns>
    public CellState Attack(BoardSide boardSide, string cellCode)
    {
        var board = BoardSelector(boardSide);
        var (cellState, shipId, shipSunk) = board.Attack(cellCode);

        // Raise domain event for cell attack
        AddDomainEvent(new UnderAttackEvent(Id, cellCode, cellState));

        if (cellState != CellState.Hit)
        {
            return cellState;
        }

        // Check if the cell belongs to a ship that was sunk
        if (shipId is not null && shipSunk)
        {
            // Raise domain event if a ship was sunk
            AddDomainEvent(new ShipSunkEvent(Id, shipId, boardSide));
        }

        // Check if the game is over for the attacked boardSide
        if (IsGameOver(boardSide))
        {
            // Raise domain event if a game is over
            State = GameState.GameOver;
            AddDomainEvent(new GameOverEvent(Id, boardSide));
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
    public bool IsReady => IsBoardReady(BoardSide.Player) && IsBoardReady(BoardSide.Opponent);

    /// <summary>
    /// Gets the available cell codes for the specified boardSide
    /// </summary>
    /// <param name="boardSide">The boardSide whose available cell codes to get</param>
    /// <returns>The available cell codes for the specified boardSide</returns>
    public IReadOnlyCollection<string> GetAvailableCellCodes(BoardSide boardSide)
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
    public IReadOnlyCollection<ShipId> GetShips(BoardSide boardSide)
    {
        return BoardSelector(boardSide).Ships.Select(s => s.Id).ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the position of the specified ship on the specified boardSide
    /// </summary>
    /// <param name="boardSide">The boardSide whose ship position to get</param>
    /// <param name="shipId">The identifier of the ship whose position to get</param>
    /// <returns>The position of the specified ship on the specified boardSide</returns>
    public IReadOnlyCollection<string> GetShipPosition(BoardSide boardSide, ShipId shipId)
    {
        return BoardSelector(boardSide).Ships.First(s => s.Id == shipId).Position;
    }

    /// <summary>
    /// Starts the gameplay for a game that is ready.
    /// </summary>
    /// <returns>A completed task.</returns>
    /// <exception cref="GameNotReadyException">Thrown when the game is not ready to start gameplay.</exception>
    /// <remarks> Initializes gameplay by transitioning the game state to 'GameOn' and raising a
    /// <see cref="GameStartedEvent"/> domain event. </remarks>
    public void StartGameplay()
    {
        if (State != GameState.Ready || !IsReady)
        {
            throw new GameNotReadyException(Id);
        }

        State = GameState.GameOn;
        CurrentTurn = BoardSide.Player;

        AddDomainEvent(new GameStartedEvent(Id));
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
}
