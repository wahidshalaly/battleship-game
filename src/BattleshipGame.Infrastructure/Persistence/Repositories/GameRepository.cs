using BattleshipGame.Application.Interfaces.Persistence;
using BattleshipGame.Domain.DomainModel.GameAggregate;
using BattleshipGame.Domain.DomainModel.PlayerAggregate;
using BattleshipGame.Domain.Exceptions;
using BattleshipGame.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BattleshipGame.Infrastructure.Persistence.Repositories;

internal class GameRepository(BattleshipGameDbContext context) : IGameRepository
{
    public async Task<Game?> GetByIdAsync(GameId gameId, CancellationToken ct)
    {
        var entity = await context.Games.FirstOrDefaultAsync(g => g.Id == gameId.Value, ct);
        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<Game> GetByIdOrThrowAsync(GameId gameId, CancellationToken ct)
    {
        var entity =
            await context.Games.FirstOrDefaultAsync(g => g.Id == gameId.Value, ct)
            ?? throw new GameNotFoundException(gameId);
        return MapToDomain(entity);
    }

    public async Task SaveAsync(Game game, CancellationToken ct)
    {
        var existing = await context.Games.FirstOrDefaultAsync(g => g.Id == game.Id.Value, ct);

        if (existing is null)
        {
            await context.Games.AddAsync(MapToEntity(game), ct);
        }
        else
        {
            UpdateExistingEntity(game, existing);
        }
    }

    public async Task<IReadOnlyCollection<Game>> GetByPlayerIdAsync(
        PlayerId playerId,
        CancellationToken ct
    )
    {
        var entities = await context.Games.Where(g => g.PlayerId == playerId.Value).ToListAsync(ct);

        return entities.Select(MapToDomain).ToList().AsReadOnly();
    }

    public async Task<Game?> GetActiveGameByPlayerIdAsync(PlayerId playerId, CancellationToken ct)
    {
        var entity = await context
            .Games.Where(g => g.PlayerId == playerId.Value && g.State != (int)GameState.GameOver)
            .OrderBy(g => g.CreatedAt)
            .LastOrDefaultAsync(ct);

        return entity is null ? null : MapToDomain(entity);
    }

    private static Game MapToDomain(GameEntity entity) =>
        Game.Reconstitute(
            id: new GameId(entity.Id),
            playerId: new PlayerId(entity.PlayerId),
            boardSize: entity.BoardSize,
            strategy: (OpponentStrategy)entity.OpponentStrategy,
            state: (GameState)entity.State,
            targetSide: (BoardSide)entity.TargetSide,
            winnerSide: (BoardSide)entity.WinnerSide,
            createdAt: entity.CreatedAt,
            lastUpdatedAt: entity.LastUpdatedAt,
            ownBoard: MapBoardToDomain(entity.OwnBoard, entity.BoardSize),
            oppBoard: MapBoardToDomain(entity.OppBoard, entity.BoardSize)
        );

    private static Board MapBoardToDomain(BoardJson boardJson, int boardSize)
    {
        var ships = boardJson
            .Ships.Select(s =>
                Ship.Reconstitute(
                    id: new ShipId(s.Id),
                    kind: (ShipKind)s.Kind,
                    codes: s.Codes,
                    hits: s.Hits
                )
            )
            .ToList();

        var cells = boardJson.Cells.Select(c =>
        {
            var (letter, digit) = Cell.FromCode(c.Code);
            var cell = new Cell(letter, digit);
            ShipId? shipId = c.ShipId.HasValue ? new ShipId(c.ShipId.Value) : null;
            cell.Reconstitute(shipId, (CellState)c.State);
            return cell;
        });

        return Board.Reconstitute(
            id: new BoardId(boardJson.Id),
            boardSize: boardSize,
            cells: cells,
            ships: ships
        );
    }

    private static GameEntity MapToEntity(Game game) =>
        new()
        {
            Id = game.Id.Value,
            PlayerId = game.PlayerId.Value,
            BoardSize = game.BoardSize,
            OpponentStrategy = (int)game.OpponentStrategy,
            State = (int)game.State,
            TargetSide = (int)game.TargetSide,
            WinnerSide = (int)game.WinnerSide,
            CreatedAt = game.CreatedAt,
            LastUpdatedAt = game.LastUpdatedAt,
            OwnBoard = MapBoardToJson(game, BoardSide.Player),
            OppBoard = MapBoardToJson(game, BoardSide.Opponent),
        };

    private static void UpdateExistingEntity(Game game, GameEntity existing)
    {
        existing.State = (int)game.State;
        existing.TargetSide = (int)game.TargetSide;
        existing.WinnerSide = (int)game.WinnerSide;
        existing.LastUpdatedAt = game.LastUpdatedAt;
        existing.OwnBoard = MapBoardToJson(game, BoardSide.Player);
        existing.OppBoard = MapBoardToJson(game, BoardSide.Opponent);
    }

    private static BoardJson MapBoardToJson(Game game, BoardSide side)
    {
        var allHits = game.GetHits(side).ToHashSet();
        var allMisses = game.GetMisseds(side).ToHashSet();

        var ships = game.GetShips(side)
            .Select(shipId =>
            {
                var position = game.GetShipPosition(side, shipId);
                return new ShipJson
                {
                    Id = shipId.Value,
                    Kind = (int)game.GetShipKind(side, shipId),
                    Codes = position.ToList(),
                    Hits = allHits.Intersect(position).ToList(),
                };
            })
            .ToList();

        var cells = game.GetNextTargets(side)
            .Concat(allHits)
            .Concat(allMisses)
            .Select(code =>
            {
                var owningShipId = game.GetShips(side)
                    .FirstOrDefault(sid => game.GetShipPosition(side, sid).Contains(code));

                var state =
                    allHits.Contains(code) ? CellState.Hit
                    : allMisses.Contains(code) ? CellState.Missed
                    : owningShipId != default ? CellState.Occupied
                    : CellState.Clear;

                return new CellJson
                {
                    Code = code,
                    ShipId = owningShipId == default ? null : owningShipId.Value,
                    State = (int)state,
                };
            })
            .ToList();

        return new BoardJson
        {
            Id = game.GetBoardId(side),
            Ships = ships,
            Cells = cells,
        };
    }
}
